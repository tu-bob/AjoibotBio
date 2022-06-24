using AjoibotBio.Js;
using AjoibotBio.Utils;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using ZKFaceId;
using ZKFaceId.Model;

namespace AjoibotBio.MainWindow
{
    public partial class MainWindow : Window
    {

        private void InitFaceIdCamera(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                //TODO handle errors
                ZKCameraLib.Init();

                if (ZKCameraLib.GetDeviceCount() > 1)
                {
                    MainViewModel.Visible = new ZKCamera(0);
                    MainViewModel.NIR = new ZKCamera(1);

                    MainViewModel.Visible.StartVideoStream();
                    MainViewModel.Visible.NewFrame += OnNewCameraFrame;
                    MainViewModel.Visible.NewCustomData += OnNewCustomData;
                }
            });
        }

        public void OnNewCameraFrame(object sender, VideoData data)
        {
            var imageBase64 = "data:image/bmp;base64,";
            using (Stream stream = new MemoryStream(data.frame))
            {
                BitmapImage image = new BitmapImage();
                stream.Position = 0;
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();
                imageBase64 += BitmapFormat.BitmapToBase64(image);
            }
            this.Dispatcher.Invoke(() =>
            {
                MainWebView.ExecuteScriptAsync($"UpdateFrame('{imageBase64}')");
            });
        }

        public void OnNewCustomData(object sender, CustomData data)
        { 
            this.Dispatcher.Invoke(() =>
            {
                MainWebView.ExecuteScriptAsync($"SetNewBioData({data.bioData}, {data.width}, {data.height})");
            });
        }
        private void MainWebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            MainWebView.CoreWebView2.AddHostObjectToScript("faceId", new FaceId());
        }
    }
}
