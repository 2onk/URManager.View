using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using URManager.Backend.Data;
using URManager.Backend.JSON;
using URManager.Backend.Model;
using URManager.Backend.Net;
using URManager.Backend.ViewModel;
using URManager.View.Command;

namespace URManager.View.ViewModel
{
    public class RobotsViewModel : TabItems
    { 
        private readonly IRobotDataProvider _robotDataProvider;
        private RobotItemViewModel _selectedRobot;
        private int _currentRobotIndex = 0;


        public RobotsViewModel(object name, object icon, bool isClosable) : base(name, icon, isClosable)
        {
            _robotDataProvider = new RobotDataProvider();
            AddCommand = new DelegateCommand(Add);
            ExportJsonCommand = new DelegateCommand(ExportJsonAsync);
            ImportJsonCommand = new DelegateCommand(ImportJsonAsync);
            DeleteCommand = new DelegateCommand(Delete, CanDelete);
        }


        public ObservableCollection<RobotItemViewModel> Robots { get; } = new();

        public RobotItemViewModel SelectedRobot
        {
            get => _selectedRobot;
            set
            {
                if (value == _selectedRobot) return;
                _selectedRobot = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsRobotSelected));
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Get bool if any robot in list is selected
        /// </summary>
        public bool IsRobotSelected => SelectedRobot is not null;

        public DelegateCommand AddCommand { get; }

        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand ExportJsonCommand { get; }
        public DelegateCommand ImportJsonCommand { get; }

        /// <summary>
        /// Provides dummy robot data to listview
        /// </summary>
        /// <returns></returns>
        public async override Task LoadAsync()
        {
            if (Robots.Any()) return;

            var robots = await _robotDataProvider.GetAll();
            if (robots is null) return;

            foreach (var robot in robots)
            {
                Robots.Add(new RobotItemViewModel(robot));
                _currentRobotIndex++;
            }
        }

        /// <summary>
        /// Start Backup process with all selected robots. 
        /// Steps are: ping, connect to dashboard, create backup, connect to sftp, download file and delete, disconnect.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public async Task BackupProcessAsync(SettingsViewModel settings)
        {
            if(CheckRoutine(settings) is not true) return;

            //count successfull supportfile downloads
            int robotCounter = 0;

            foreach (var robot in Robots)
            {
                if (robot.IP is null || robot.Backup == false) continue;

                if (!CheckIP(robot.IP))
                {
                    settings.ItemLogger.InsertNewMessage($"{robot.RobotName}: {robot.IP} please check IP");
                    continue;
                }
                    
                var client = new ClientTCP(robot.IP);
                var sftpclient = new ClientSftp(robot.IP);

                var connected = await ConnectToDashboardServerAsync(robot, settings, client);
                if (!connected) continue;

                //check polyscope version for supportfile support. CB3 > 3.13 and CB5 > 5.8
                var versionCheck = await PolyscopeVersionCheck(client);
                if (!versionCheck)
                {
                    settings.ItemLogger.InsertNewMessage($"{robot.RobotName}, {robot.IP}: is not supportfile compatible");
                    continue;
                }  

                //generate supportfile with dashboardcommands
                settings.ItemLogger.InsertNewMessage($"Sending Dashboardcommand: {DashboardCommands.Generate_support_file}{DashboardCommands.Support_file_savepath}");
                SendDashboardCommand(DashboardCommands.Generate_support_file + DashboardCommands.Support_file_savepath, client);

                string message =  await ReadDashboardMessage(client);
                settings.ItemLogger.InsertNewMessage(message);

                if (message.Contains("Error")) continue;

                //download via sftp supportfile and delete at remote destination
                if (sftpclient.Connected) continue;
                var success = await DownloadAndDeleteSupportfile(message, sftpclient, settings);
                if (success is not true) 
                {
                    settings.ItemLogger.InsertNewMessage($"Couldnt connect to: {robot.RobotName}, {robot.IP}");
                }
                settings.ItemLogger.InsertNewMessage($"Supportfile downloaded and deleted from {robot.RobotName}, {robot.IP}");

                client.Disconnect();
                settings.ItemLogger.InsertNewMessage($"Disconnected: {robot.RobotName}: {robot.IP}");

                robotCounter++;
            }

            settings.ItemLogger.InsertNewMessage($"Finished getting supportfiles from all robots: {robotCounter}");
        }

