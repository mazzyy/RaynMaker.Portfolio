using System.Windows;

namespace RaynMaker.Portfolio.Shell
{
    public partial class Shell : Window
    {
        public Shell()
        {
            InitializeComponent();

            myBrowser.Navigate("http://localhost:2525/");
        }
    }
}
