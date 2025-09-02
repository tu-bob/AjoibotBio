using AjoibotBio.Js;
using AjoibotBio.Properties;
using AjoibotBio.Utils;
using log4net;
using Microsoft.Web.WebView2.Core;
using System;
using System.Web;
using System.Windows;
using System.Windows.Interop;

namespace AjoibotBio.MainWindow
{
    public partial class MainWindow : Window
    {
        private event EventHandler MainWindowInitialized;

        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public delegate void AppRestartedHandler();

        public static event AppRestartedHandler AppRestarted;

        public static IntPtr WindowHandle { get; private set; }

        private bool _isWebViewAvailable;

        public MainWindow()
        {
            InitializeComponent();

            CheckPrerequisits();

            Loaded += (s, e) =>
            {
                MainWindow.WindowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
                HwndSource.FromHwnd(MainWindow.WindowHandle)?.AddHook(new HwndSourceHook(HandleMessages));
            };
#if DEBUG
            //MainViewModel.Uri = "https://office.kth.tj/api/bio/registration.php?0&5473&792e76dee67d778158f2f6a90dff03f2&1";
#endif
            MainWindowInitialized += InitFaceIdCamera;

            MainWindowInitialized += RestoreWindowSize;

            MainWindowInitialized += InitFingerprintScanner;

            MainWindowInitialized += ParseUri;

            MainWindowInitialized.Invoke(this, EventArgs.Empty);

            AppRestarted += ParseUri;
        }

        private void CheckPrerequisits()
        {
            var installInfo = WebView2Install.GetInfo();
            _isWebViewAvailable = installInfo.Type == InstallType.WebView2;
            if (!_isWebViewAvailable)
            {
                Log.Error("WebView2 environment is not installed on current machine");
                try
                {
                    MessageBox.Show(
                        "Microsoft Edge WebView2 Runtime is not installed.\n" +
                        "Please install it to enable the built-in browser.\n\n" +
                        "We will open the official download page in your browser.",
                        "WebView2 Runtime Required",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "https://go.microsoft.com/fwlink/p/?LinkId=2124703",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to open WebView2 installer page", ex);
                }
            }
        }

        private void ParseUri(object sender, EventArgs e)
        {
            ParseUri();
        }

        private void ParseUri()
        {
            Log.Debug("Parse uri: " + MainViewModel.Uri);

            if (MainViewModel.Uri.Contains("a_light"))
            {
                var url = new Uri(MainViewModel.Uri);
                var command = HttpUtility
                    .ParseQueryString(url.Query)
                    .Get("lamp");
                //SendCommandToLamp(command);
                //Commands.ShutdownApp();
                Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
            }
            else
            {
                if (_isWebViewAvailable)
                    NavigateToUri();
                else
                    Log.Warn("Skipping navigation because WebView2 runtime is not available.");
            }
        }

        private void RestoreWindowSize(object sender, EventArgs e)
        {
            var settings = new Settings();
            
            MainViewModel.IsFullscreen = settings.IsFullScreen;
            MainViewModel.WindowHeight = settings.WindowHeight;
            MainViewModel.WindowWidth = settings.WindowWidth;

            if (MainViewModel.IsFullscreen)
            {
                this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.None;
            } else if (MainViewModel.WindowHeight > 0 && MainViewModel.WindowWidth > 0)
            {
                this.Height = MainViewModel.WindowHeight;
                this.Width = MainViewModel.WindowWidth;
            }
        }

        private void ToogleFullScreen()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.None;
            }

            MainViewModel.IsFullscreen = !MainViewModel.IsFullscreen;
        }

        #region Accept System Message
        internal static void HandleParameter(string[] args)
        {
            if (Application.Current?.MainWindow is MainWindow mainWindow)
            {
                {
                    string line = string.Join("", args);
                    if (!string.IsNullOrEmpty(line))
                    {
                        MainViewModel.Uri = "http://" + line.Split(':')[1];
                        Log.Debug($"App restared with url: {MainViewModel.Uri}");
                        AppRestarted();
                    }
                    else
                    {
                        var settings = new Settings();
                        settings.Reload();
                        MainViewModel.Uri = settings.LastUri;
                    }
                }
            }
        }

