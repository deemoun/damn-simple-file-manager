using System;
using System.Windows;

namespace DamnSimpleFileManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                var ex = args.ExceptionObject as Exception ?? new Exception(args.ExceptionObject.ToString());
                Logger.LogError("Unhandled exception", ex);
            };

            DispatcherUnhandledException += (s, args) =>
            {
                Logger.LogError("Dispatcher unhandled exception", args.Exception);
            };

            Settings.Load();
            Logger.Log("Application started");
        }
    }
}
