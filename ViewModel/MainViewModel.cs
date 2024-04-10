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
        private TabItems? _selectedViewModel;
        private SettingsViewModel _settingsViewModel;
        private DispatcherTimer _timer;
        private bool _isBackupEnabled = false;
        public ObservableCollection<TabItems> Tabs { get; set; } = new();

        public MainViewModel()
        {
            CreateTabs();
            _timer = new DispatcherTimer();
            SelectViewModelCommand = new DelegateCommand(SelectViewModel);
            StartBackupProcessCommand = new DelegateCommand(StartBackupProcess);
            //StartUpdateProcessCommand = new DelegateCommand(StartUpdateProcess);
        }

        public TabItems? SelectedViewModel
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

        public bool IsBackupEnabled
        {
            get => _isBackupEnabled;

            set
            {
                if (value == _isBackupEnabled) return;
                _isBackupEnabled = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand SelectViewModelCommand { get; }
        public DelegateCommand StartBackupProcessCommand { get; }

        //update noch ausarbeiten 
        //public DelegateCommand StartUpdateProcessCommand { get; }

        //can delete noch ausarbeiten
        //private bool CanStartBackupProcess(object? parameter) => SettingsViewModel.SelectedSavePath != "";


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
        private async void SelectViewModel(object? parameter)
        {
            SelectedViewModel = parameter as TabItems;
            await LoadAsync();
        }

        /// <summary>
        /// Start backup process for all selected robots with selected interval
        /// </summary>
        /// <param name="parameter"></param>
        private void StartBackupProcess(object? parameter)
        {
            if (!IsBackupEnabled)
            {
                IsBackupEnabled = true;

                DispatcherTimerSetup(_settingsViewModel.SelectedIntervallItem.Intervall);

                //call first time backup process
                BackupProcess();

                //backup every 60 s
                _timer.Start();
            }
            else
            {
                IsBackupEnabled = false;
                _timer.Tick -= Timer_Tick;
                _timer.Stop();
            }
        }
        /// <summary>
        /// Dispatcher Timer set to 60s ticks
        /// </summary>
        private void DispatcherTimerSetup(int timerIntervall=60)
        {
            _timer.Interval = TimeSpan.FromSeconds(timerIntervall);
            _timer.Tick += Timer_Tick;
        }

        /// <summary>
        /// Main BackupProcess call which calls the method in RobotViewModel
        /// </summary>
        private async void BackupProcess()
        {
            _settingsViewModel.ItemLogger.Add("start");
            if (SelectedViewModel is RobotsViewModel robvm)
            {
                await robvm.BackupProcessAsync(_settingsViewModel);
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
            _settingsViewModel = new SettingsViewModel("Settings", new SymbolIconSource { Symbol = Symbol.Setting }, false);
            Tabs.Add(SelectedViewModel);
            Tabs.Add(_settingsViewModel);
        }


        //private void StartUpdateProcess(object? obj)
        //{
        //    RobotsViewModel.UpdateProcess(_settingsViewModel);
        //}
    }
}
