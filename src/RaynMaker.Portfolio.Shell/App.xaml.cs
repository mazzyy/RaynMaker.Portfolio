
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Plainion;
using static RaynMaker.Portfolio.Main;

namespace RaynMaker.Portfolio.Shell
{
    public partial class App : Application
    {
        private Instance myInstance;

        public static int Port;

        protected override void OnStartup(StartupEventArgs e)
        {
            Debugger.Launch();
            DispatcherUnhandledException += OnUnhandledException;

            var projectFile = RaynMaker.Portfolio.Main.getProjectFileFromCommandLine(Environment.GetCommandLineArgs().Skip(1).ToArray());
            myInstance = RaynMaker.Portfolio.Main.start(projectFile);
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
            RaynMaker.Portfolio.Main.stop(myInstance);
            base.OnExit(e);
        }
    }
}
