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
using ZKFaceId;

namespace AjoibotBio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(App));

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
                else {
                    var settings = new Settings();
                    MainViewModel.Uri = settings.LastUri;
                }

                if(MainViewModel.Uri == null)
                {
                    log.Error("URL is not provided");
                    this.Shutdown();
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



        private void Application_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                var settings = new Settings
                {
                    LastUri = MainViewModel.Uri,
                    IsFullScreen = MainViewModel.IsFullscreen,
                    WindowWidth = MainViewModel.WindowWidth,
                    WindowHeight = MainViewModel.WindowHeight
                };
                settings.Save();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            //Close all devices and libraries
            MainViewModel.Visible?.CloseDevice();
            
            MainViewModel.NIR?.CloseDevice();

            ZKCameraLib.Terminate();

            foreach (var scanner in MainViewModel.ZkScanners)
            {
                scanner.DisconnectDevice();
            }

            zkfp2.Terminate();
        }
    }
}
