using HK_Rando_4_Log_Display.FileReader;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace HK_Rando_4_Log_Display
{
    public partial class App : Application
    {
        private ServiceProvider serviceProvider;

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
            services.AddTransient<IHelperLogReader, HelperLogReader>();
            services.AddTransient<ITrackerLogReader, TrackerLogReader>();
            services.AddTransient<ISettingsReader, SettingsReader>();
            services.AddTransient<IItemSpoilerReader, ItemSpoilerReader>();
            services.AddTransient<ITransitionSpoilerReader, TransitionSpoilerReader>();
            services.AddSingleton<IResourceLoader, ResourceLoader>();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            SetupExceptionHandling();
            var mainWindow = serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }

        private void LogUnhandledException(Exception exception, string source)
        {
            string message = $"Unhandled exception ({source})";
            try
            {
                System.Reflection.AssemblyName assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                message = string.Format("Unhandled exception in {0} v{1}", assemblyName.Name, assemblyName.Version);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in LogUnhandledException");
            }
            finally
            {
                _logger.Error(exception, message);
            }
        }
    }
}
