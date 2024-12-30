using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ODExplorer.Stores;
using ODExplorer.ViewModels.ViewVMs;
using ODExplorer.Windows;
using ODUtils.APis;
using ODUtils.ViewModelNavigation;
using System;
using System.IO;
using System.Windows;
using ODExplorer.Extensions;
using NLog;
using System.Diagnostics;

namespace ODExplorer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static readonly Version AppVersion = new(2, 0, 6);
#if INSTALL
        public readonly static string BaseDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OD Explorer");
#else
        public readonly static string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
#endif
        private const string database = "ODExplorer.db";
        private readonly string connectionString = $"DataSource={Path.Combine(BaseDirectory, database)};";

        private readonly IHost _host;

        public App()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = Path.Combine(BaseDirectory, "Error.txt") };
            var logConsole = new NLog.Targets.ConsoleTarget("logConsole");

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logConsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            config.AddRule(LogLevel.Error, LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;

            _host = Host.CreateDefaultBuilder()
               .AddDatabase(connectionString)
               .AddViewModels()
               .AddWindows()
               .AddStores()
               .AddServices()
               .AddNavigation()
               .AddHttpClients()
               .Build();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            _host.Start();

            var api = _host.Services.GetRequiredService<EdsmApiService>();
            if (api != null)
            {
                api.OnError += Api_OnError;
            }
            if (Directory.Exists(BaseDirectory) == false)
            {
                Directory.CreateDirectory(BaseDirectory);
            }
//#if DEBUG
//            var createDb = !File.Exists(Path.Combine(BaseDirectory, database));

//            if (createDb)
//            {
//#endif
            //Disable shutdown when the dialog closes
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var updateWindow = _host.Services.GetRequiredService<LoaderWindow>();
            if (updateWindow.ShowDialog() is bool v && !v)
            {
                Shutdown();
                return;
            }

            ShutdownMode = ShutdownMode.OnMainWindowClose;
//#if DEBUG
//            }
//#endif


            var settings = _host.Services.GetRequiredService<SettingsStore>();

            settings.LoadSettings();

            var navigationService = _host.Services.GetRequiredService<OdNavigationService<LoadingViewModel>>();
            navigationService.Navigate();

            MainWindow = _host.Services.GetRequiredService<MainWindow>();
            MainWindow.Show();
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Error(e.Exception.ToString(), "DispatcherUnhandledException");
        }

        private void Api_OnError(object? sender, Exception e)
        {
            Logger.Error(e, "EDSM Api Error");
        }

        private void CurrentDomain_FirstChanceException(object? sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            if (e.Exception is System.Threading.Tasks.TaskCanceledException tc)
            {
                Trace.WriteLine(tc.Message);
                return;
            }
            if (e.Exception is OperationCanceledException oce)
            {
                Trace.WriteLine(oce.Message);
                return;
            }
            if (e.Exception is System.Threading.Tasks.TaskCanceledException tce)
            {
                Trace.WriteLine(tce.Message);
                return;
            }
            Logger.Error(e.Exception, "First Chance Exception");
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Error(e.ExceptionObject?.ToString(), "CurrentDomain_UnhandledException");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host.Dispose();
            base.OnExit(e);
        }
    }
}
