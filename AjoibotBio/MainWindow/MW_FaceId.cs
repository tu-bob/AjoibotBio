using AjoibotBio.Js;
using AjoibotBio.Utils;
using System;
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
                var result = ZKCameraLib.Init();

                if (result == 0)
                {
                    Log.Debug("CameraLib initialized");
                    var count = ZKCameraLib.GetDeviceCount();
                    if (count > 1)
                    {
                        try
                        {
                            //MainViewModel.Visible = new ZKCamera(0);
                            MainViewModel.Visible = ZKCameraLib.GetDeviceType(0) == 1? new ZKCamera(0) : new ZKCamera(1);
                            MainViewModel.NIR = ZKCameraLib.GetDeviceType(0) == 1 ? new ZKCamera(1) : new ZKCamera(0);

                            MainViewModel.Visible.InitHID();
                            MainViewModel.Visible.StartVideoStream();
                            MainViewModel.Visible.NewFrame += OnNewCameraFrame;
                            MainViewModel.Visible.NewCustomData += OnNewCustomData;
                        }
                        catch (Exception e)
                        {
                            Log.Fatal("Failed to initialize Face Recognition", e);

                            if (MainViewModel.Visible != null)
                            {
                                var closeVisibleCode = MainViewModel.Visible.CloseDevice();
                                MainViewModel.Visible = null;
                                Log.Debug($"Visible camera closed with code {closeVisibleCode}");
                            }

                            if(MainViewModel.NIR != null)
                            {
                                var closeNIRCode = MainViewModel.NIR?.CloseDevice();
                                MainViewModel.NIR = null;
                                Log.Debug($"NIR camera closed with code {closeNIRCode}");
                            }
                        }
                    }
                    else
                    {
                        Log.Debug("No camera connected. Device count equals to " + count);
                    }

                }
                else
                {
                    Log.Error("Failed to init CameraLib.  Error code " + result);
                }

            });
        }

        public void OnNewCameraFrame(object sender, byte[] frame)
        {
            var imageBase64 = "data:image/bmp;base64,";
            using (Stream stream = new MemoryStream(frame))
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

        public void OnNewCustomData(object sender, CustomBioData data)
        { 
            this.Dispatcher.Invoke(() =>
            {
                MainWebView.ExecuteScriptAsync($"SetNewBioData({data.bioData}, {data.width}, {data.height})");
            });
        }
    }
}
