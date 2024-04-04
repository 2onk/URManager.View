using Ookii.Dialogs.Wpf;
using URManager.Backend.Model;
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
        }

        public DelegateCommand BrowseSavePathCommand { get; }
        public ItemLogger ItemLogger { get; set; }

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
        private void BrowseSavePath(object? parameter)
        {
            var dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select a folder where you would like to save your supportfiles.";
            dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.

            if ((bool)dialog.ShowDialog()!)
            {
                SelectedSavePath = dialog.SelectedPath;
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
