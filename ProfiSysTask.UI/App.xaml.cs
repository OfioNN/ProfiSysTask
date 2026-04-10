using Microsoft.Extensions.DependencyInjection;
using ProfiSysTask.Core.Interfaces;
using ProfiSysTask.Infrastructure.DataAccess;
using ProfiSysTask.Infrastructure.Services;
using ProfiSysTask.UI.ViewModels;
using ProfiSysTask.UI.Views;
using System.Windows;

namespace ProfiSysTask.UI {
    public partial class App : Application {
        private readonly IServiceProvider _serviceProvider;

        public App() {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services) {
            services.AddDbContext<AppDbContext>();

            services.AddTransient<ICsvImporter, CsvImporter>();
            services.AddTransient<IDocumentRepository, DocumentRepository>();

            services.AddTransient<StartViewModel>();
            services.AddTransient<DataViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainWindow>();
        }

        private void OnStartup(object sender, StartupEventArgs e) {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

    }
}
