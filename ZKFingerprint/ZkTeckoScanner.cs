using libzkfpcsharp;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ZKFingerprint
{
    public class ZkTeckoScanner
    {
        #region Parameters

        private IntPtr DevHandle = IntPtr.Zero;
        private IntPtr DBHandle = IntPtr.Zero;
        private int mfpWidth = 300; //288
        private int mfpHeight = 300; //369
        private int cbCapTmp = 2048;
        private readonly byte[] CapTmp = new byte[2048];
        private readonly int SignalDuration = 1000;

        private byte[] FpBuffer { get; set; }

        //AEngine
        //private string PrintTmp { get; set; }
        //private int PointsTmp { get; set; }
        public int DeviceIndex { get; set; }

        private readonly bool IsScanningOn = true;

        #endregion

        #region Events
        public delegate void PrintImageCapturedHandler(int index, string image, string print);
        public event PrintImageCapturedHandler FingerPrintCaptured;

        public delegate void DeviceDisconnectedHandler();
        public event DeviceDisconnectedHandler DeviceDisconnected;
        #endregion

        public bool ConnectDevice()
        {
            //TODO replace event
            //GlobalVariables.DevHandles.Remove(DevHandle);
            if (IntPtr.Zero == (DevHandle = zkfp2.OpenDevice(DeviceIndex)))
            {
                if (DeviceDisconnected != null)
                    Application.Current.Dispatcher.Invoke(DeviceDisconnected);
                return false;
            }
            //GlobalVariables.DevHandles.Add(DevHandle);
            return true;
        }

        public bool DisconnectDevice()
        {
            if (zkfp2.CloseDevice(DevHandle) == 0)
            {
                //TODO replace with event
                //GlobalVariables.DevHandles.Remove(DevHandle);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Acquire finger print image and print template CapTmp
        /// </summary>
        /// <param name="image">New Bitmap image</param>
        /// <returns>Print 0 if Ok, and -1 if device is unreachable</returns>
        public void CapturePrint()
        {
            if (ConnectDevice())
            {
                if (IntPtr.Zero != (DBHandle = zkfp2.DBInit()))
                {
                    byte[] paramValue = new byte[4];
                    int size = 4;
                    zkfp2.GetParameters(DevHandle, 1, paramValue, ref size);
                    zkfp2.ByteArray2Int(paramValue, ref mfpWidth);

                    size = 4;
                    zkfp2.GetParameters(DevHandle, 2, paramValue, ref size);
                    zkfp2.ByteArray2Int(paramValue, ref mfpHeight);

                    while (IsScanningOn)
                    {
                        FpBuffer = new byte[mfpWidth * mfpHeight];
                        int ret = zkfp2.AcquireFingerprint(DevHandle,
                            FpBuffer, CapTmp, ref cbCapTmp);
                        if (ret == zkfp.ZKFP_ERR_OK)
                        {
                            var image = new BitmapImage();
                            var mem = new MemoryStream();
                            BitmapFormat.GetBitmap(FpBuffer, mfpWidth, mfpHeight, ref mem);
                            mem.Position = 0;
                            image.BeginInit();
                            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.UriSource = null;
                            image.StreamSource = mem;
                            image.EndInit();
                            image.Freeze();
                            mem.Flush();
                            mem.Dispose();
                            var imageBase64 = BitmapFormat.BitmapToBase64(image);
                            var fingerPrint = Convert.ToBase64String(CapTmp);
                            Application.Current.Dispatcher.Invoke(() => FingerPrintCaptured(DeviceIndex, imageBase64, fingerPrint));
                        }
                        Thread.Sleep(50);
                    }
                }
            }
            if (DeviceDisconnected != null)
                Application.Current.Dispatcher.Invoke(DeviceDisconnected);
        }

        /// <summary>
        /// Alert signal with preset duration
        /// </summary>
        /// <param name="code"></param>
        /// <param name="duration"></param>
        public void AlertSignal(int code, int duration)
        {
            SignalOn(code);
            Thread.Sleep(duration);
            SignalOff(code);
        }

        private void ResetSignals()
        {
            for (int i = 101; i < 105; i++)
            {
                int code = i;
                SignalOff(code);
            }
        }

        /// <summary>
        /// Activate light or sound on divice
        /// </summary>
        /// <param name="code">White = 101, Green = 102, Red = 103, Sound = 104</param>
        public void SignalOn(int code)
        {
            byte[] turnOn = new byte[4];
            zkfp2.Int2ByteArray(1, turnOn);
            var ret = zkfp2.SetParameters(DevHandle, code, turnOn, 4);
        }

        public void SignalOff(int code)
        {
            byte[] turnOff = new byte[4];
            zkfp2.Int2ByteArray(0, turnOff);
            var ret = zkfp2.SetParameters(DevHandle, code, turnOff, 4);
        }
    }
}