        public async Task UpdateProcessAsync(SettingsViewModel settings)
        {
            if (CheckRoutine(settings) is not true) return;

            //count successfull updates
            int robotCounter = 0;

            foreach (var robot in Robots)
            {
                if (robot.IP is null || robot.Update == false) continue;

                if (!CheckIP(robot.IP))
                {
                    settings.ItemLogger.InsertNewMessage($"{robot.RobotName}: {robot.IP} please check IP");
                    continue;
                }

                var sshClient = new ClientSsh(robot.IP);
                bool connected = sshClient.SshConnect();
                if (connected is not true)
                {
                    settings.ItemLogger.InsertNewMessage($"Couldnt connect to remote host: {robot.RobotName}, {robot.IP} Please check Ip adress of host. Is SSH activated?");
                    continue;
                }

                var usbCheck = CheckUsbConnected(sshClient);
                if (usbCheck == "false")
                {
                    settings.ItemLogger.InsertNewMessage($"Please connect an USB stick to the robot otherwise the update cant be executed: {robot.RobotName}, {robot.IP}");
                    continue;
                }

                var sftpclient = new ClientSftp(robot.IP);
                var success = await sftpclient.ConnectToSftpServer();
                if (success is not true)
                {
                    settings.ItemLogger.InsertNewMessage($"Couldnt connect to remote host: {robot.RobotName}, {robot.IP} Please check Ip adress of host. Is SFTP blocked?");
                    continue;
                }
                settings.ItemLogger.InsertNewMessage($"Connected successfully with: {robot.RobotName}, {robot.IP}");
                settings.ItemLogger.InsertNewMessage($"Uploading: {Path.GetFileName(settings.SelectedSavePath)}");

                sshClient.ExecuteCommand("mount -o remount,async " + usbCheck);
                await sftpclient.UploadFile(usbCheck, settings.SelectedSavePath);
                settings.ItemLogger.InsertNewMessage($"Upload finished: {Path.GetFileName(settings.SelectedSavePath)}");
                sftpclient.Disconnect();

                //send update polyscope
                settings.ItemLogger.InsertNewMessage($"Started update: {robot.RobotName}, {robot.IP}");
                var result = sshClient.ExecuteCommand($"{SshCommands.UpdatePolyscope}{usbCheck}{Path.GetFileName(settings.SelectedSavePath)}");
                sshClient.SshDisconnect();

                //wait until robot finished or waiting time runs out
                var trycounter = 0;
                var dashboardConnected = false;
                while(trycounter <= 7 && dashboardConnected is not true)
                {
                    dashboardConnected = await CheckIfRobotFinishedUpdate(robot.IP);
                    trycounter++;
                }

                if (dashboardConnected is not true || trycounter > 7)
                {
                    settings.ItemLogger.InsertNewMessage($"Couldnt check if robot update is finished due to time out error: {robot.RobotName}, {robot.IP}");
                    continue;
                }

                connected = sshClient.SshConnect();
                if (connected is not true)
                {
                    settings.ItemLogger.InsertNewMessage($"Couldnt connect to remote host: {robot.RobotName}, {robot.IP} Please check Ip adress of host. Is SSH activated?");
                    continue;
                }

                usbCheck = CheckUsbConnected(sshClient);
                if (usbCheck == "false")
                {
                    settings.ItemLogger.InsertNewMessage($"Please connect an USB stick to the robot otherwise the update cant be executed: {robot.RobotName}, {robot.IP}");
                    continue;
                }

                //power on -> IDLE mode to install firmware 
                sshClient.ExecuteCommand(SshCommands.RemotePowerOn);
                sshClient.ExecuteCommand("rm "+ usbCheck + Path.GetFileName(settings.SelectedSavePath));
                sshClient.SshDisconnect();
                settings.ItemLogger.InsertNewMessage($"Robot update is finished: {robot.RobotName}, {robot.IP}");
                robotCounter++;
            }
            settings.ItemLogger.InsertNewMessage($"Finished updating robots: {robotCounter}");
        }

