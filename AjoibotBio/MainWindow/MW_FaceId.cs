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
                            MainViewModel.Visible = ZKCameraLib.GetDeviceType(0) == 1 ? new ZKCamera(0) : new ZKCamera(1);
                            MainViewModel.NIR = ZKCameraLib.GetDeviceType(0) == 1 ? new ZKCamera(1) : new ZKCamera(0);

                            MainViewModel.Visible.StartVideoStream();
                            MainViewModel.Visible.NewFrame += OnNewCameraFrame;
                            MainViewModel.Visible.NewCustomData += OnNewCustomData;

                            MainViewModel.FaceRecognitionHID = new ZKHID(0);
                            MainViewModel.FaceRecognitionHID.Start();
                        }
                        catch (Exception e)
                        {
                            Log.Fatal("Failed to initialize Face Recognition", e);

                            MainViewModel.CloseFaceIdDevices();

                            if (MainViewModel.NIR != null)
                            {
                                var closeNIRCode = MainViewModel.NIR.Close();
                                MainViewModel.NIR = null;
                                Log.Debug($"NIR camera closed with code {closeNIRCode}");
                            }

                            if (MainViewModel.FaceRecognitionHID != null)
                            {
                                var closeHIDCode = MainViewModel.FaceRecognitionHID.Close();
                                MainViewModel.FaceRecognitionHID = null;
                                Log.Debug($"HID device closed with code {closeHIDCode}");
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

            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    MainWebView.ExecuteScriptAsync($"UpdateFrame('{imageBase64}')");
                });
            }
            catch(Exception e)
            {
                Log.Error("Failed to call UpdateFrame function", e);
            }
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
