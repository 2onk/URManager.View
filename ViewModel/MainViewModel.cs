﻿using Microsoft.UI.Xaml.Controls;
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
        private DispatcherTimer _timer;
        private bool _isBackupChecked = false;

        public ObservableCollection<TabItems> Tabs { get; set; } = new();

        public MainViewModel()
        {
            CreateTabs();
            _timer = new DispatcherTimer();
            SelectViewModelCommand = new DelegateCommand(SelectViewModel);
            StartBackupProcessCommand = new DelegateCommand(StartBackupProcess, CanStartBackupProcess);
            //StartUpdateProcessCommand = new DelegateCommand(StartUpdateProcess);
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

        public bool IsBackupChecked
        {
            get => _isBackupChecked;

            set
            {
                if (value == _isBackupChecked) return;
                _isBackupChecked = value;
                RaisePropertyChanged();
            }
        }

        public bool BackupPressed ()
        {
            IsBackupChecked = !IsBackupChecked;
            return IsBackupChecked;
        }

        public DelegateCommand SelectViewModelCommand { get; }
        public DelegateCommand StartBackupProcessCommand { get; }

        //update noch ausarbeiten 
        //public DelegateCommand StartUpdateProcessCommand { get; }

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
            if (IsBackupChecked is true)
            {
                DispatcherTimerSetup(SettingsViewModel.SelectedIntervallItem.Intervall);

                //call first time backup process
                BackupProcess();

                //backup every day
                _timer.Start();
            }
            else
            {
                _timer.Tick -= Timer_Tick;
                _timer.Stop();
            }
        }

        //canexcute noch ausarbeiten
        //private bool CanStartBackupProcess(object? parameter) => SettingsViewModel.SelectedSavePath != "";

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
            if (IsBackupChecked is not true) return;

            SettingsViewModel.ItemLogger.InsertNewMessage($"Backup start: {System.DateTime.Now}");
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
            Tabs.Add(SelectedViewModel);
            Tabs.Add(SettingsViewModel);
        }


        //private void StartUpdateProcess(object? obj)
        //{
        //    RobotsViewModel.UpdateProcess(_settingsViewModel);
        //}
    }
}
