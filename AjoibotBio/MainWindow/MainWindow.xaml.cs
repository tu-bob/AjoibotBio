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

            MainWebView.Source = new Uri("https://zhiwj.coded.tj/main.php");

            MainWindowInitialized += InitFaceIdCamera;

            MainWindowInitialized.Invoke(this, EventArgs.Empty);    

           
        }
    }
}
