using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;


namespace AjoibotBio.MainWindow
{
    public partial class MainWindow : Window
    {
        private event EventHandler MainWindowInitialized;

        public MainWindow()
        {
            InitializeComponent();

            MainWebView.Source = new Uri("https://office.tajmedun.tj/testdemo.php");

            MainWindowInitialized += InitFaceIdCamera;

            MainWindowInitialized.Invoke(this, EventArgs.Empty);    

           
        }
    }
}
