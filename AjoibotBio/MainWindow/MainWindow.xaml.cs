using AjoibotBio.Utils;
using log4net;
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

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public MainWindow()
        {
            InitializeComponent();

            CheckPrerequisits();

            //MainWebView.Source = new Uri("https://zhiwj.coded.tj/main.php");
            MainWebView.Source = new Uri("https://office.tajmedun.tj/api/bio/index.php");

            MainWindowInitialized += InitFaceIdCamera;

            MainWindowInitialized.Invoke(this, EventArgs.Empty);


        }

        private void CheckPrerequisits()
        {
            if (WebView2Install.GetInfo().Type != InstallType.WebView2)
            {
                Log.Error("WebView2 environment is not installed on current machine");
            }
        }
    }
}
