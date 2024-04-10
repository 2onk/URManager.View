using System.Collections;
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
        private Settings _settings;
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

        public string SelectedSavePath
        {
            get => _settings.SelectedsavePath;
            set
            {
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
                _settings.IsBackupSelected = value;
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
                _settings.IsUpdateSelected = value;
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
        /// Open Dialog window to let the user browse any path for saving support/backup files
        /// </summary>
        /// <param name="parameter"></param>
        private async void BrowseSavePath(object? parameter)
        {

            string savepath = await FilePicker.OpenFolderAsync();

            if (savepath is not null)
            {
                SelectedSavePath = savepath;
                ItemLogger.Add("Saving path selected");
            }
            else
            {
                SelectedSavePath = "";
                ItemLogger.Add("No path selected");
            }
        }
    }
}
