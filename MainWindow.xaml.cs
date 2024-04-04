using Microsoft.UI.Xaml;
using URManager.View.ViewModel;


namespace URManager.View
{

    public sealed partial class MainWindow : Window
    {


        public MainWindow()
        {
            this.InitializeComponent();
            Title = "UR Manager";
            ViewModel = new MainViewModel();
            root.Loaded += Root_Loaded;

        }
        public MainViewModel ViewModel { get; }

        private void ButtonToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            root.RequestedTheme = root.RequestedTheme == ElementTheme.Light
                                      ? ElementTheme.Dark
                                      : ElementTheme.Light;
        }

        private async void Root_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadAsync();
        }

    }
}
