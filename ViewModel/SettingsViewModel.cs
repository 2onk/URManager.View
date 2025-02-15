﻿using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using URManager.Backend.Data;
using URManager.Backend.Model;
using URManager.Backend.ViewModel;
using URManager.View.Command;

namespace URManager.View.ViewModel
{
    public class SettingsViewModel : TabItems
    {
        private readonly Settings _settings;
        private readonly IBackupDataProvider _backupDataProvider;

        public SettingsViewModel(object name, object icon, bool isClosable) : base(name, icon, isClosable)
        {
            BrowseSavePathCommand = new DelegateCommand(BrowseSavePath);
            _backupDataProvider = new BackupDataProvider();
            ItemLogger = new();
            _settings = new Settings();
            LoadBackupData();
        }

        public DelegateCommand BrowseSavePathCommand { get; }
        public ItemLogger ItemLogger { get; set; }

        public ObservableCollection<BackupIntervall> BackupIntervalls { get; } = new();

        /// <summary>
        /// readonly prop for all settings
        /// </summary>
        public Settings Settings => _settings;

        public string SelectedSavePath
        {
            get => _settings.SelectedsavePath;
            set
            {
                if (value == _settings.SelectedsavePath) return;
                _settings.SelectedsavePath = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Check if checkbox backup is selected
        /// </summary>
        public bool IsBackupSelected
        {
            get => _settings.IsBackupSelected;
            set
            {
                if (value == _settings.IsBackupSelected) return;
                _settings.IsBackupSelected = value;
                SelectedSavePath = "";
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Check if checkbox update is selected
        /// </summary>
        public bool IsUpdateSelected
        {
            get => _settings.IsUpdateSelected;
            set
            {
                if (value == _settings.IsUpdateSelected) return;
                _settings.IsUpdateSelected = value;
                SelectedSavePath = "";
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Get selected Backup Intervall
        /// </summary>
        public BackupIntervall SelectedIntervallItem
        {
            get => _settings.SelectedBackupIntervall;
            set
            {
                if (value == _settings.SelectedBackupIntervall) return;
                _settings.SelectedBackupIntervall = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// load backupdata intervalls in for combobox
        /// </summary>
        /// <returns>true if loaded</returns>
        private bool LoadBackupData()
        {
            if (BackupIntervalls.Any()) return false;
            

            var intervalls = _backupDataProvider.GetAll();
            if (intervalls is null) return false;

            foreach (var backupIntervall in intervalls)
            {
                BackupIntervalls.Add(backupIntervall);
            }
            return true;
        }

        /// <summary>
        /// Depending on backup or update choose the correct method
        /// </summary>
        /// <param name="parameter"></param>
        private async void BrowseSavePath(object parameter)
        {

            if (IsBackupSelected) await BrowseSavePathBackup(parameter);
            else await BrowseUpdateFile(parameter);
        }

        /// <summary>
        /// Open Dialog window to let the user browse any path for saving support/backup files
        /// </summary>
        private async Task BrowseSavePathBackup(object parameter)
        {
            string savepath = await FilePicker.OpenFolderAsync();

            if (savepath is not null)
            {
                SelectedSavePath = savepath;
                ItemLogger.InsertNewMessage("Saving path selected");
            }
            else
            {
                SelectedSavePath = "";
                ItemLogger.InsertNewMessage("No path selected");
            }
        }

        /// <summary>
        /// Open Dialog window to let the user browse for a .urp updatefile
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="filter"></param>
        private async Task BrowseUpdateFile(object parameter, string filter = ".urup")
        {
            string savepath = await FilePicker.OpenAsync(filter);

            if (savepath is not null)
            {
                SelectedSavePath = savepath;
                ItemLogger.InsertNewMessage("Updatefile selected");
            }
            else
            {
                SelectedSavePath = "";
                ItemLogger.InsertNewMessage("No updatefile selected");
            }
        }
    }
}
