using AjoibotBio.Utils;
using System;
using System.Diagnostics;
using System.IO;
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
                }
            });
        }

        public void OnNewCameraFrame(object sender, VideoData data)
        {
            var imageBase64 = "";
            using (Stream stream = new MemoryStream(data.frame))
            {
                BitmapImage image = new BitmapImage();
                stream.Position = 0;
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();
                imageBase64 = BitmapFormat.BitmapToBase64(image);
            }
            this.Dispatcher.Invoke(() =>
            {
                MainWebView.ExecuteScriptAsync($"UpdateFrame('{imageBase64}')");
            });
        }
    }
}
