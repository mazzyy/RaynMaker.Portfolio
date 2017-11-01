
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using static RaynMaker.Portfolio.Main;

namespace RaynMaker.Portfolio.Shell
{
    public partial class App : Application
    {
        private Instance myInstance;

        public static int Port;

        protected override void OnStartup(StartupEventArgs e)
        {
            var projectFile = RaynMaker.Portfolio.Main.getProjectFileFromCommandLine(Environment.GetCommandLineArgs().Skip(1).ToArray());
            myInstance = RaynMaker.Portfolio.Main.start(projectFile);
            Port = myInstance.port;

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            RaynMaker.Portfolio.Main.stop(myInstance);
            base.OnExit(e);
        }
    }
}
