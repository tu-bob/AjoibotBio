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

            MainWindowInitialized += InitFaceIdCamera;

            MainWindowInitialized.Invoke(this, EventArgs.Empty);    

            Debug.WriteLine("inited");
        }
    }
}
