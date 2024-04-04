using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using URManager.Backend.ViewModel;
using URManager.View.Command;
using URManager.Backend.Model;

namespace URManager.View.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private TabItems? _selectedViewModel;
        private SettingsViewModel _settingsViewModel;
        public ObservableCollection<TabItems> Tabs { get; set; } = new();

        public MainViewModel()
        {
            CreateTabs();
            SelectViewModelCommand = new DelegateCommand(SelectViewModel);
            StartBackupProcessCommand = new DelegateCommand(StartBackupProcess);
            //StartUpdateProcessCommand = new DelegateCommand(StartUpdateProcess);
        }

        public TabItems? SelectedViewModel
        {
            get => _selectedViewModel;
            set
            {
                _selectedViewModel = value;
                RaisePropertyChanged();
            }
        }

        public SettingsViewModel SettingsViewModel
        {
            get => _settingsViewModel;

            set
            {
                _settingsViewModel = value;
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
            if (SelectedViewModel is not null)
            {
                await SelectedViewModel.LoadAsync();
            }
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
        private async void StartBackupProcess(object? parameter)
        {
            _settingsViewModel.ItemLogger.Add("start");
            if (SelectedViewModel is RobotsViewModel robvm)
            {
                 await robvm.BackupProcessAsync(_settingsViewModel);
            }
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
