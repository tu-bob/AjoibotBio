using AjoibotBio.MainWindow;
using AjoibotBio.Properties;
using AjoibotBio.Utils;
using libzkfpcsharp;
using log4net;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using ZKFaceId;

namespace AjoibotBio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(App));

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Mutex mutex;

        private const string UniqueEventName = "AjoibotFingerPrintEvente2176e68-536a-11e8-9c2d-fa7ae01bbebc";

        private const string UniqueMutexName = "AjoibotFingerPrintcbb6e25c-536a-11e8-9c2d-fa7ae01bbebc";

        private EventWaitHandle eventWaitHandle;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();
            log.Info("        =============  Started Logging  =============        ");

            bool isOwned;
            this.mutex = new Mutex(true, UniqueMutexName, out isOwned);
            this.eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);

            GC.KeepAlive(this.mutex);

            if (isOwned)
            {
                var thread = new Thread(() =>
                {
                    while (this.eventWaitHandle.WaitOne())
                    {
                        Current.Dispatcher.BeginInvoke(
                            (Action)(() => ((MainWindow.MainWindow)Current.MainWindow).BringToForeground()));
                    }
                });


                thread.IsBackground = true;
                thread.Start();

                string line = string.Join("", e.Args);
                if (!string.IsNullOrEmpty(line))
                {
                    MainViewModel.Uri = "http://" + line.Split(':')[1];
                }

                return;
            }

            var proc = Process.GetCurrentProcess();
            var processName = proc.ProcessName.Replace(".vshost", "");
            var runningProcess = Process.GetProcesses()
                .FirstOrDefault(x => (x.ProcessName == processName
                || x.ProcessName == proc.ProcessName
                || x.ProcessName == proc.ProcessName + ".vshost")
                && x.Id != proc.Id);

            if (runningProcess != null)
                UnsafeNative.SendMessage(runningProcess.MainWindowHandle, string.Join(" ", e.Args));

            this.eventWaitHandle.Set();
            this.Shutdown();
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error("Fatal error: " + e.ToString());
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                var settings = new Settings
                {
                    LastUri = MainViewModel.Uri,
                    FullScreen = MainViewModel.IsFullscreen,
                    WindowsWidth = MainViewModel.WindowsWidth,
                    WindowsHeight = MainViewModel.WindowsHeight
                };
                settings.Save();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            //Close all devices and libraries
            //MainViewModel.CloseFaceIdDevices();

            //ZKCameraLib.Terminate();

            //ZKHIDGeneral.Terminate();

            foreach (var scanner in MainViewModel.ZkScanners)
            {
                scanner.DisconnectDevice();
            }

            zkfp2.Terminate();
        }
    }
}
