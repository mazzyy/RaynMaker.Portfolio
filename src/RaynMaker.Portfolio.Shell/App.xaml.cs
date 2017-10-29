
using System;
using System.Linq;
using System.Threading;
using System.Windows;

namespace RaynMaker.Portfolio.Shell
{
    public partial class App : Application
    {
        private CancellationTokenSource myCTS;

        protected override void OnStartup(StartupEventArgs e)
        {
            var projectFile = RaynMaker.Portfolio.Main.getProjectFileFromCommandLine(Environment.GetCommandLineArgs().Skip(1).ToArray());
            var ret = RaynMaker.Portfolio.Main.start(projectFile);
            myCTS = ret.Item2;

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            myCTS.Cancel();
            base.OnExit(e);
        }
    }
}
