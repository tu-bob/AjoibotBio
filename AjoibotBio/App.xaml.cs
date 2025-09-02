using AjoibotBio.MainWindow;
using AjoibotBio.Properties;
using AjoibotBio.Utils;
using log4net;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using ZKFaceId;
using ZKFingerprint;

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
            var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
            log4net.Config.XmlConfigurator.Configure(logRepository, new System.IO.FileInfo("log4net.config"));
            log.Info("        =============  Started Logging  =============        ");

            // Ensure the application is running with administrative privileges if required
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    log.Warn("Application is not running as administrator. Attempting to relaunch elevated.");
                    MessageBox.Show(
                        "This application requires administrator privileges to properly initialize embedded browser and device drivers.\n" +
                        "The application will relaunch with elevated permissions.",
                        "Administrator Rights Required",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    var currentExe = Process.GetCurrentProcess().MainModule?.FileName ?? System.Reflection.Assembly.GetEntryAssembly()?.Location;
                    var psi = new ProcessStartInfo(currentExe!)
                    {
                        UseShellExecute = true,
                        Verb = "runas",
                        Arguments = string.Join(" ", e.Args ?? Array.Empty<string>())
                    };

                    try
                    {
                        Process.Start(psi);
                        // Close current (non-elevated) instance
                        this.Shutdown();
                        return;
                    }
                    catch (Exception startEx)
                    {
                        log.Error("Failed to relaunch application with elevated privileges.", startEx);
                        // Continue without elevation; downstream components may fail if admin is truly required.
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error while checking or acquiring administrative privileges.", ex);
            }

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

            ZkFingerprintLib.Terminate();
        }
    }
}
