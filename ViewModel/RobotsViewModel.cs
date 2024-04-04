using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using URManager.Backend.Data;
using URManager.Backend.Model;
using URManager.Backend.Net;
using URManager.View.Command;

namespace URManager.View.ViewModel
{
    public class RobotsViewModel : TabItems
    {
        private readonly IRobotDataProvider _robotDataProvider;
        private RobotItemViewModel? _selectedRobot;

        public RobotsViewModel(object name, object icon, bool isClosable) : base(name, icon, isClosable)
        {
            _robotDataProvider = new RobotDataProvider();
            AddCommand = new DelegateCommand(Add);
            DeleteCommand = new DelegateCommand(Delete, CanDelete);
        }


        public ObservableCollection<RobotItemViewModel> Robots { get; } = new();

        public RobotItemViewModel? SelectedRobot
        {
            get => _selectedRobot;
            set
            {
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

        /// <summary>
        /// Provides dummy robot data to listview
        /// </summary>
        /// <returns></returns>
        public async override Task LoadAsync()
        {
            if (Robots.Any())
            {
                return;
            }

            var robots = await _robotDataProvider.GetAll();
            if (robots is not null)
            {
                foreach (var robot in robots)
                {
                    Robots.Add(new RobotItemViewModel(robot));
                }
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
            foreach (var robot in Robots)
            {
                if (robot.IP is not null)
                {
                    if (CheckIP(robot.IP))
                    {
                        var client = new ClientTCP(robot.IP);
                        var sftclient = new ClientSSH(robot.IP);

                        var connected = await ConnectToDashboardServerAsync(robot, settings, client);
                        if (connected)
                        {
                            //generate supportfile with dashboardcommands
                            settings.ItemLogger.Add("Sending Dashboardcommand: " + DashboardCommands.Generate_support_file + DashboardCommands.Support_file_savepath);
                            SendDashboardCommand(DashboardCommands.Generate_support_file + DashboardCommands.Support_file_savepath, client);

                            string message =  await ReadDashboardMessage(client);
                            settings.ItemLogger.Add(message);


                            //download via sftp supportfile and delete at remote destination
                            if (!sftclient.Connected)
                            {
                                var filename = GetSupportFileName(message);
                                sftclient.ConnectToSftpServer();
                                sftclient.DownloadFile(DashboardCommands.Support_file_savepath + filename, settings.SelectedSavePath + "\\" + filename);
                                sftclient.DeleteFile(DashboardCommands.Support_file_savepath + filename);
                                sftclient.Disconnect();
                            }


                            client.Disconnect();
                            settings.ItemLogger.Add("Disconnected: " + robot.RobotName + ": " + robot.IP);
                        }
                    }
                    else
                    {
                        settings.ItemLogger.Add(robot.RobotName + ": " + robot.IP + " please check IP");
                    }
                }
            }
        }

        //public void UpdateProcess(SettingsViewModel settings)
        //{

        //}

        /// <summary>
        /// Add new robot to list
        /// </summary>
        /// <param name="parameter"></param>
        private void Add(object? parameter)
        {
            var robot = new Robot { RobotName = "New", IP = "0.0.0.0" };
            var viewModel = new RobotItemViewModel(robot);
            Robots.Add(viewModel);
            SelectedRobot = viewModel;
        }

        /// <summary>
        /// Delete selected robot in listview
        /// </summary>
        /// <param name="parameter"></param>
        private void Delete(object? parameter)
        {
            if (SelectedRobot is not null)
            {
                Robots.Remove(SelectedRobot);
                SelectedRobot = null;
            }
        }

        /// <summary>
        /// If robot selected activate delete possible
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns>bool</returns>
        private bool CanDelete(object? parameter) => SelectedRobot is not null;

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
            else
            {
                return true;
            }
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
                settings.ItemLogger.Add("Please check IP adress of Robots");
                return false;
            }

            settings.ItemLogger.Add("pinging: " + robot.IP);
            var connected = await client.ConnectToServerAsync();

            if (connected)
            {
                settings.ItemLogger.Add("Connected to: " + robot.RobotName + ", " + robot.IP);
                settings.ItemLogger.Add(await ReadDashboardMessage(client));
                return true;

            }
            else
            {
                settings.ItemLogger.Add(robot.RobotName + ": " + robot.IP + " is not available");
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
