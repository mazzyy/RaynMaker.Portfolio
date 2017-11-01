using System.Windows;

namespace RaynMaker.Portfolio.Shell
{
    public partial class Shell : Window
    {
        public Shell()
        {
            InitializeComponent();

            myBrowser.Navigate(string.Format("http://localhost:{0}/", App.Port));
        }
    }
}
