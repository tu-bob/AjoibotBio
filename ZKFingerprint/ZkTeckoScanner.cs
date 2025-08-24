using libzkfpcsharp;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using log4net;

namespace ZKFingerprint
{
    public class ZkTeckoScanner
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ZkTeckoScanner));

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
        private int _acquireFailCount = 0;

        #endregion

        #region Events
        public delegate void PrintImageCapturedHandler(int index, string image, string print);
        public event PrintImageCapturedHandler FingerPrintCaptured;

        public delegate void DeviceDisconnectedHandler();
        public event DeviceDisconnectedHandler DeviceDisconnected;
        #endregion

        public bool ConnectDevice()
        {
            try
            {
                Log.Info($"Attempting to open fingerprint device at index {DeviceIndex}...");
                if (IntPtr.Zero == (DevHandle = zkfp2.OpenDevice(DeviceIndex)))
                {
                    Log.Error($"Failed to open fingerprint device (index={DeviceIndex}). Returned handle is zero. Ensure drivers are installed and the device is connected.");
                    return false;
                }
                Log.Info($"Fingerprint device opened successfully (index={DeviceIndex}, handle={DevHandle}).");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Exception while opening fingerprint device (index={DeviceIndex}).", ex);
                return false;
            }
        }

        public bool DisconnectDevice()
        {
            try
            {
                Log.Info($"Disconnecting fingerprint device (index={DeviceIndex}, handle={DevHandle})...");
                // Turn off all signals
                for (int i = 101; i < 105; i++)
                {
                    int code = i;
                    byte[] turnOff = new byte[4];
                    zkfp2.Int2ByteArray(0, turnOff);
                    var ret = zkfp2.SetParameters(DevHandle, code, turnOff, 4);
                    Log.Debug($"SignalOff during disconnect: code={code}, ret={ret}");
                }

                int closeRet = zkfp2.CloseDevice(DevHandle);
                bool success = (closeRet == 0);
                if (success)
                {
                    Log.Info($"Fingerprint device disconnected successfully (index={DeviceIndex}).");
                }
                else
                {
                    Log.Error($"Failed to close fingerprint device (index={DeviceIndex}). CloseDevice returned {closeRet}.");
                }
                return success;
            }
            catch (Exception ex)
            {
                Log.Error($"Exception while disconnecting fingerprint device (index={DeviceIndex}).", ex);
                return false;
            }
        }

        /// <summary>
        /// Acquire finger print image and print template CapTmp
        /// </summary>
        public void CapturePrint()
        {
            try
            {
                if (!ConnectDevice())
                {
                    Log.Error($"CapturePrint aborted: unable to connect to device (index={DeviceIndex}).");
                    return;
                }

                Log.Debug("Initializing fingerprint database (DBInit)...");
                DBHandle = zkfp2.DBInit();
                if (IntPtr.Zero == DBHandle)
                {
                    Log.Error("DBInit returned null handle. Cannot proceed with fingerprint capture.");
                    return;
                }

                try
                {
                    byte[] paramValue = new byte[4];
                    int size = 4;
                    int ret1 = zkfp2.GetParameters(DevHandle, 1, paramValue, ref size);
                    zkfp2.ByteArray2Int(paramValue, ref mfpWidth);

                    size = 4;
                    int ret2 = zkfp2.GetParameters(DevHandle, 2, paramValue, ref size);
                    zkfp2.ByteArray2Int(paramValue, ref mfpHeight);

                    Log.Info($"Fingerprint capture parameters: width={mfpWidth} (ret={ret1}), height={mfpHeight} (ret={ret2}), cbCapTmp={cbCapTmp}.");

                    while (IsScanningOn)
                    {
                        FpBuffer = new byte[mfpWidth * mfpHeight];
                        int ret = zkfp2.AcquireFingerprint(DevHandle, FpBuffer, CapTmp, ref cbCapTmp);
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
                            Log.Info($"Fingerprint acquired successfully (index={DeviceIndex}, imageSize={imageBase64?.Length ?? 0}, templateSize={cbCapTmp}).");
                            Application.Current.Dispatcher.Invoke(() => FingerPrintCaptured?.Invoke(DeviceIndex, imageBase64, fingerPrint));
                        }
                        Thread.Sleep(50);
                    }
                }
                catch (Exception exInner)
                {
                    Log.Error("Exception during fingerprint acquisition loop.", exInner);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Unexpected exception in CapturePrint.", ex);
            }
            finally
            {
                // Optionally close on exit of capture logic
                DisconnectDevice();
            }
        }

        /// <summary>
        /// Alert signal with preset duration
        /// </summary>
        public void AlertSignal(int code, int duration)
        {
            Log.Debug($"AlertSignal: code={code}, duration={duration}ms.");
            SignalOn(code);
            Thread.Sleep(duration);
            SignalOff(code);
        }

        private void ResetSignals()
        {
            Log.Debug("ResetSignals: turning off codes 101..104.");
            for (int i = 101; i < 105; i++)
            {
                int code = i;
                SignalOff(code);
            }
        }

        /// <summary>
        /// Activate light or sound on device
        /// </summary>
        /// <param name="code">White = 101, Green = 102, Red = 103, Sound = 104</param>
        public void SignalOn(int code)
        {
            try
            {
                byte[] turnOn = new byte[4];
                zkfp2.Int2ByteArray(1, turnOn);
                var ret = zkfp2.SetParameters(DevHandle, code, turnOn, 4);
                Log.Debug($"SignalOn: code={code}, ret={ret}.");
            }
            catch (Exception ex)
            {
                Log.Error($"Exception in SignalOn (code={code}).", ex);
            }
        }

        public void SignalOff(int code)
        {
            try
            {
                byte[] turnOff = new byte[4];
                zkfp2.Int2ByteArray(0, turnOff);
                var ret = zkfp2.SetParameters(DevHandle, code, turnOff, 4);
                Log.Debug($"SignalOff: code={code}, ret={ret}.");
            }
            catch (Exception ex)
            {
                Log.Error($"Exception in SignalOff (code={code}).", ex);
            }
        }
    }
}
