using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using URManager.Backend.ViewModel;
using URManager.View.Command;
using URManager.Backend.Model;
using Microsoft.UI.Xaml;
using System;

namespace URManager.View.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private TabItems _selectedViewModel;
        private SettingsViewModel _settingsViewModel;
        private FlexibleEthernetIpViewModel _flexibleEthernetIPViewModel;
        private DispatcherTimer _timer;
        private bool _isBackupButtonChecked = false;
        private bool _isUpdateButtonChecked = false;

        public ObservableCollection<TabItems> Tabs { get; set; } = new();

        public MainViewModel()
        {
            CreateTabs();
            _timer = new DispatcherTimer();
            SelectViewModelCommand = new DelegateCommand(SelectViewModel);
            StartBackupProcessCommand = new DelegateCommand(StartBackupProcess, CanStartBackupProcess);
            StartUpdateProcessCommand = new DelegateCommand(StartUpdateProcess);
            //ReportToDasaCommand = new DelegateCommand(ReportToDasa);
        }

        public TabItems SelectedViewModel
        {
            get => _selectedViewModel;
            set
            {
                if (value == _selectedViewModel) return;
                _selectedViewModel = value;
                RaisePropertyChanged();
            }
        }

        public SettingsViewModel SettingsViewModel
        {
            get => _settingsViewModel;

            set
            {
                if (value == _settingsViewModel) return;
                _settingsViewModel = value;
                RaisePropertyChanged();
            }
        }

        public FlexibleEthernetIpViewModel FlexibleEthernetIpViewModel
        {
            get => _flexibleEthernetIPViewModel;

            set
            {
                if (value == _flexibleEthernetIPViewModel) return;
                _flexibleEthernetIPViewModel = value;
                RaisePropertyChanged();
            }
        }

        public bool IsBackupButtonChecked
        {
            get => _isBackupButtonChecked;

            set
            {
                if (value == _isBackupButtonChecked) return;
                _isBackupButtonChecked = value;
                RaisePropertyChanged();
            }
        }

        public bool IsUpdateButtonChecked
        {
            get => _isUpdateButtonChecked;

            set
            {
                if (value == _isUpdateButtonChecked) return;
                _isUpdateButtonChecked = value;
                RaisePropertyChanged();
            }
        }
        public DelegateCommand SelectViewModelCommand { get; }
        public DelegateCommand StartBackupProcessCommand { get; }

        public DelegateCommand StartUpdateProcessCommand { get; }
        //public DelegateCommand ReportToDasaCommand { get; }

        /// <summary>
        /// Provides dummy robot data to listview
        /// </summary>
        /// <returns></returns>
        public async override Task LoadAsync()
        {
            if (SelectedViewModel is null) return; 
            await SelectedViewModel.LoadAsync();         
        }

        /// <summary>
        /// Switch view and viewmodels as tabitems
        /// </summary>
        /// <param name="parameter"></param>
        private async void SelectViewModel(object parameter)
        {
            SelectedViewModel = parameter as TabItems;
            await LoadAsync();
        }

        /// <summary>
        /// Start backup process for all selected robots with selected interval
        /// </summary>
        /// <param name="parameter"></param>
        private void StartBackupProcess(object parameter)
        {
            if (IsBackupButtonChecked is true)
            {
                DispatcherTimerSetup(SettingsViewModel.SelectedIntervallItem.Intervall);

                //call first time backup process
                BackupProcess();

                //backup chosen Intervall
                _timer.Start();
            }
            else
            {
                _timer.Tick -= Timer_Tick;
                _timer.Stop();
            }
        }

        /// <summary>
        /// disable backupprocess if update in settings selected
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns>true if possible</returns>
        private bool CanStartBackupProcess(object parameter) => SettingsViewModel.IsBackupSelected is true;

        /// <summary>
        /// Dispatcher Timer set to 1day ticks
        /// </summary>
        private void DispatcherTimerSetup(int timerIntervall=1)
        {
            _timer.Interval = TimeSpan.FromDays(timerIntervall);
            _timer.Tick += Timer_Tick;
        }

        /// <summary>
        /// Main BackupProcess call which calls the method in RobotViewModel
        /// </summary>
        private async void BackupProcess()
        {
            if (IsBackupButtonChecked is not true) return;

            SettingsViewModel.ItemLogger.InsertNewMessage($"Backup started: {System.DateTime.Now}");
            if (SelectedViewModel is RobotsViewModel robvm)
            {
                await robvm.BackupProcessAsync(SettingsViewModel);
            }
        }
        /// <summary>
        /// Every DispatcherTimer tick the BackupProcess will be executed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, object e)
        {
            BackupProcess();
        }

        /// <summary>
        /// Create Tabs in MainView and declare tab title, icon, isclosable
        /// </summary>
        private void CreateTabs()
        {
            SelectedViewModel = new RobotsViewModel("Robots", new SymbolIconSource { Symbol = Symbol.List }, false);
            SettingsViewModel = new SettingsViewModel("Settings", new SymbolIconSource { Symbol = Symbol.Setting }, false);
            FlexibleEthernetIpViewModel = new FlexibleEthernetIpViewModel("Flexible Ethernet/IP", new SymbolIconSource { Symbol = Symbol.Library }, false);

            Tabs.Add(SelectedViewModel);
            Tabs.Add(SettingsViewModel);
            Tabs.Add(FlexibleEthernetIpViewModel);
        }

        /// <summary>
        /// start update process 
        /// </summary>
        /// <param name="parameter"></param>
        private async void StartUpdateProcess(object parameter)
        {
            if (IsUpdateButtonChecked is not true) return;

            SettingsViewModel.ItemLogger.InsertNewMessage($"Robot update started: {System.DateTime.Now}");
            if (SelectedViewModel is RobotsViewModel robvm)
            {
                await robvm.UpdateProcessAsync(SettingsViewModel);
            }
        }

    }
}
