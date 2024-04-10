using System.Collections;
using URManager.Backend.Model;
using URManager.Backend.ViewModel;
using URManager.View.Command;

namespace URManager.View.ViewModel
{
    public class SettingsViewModel : TabItems
    {
        private Settings _settings;

        public SettingsViewModel(object name, object icon, bool isClosable) : base(name, icon, isClosable)
        {
            BrowseSavePathCommand = new DelegateCommand(BrowseSavePath);
            ItemLogger = new();
            _settings = new Settings();
            BackupIntervall = new int[1,7,14,31];
        }

        public DelegateCommand BrowseSavePathCommand { get; }
        public ItemLogger ItemLogger { get; set; }

        public ICollection BackupIntervall { get;}

        public int SelectedIntervall { get; set; }


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