        private static IntPtr HandleMessages(IntPtr handle, int message, IntPtr wParameter,
            IntPtr lParameter, ref Boolean handled)
        {
            if (handle != MainWindow.WindowHandle)
                return IntPtr.Zero;

            var data = UnsafeNative.GetMessage(message, lParameter);

            if (data != null)
            {
                if (Application.Current.MainWindow == null)
                    return IntPtr.Zero;

                var args = data.Split(' ');
                HandleParameter(args);
                handled = true;
            }

            return IntPtr.Zero;
        }

        public void BringToForeground()
        {
            if (this.Visibility == Visibility.Hidden)
                this.Show();
            this.Activate();
            this.Topmost = true;
            this.Topmost = false;
            this.Focus();
        }

        #endregion

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            MainViewModel.WindowHeight = e.NewSize.Height;
            MainViewModel.WindowWidth = e.NewSize.Width;
        }

        private void Grid_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F11)
            {
                ToogleFullScreen();
            }

            if (e.Key == System.Windows.Input.Key.F5)
            {
                MainWebView.Reload();
            }
        }

        #region WebView

        private async void MainWebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            Log.Debug($"Webview completed navigation to {MainViewModel.Uri}. Injecting scripts...");
            MainWebView.CoreWebView2.AddHostObjectToScript("commands", new Commands());
            MainWebView.CoreWebView2.AddHostObjectToScript("faceId", new FaceId());
            MainWebView.CoreWebView2.AddHostObjectToScript("fingerpint", new Fingerprint());

            // Inject JavaScript to disable copy and paste
            string jsCode = @"
                    document.addEventListener('keydown', function(e) {
                        if ((e.ctrlKey || e.metaKey) && (e.key === 'c' || e.key === 'C' || e.key === 'v' || e.key === 'V')) {
                            e.preventDefault();
                        }
                    });
                ";

            await MainWebView.CoreWebView2.ExecuteScriptAsync(jsCode);
            Log.Debug("Scripts are injected into webview");
        }

        private async void MainWebView_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                Log.Debug("Core webview engine initialized");

                await MainWebView.EnsureCoreWebView2Async();

                MainWebView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
                MainWebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                MainWebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            }
            else
            {
                Log.Error("Failed to initialize WebView2 engine", e.InitializationException);
                MessageBox.Show(
                    "Failed to initialize embedded browser (WebView2).\n" +
                    "Please ensure the WebView2 Runtime is installed.",
                    "WebView2 Initialization Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void MainWebView_Initialized(object sender, EventArgs e)
        {
            Log.Debug("Webview component initiazlied");
        }

        private void MainWebView_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            Log.Debug($"Message received from web view {e.ToString}");
        }

        private async void MainWebView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isWebViewAvailable)
            {
                Log.Warn("WebView2 runtime not available. Skipping WebView initialization.");
                return;
            }

            try
            {
                await MainWebView.EnsureCoreWebView2Async();
                MainWebView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
                MainWebView.CoreWebView2.WebResourceRequested += OnWebResourceRequested;
            }
            catch (Exception ex)
            {
                Log.Error("Error ensuring WebView2 core during Loaded", ex);
            }
        }

        private void OnWebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            var request = e.Request;
            var headers = request.Headers;

            headers.SetHeader("HTTP_AJOIBOT_APP_VERSION", "1.0");
        }


        private void printInMainWebView(string message)
        {
            var url = new Uri("data:text/html," + message);
            MainWebView.Source = url;
        }

        private void NavigateToUri()
        {
            Log.Debug($"Navigating to url: {MainViewModel.Uri}");
            if (string.IsNullOrEmpty(MainViewModel.Uri))
            {
                this.printInMainWebView("<h2 style='color:red'>Url was not provided<h2>");
            }
            else
            {
                MainWebView.Source = new Uri(MainViewModel.Uri);
            }
        }
        #endregion
    }
}
