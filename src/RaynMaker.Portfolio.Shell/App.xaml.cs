
using Plainion;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using static RaynMaker.Portfolio.Service.Main;

namespace RaynMaker.Portfolio.Shell
{
    public partial class App : Application
    {
        private Instance myInstance;

        public static int Port;

        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += OnUnhandledException;

            var projectFile = getProjectFileFromCommandLine(Environment.GetCommandLineArgs().Skip(1).ToArray());
            myInstance = startCSharp((msg,ex) => MessageBox.Show(msg + Environment.NewLine + ex.Dump(), "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error), projectFile);
            Port = myInstance.port;

            base.OnStartup(e);
        }

        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Dump(), "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);

            e.Handled = true;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            stop(myInstance);
            base.OnExit(e);
        }
    }
}