        private string CheckUsbConnected(ClientSsh sshClient)
        {
            var answer = sshClient.ExecuteCommand(SshCommands.GetConnectedUsb);
            if (answer.Contains("media") is not true) return "false";
            answer = answer.TrimEnd(answer[answer.Length - 1]);
            return answer + "/";
        }

        private async Task<bool> CheckIfRobotFinishedUpdate(string ip)
        {
            await Task.Delay(120000);
            var clientTcp = new ClientTCP(ip);
            var result = await clientTcp.ConnectToServerAsync();
            clientTcp.Disconnect();
            if (result is not true) return false;
            return true;
        }

        /// <summary>
        /// Check polyscope version for CB3 series higher than 3.13 and CB5 higher than 5.8
        /// </summary>
        /// <returns>true if supportfile can be generated</returns>
        private async Task<bool> PolyscopeVersionCheck(ClientTCP client)
        {
            string polyscopeVersion = await GetPolyscopeVersion(client);
            //check for CB3
            if (polyscopeVersion[0] == '3') return PolyscopeSupportfileCheckCB3(polyscopeVersion);
            return PolyscopeSupportfileCheckCB5(polyscopeVersion);
        }

        private bool CheckRoutine(SettingsViewModel settings)
        {
            if (_currentRobotIndex == 0)
            {
                settings.ItemLogger.InsertNewMessage(LoggerMessages.NoRobotsInList);
                return false;
            }
            if (settings.SelectedSavePath == "")
            {
                settings.ItemLogger.InsertNewMessage(LoggerMessages.MissingSavePath);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get polyscope version f.e. 
        /// </summary>
        /// <returns>string with polyscope version</returns>
        private async Task<string> GetPolyscopeVersion(ClientTCP client)
        {
            SendDashboardCommand(DashboardCommands.PolyscopeVersion, client);
            string polyscopeVersion = await ReadDashboardMessage(client);
            polyscopeVersion = polyscopeVersion.Remove(0, 11);
            return polyscopeVersion;
        }

        /// <summary>
        /// check CB3 polyscope version if compatible to generate supportfile
        /// </summary>
        /// <returns>true if compatible</returns>
        private bool PolyscopeSupportfileCheckCB3(string polyscopeVersionCB3)
        {
            string[] list = polyscopeVersionCB3.Split(".");
            if (Int32.Parse(list[0]) != 3) return false;
            if (Int32.Parse(list[1]) <13) return false;
            return true;
        }

        /// <summary>
        /// check CB5 polyscope version if compatible to generate supportfile
        /// </summary>
        /// <returns>true if compatible</returns>
        private bool PolyscopeSupportfileCheckCB5(string polyscopeVersionCB5)
        {
            //remove URSoftware in string URSoftware 5.13.0.113898 (Nov 17 2022)
            string[] list = polyscopeVersionCB5.Split(".");
            if (Int32.Parse(list[0]) != 5) return false;
            if (Int32.Parse(list[1]) <= 8) return false;
            return true;
        }

        /// <summary>
        /// connect via sftp to robot/ursim download file to local host and delete at remote host
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sftclient"></param>
        /// <param name="settings"></param>
        /// <returns>true if succeded</returns>
        private async Task<bool> DownloadAndDeleteSupportfile(string message, ClientSftp sftclient, SettingsViewModel settings)
        {
            var filename = GetSupportFileName(message);
            var success = await sftclient.ConnectToSftpServer();
            if (success is not true) return false;
            sftclient.DownloadFile(DashboardCommands.Support_file_savepath + filename, settings.SelectedSavePath + "\\" + filename);
            sftclient.DeleteFile(DashboardCommands.Support_file_savepath + filename);
            sftclient.Disconnect();
            return true;
        }

        /// <summary>
        /// Add new robot to list
        /// </summary>
        /// <param name="parameter"></param>
        private void Add(object parameter)
        {
            var robot = new Robot(++_currentRobotIndex, "New", "0.0.0.0");
            var viewModel = new RobotItemViewModel(robot);
            Robots.Add(viewModel);
            SelectedRobot = viewModel;
        }

        /// <summary>
        /// Create a Json file for all robots at your desired destination
        /// </summary>
        private async void ExportJsonAsync(object parameter)
        {
            string savePath =  await BrowseSavePathJson();
            if(savePath == null) return;

            var Json = new Json(savePath, Robots);
            Json.CreateRobotJson();
        }

        /// <summary>
        /// Create a Json file for all robots at your desired destination
        /// </summary>
        private async void ImportJsonAsync(object parameter)
        {
            string savePath = await OpenJsonFile();
            if (savePath == null) return;

            var Json = new Json(savePath);
            var robots =  Json.ImportRobotJson();
            if (robots == null) return;

            DeleteRobots();
            AddRobots(robots);
        }

        /// <summary>
        /// delete observebale collection of robots
        /// </summary>
        /// <returns></returns>
        private bool DeleteRobots()
        {
            Robots.Clear();
            _currentRobotIndex = 0;
            return true;
        }

        private bool AddRobots(List<Robot> robots)
        {
            if (robots == null) return false;

            foreach (var robot in robots)
            {
                Robots.Add(new RobotItemViewModel(robot));
                _currentRobotIndex++;
            }
            return true;
        }

        /// <summary>
        /// Open Dialog window to let the user browse any path for saving robot list
        /// </summary>
        /// <returns>string</returns>
        private async Task<string> BrowseSavePathJson()
        {
            string _savePathJson = await FilePicker.SaveAsync("Choose .json", ".json", "Robots.json");
            return _savePathJson;
        }

        /// <summary>
        /// Open Dialog window to let the user browse for json import file
        /// </summary>
        /// <returns>string</returns>
        private async Task<string> OpenJsonFile()
        {
            string _savePathJson = await FilePicker.OpenAsync(".json");
            return _savePathJson;
        }

        /// <summary>
        /// Delete selected robot in listview
        /// </summary>
        /// <param name="parameter"></param>
        private void Delete(object parameter)
        {
            if (SelectedRobot is null) return;
            
            Robots.Remove(SelectedRobot);
            SelectedRobot = null;
            --_currentRobotIndex;
        }

        /// <summary>
        /// If robot selected activate delete possible
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns>bool</returns>
        private bool CanDelete(object parameter) => SelectedRobot is not null;

        /// <summary>
        /// check if IP is IPv4
        /// </summary>
        /// <param name="ip"></param>
        /// <returns>bool</returns>
        private bool CheckIP(string ip)
        {
            if (ip == "0.0.0.0" || ip == "127.0.0.1" || ip == "")
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Connect to robot dashboard server if IP is correct and device available
        /// </summary>
        /// <param name="robot"></param>
        /// <param name="settings"></param>
        /// <param name="client"></param>
        /// <returns>true if connection successfull</returns>
        private async Task<bool> ConnectToDashboardServerAsync(RobotItemViewModel robot, SettingsViewModel settings, ClientTCP client)
        {
            if (robot.IP is null)
            {
                settings.ItemLogger.InsertNewMessage($"Please check IP adress of Robot: {robot.RobotName}");
                return false;
            }

            settings.ItemLogger.InsertNewMessage("pinging: " + robot.IP);
            var connected = await client.ConnectToServerAsync();

            if (connected)
            {
                settings.ItemLogger.InsertNewMessage("Connected to: " + robot.RobotName + ", " + robot.IP);
                settings.ItemLogger.InsertNewMessage(await ReadDashboardMessage(client));
                return true;

            }
            else
            {
                settings.ItemLogger.InsertNewMessage(robot.RobotName + ": " + robot.IP + " is not available");
                return false;
            }

        }

        /// <summary>
        /// Send dashboard command as string
        /// </summary>
        /// <param name="command"></param>
        /// <param name="client"></param>
        private void SendDashboardCommand(string command, ClientTCP client)
        {
            client.SendMessage(command);
        }

        /// <summary>
        /// Get answer from robot after connection or after any command was sent
        /// </summary>
        /// <param name="client"></param>
        /// <returns>message from robot as string</returns>
        private async Task<string> ReadDashboardMessage(ClientTCP client)
        {
            var message =  await client.ReceiveMessageAsync();
            return message;
        }

        /// <summary>
        /// Get supportfile name. trim string
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>string as supportfile name</returns>
        private string GetSupportFileName(string filename)
        {
            filename = filename.Remove(0, 24);
            return filename.Remove(filename.Length - 1);
        }

    }
}
