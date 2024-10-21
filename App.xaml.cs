using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using URManager.Backend.Data;
using URManager.View.ViewModel;


namespace URManager.View
{
    public partial class App : Application
    {
        public static Window Window { get; } = new MainWindow();
        private Window m_window;
        private readonly ServiceProvider _serviceProvider;
        public App()
        {
            this.InitializeComponent();
            ServiceCollection services = new();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddTransient<MainWindow>();

            services.AddTransient<MainViewModel>();
            services.AddTransient<RobotsViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<FlexibleEthernetIpViewModel>();

            services.AddTransient<IRobotDataProvider, RobotDataProvider>();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = _serviceProvider.GetService<MainWindow>();
            m_window?.Activate();
        }

    }
}
