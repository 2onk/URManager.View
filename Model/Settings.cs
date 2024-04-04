namespace URManager.View.Model
{
    public class Settings
    {
        public Settings()
        {
            SelectedsavePath = "";
            IsBackupSelected = true;
        }
        public string SelectedsavePath { get; set; }
        public bool IsBackupSelected { get; set; }
        public bool IsUpdateSelected { get; set; }

    }
}
