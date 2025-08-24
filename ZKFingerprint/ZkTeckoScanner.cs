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

        // According to ZKFinger SDK (see ZKFingerprint\Lib\"ZKFinger Reader SDK C#_en_V2.pdf"),
        // the fingerprint template buffer should be at least 2048 bytes. The SDK writes the template
        // into this buffer and returns the actual length via the ref size parameter of AcquireFingerprint.
        // We define a named constant to avoid using a magic number and to document the rationale.
        private const int MaxTemplateSize = 2048;

        // ZKFinger SDK SetParameters codes for indicators/buzzer (model-dependent LED color labels):
        // 101 = LED1 (often White/Blue), 102 = Green, 103 = Red, 104 = Buzzer/Sound.
        // Value 1 turns ON, value 0 turns OFF.
        private const int SignalWhiteOrBlue = 101;
        private const int SignalGreen = 102;
        private const int SignalRed = 103;
        private const int SignalBeep = 104;
        private static readonly int[] AllSignalCodes = new[] { SignalWhiteOrBlue, SignalGreen, SignalRed, SignalBeep };

        private IntPtr DevHandle = IntPtr.Zero;
        private IntPtr DBHandle = IntPtr.Zero;
        private int mfpWidth = 300; //288
        private int mfpHeight = 300; //369
        private int cbCapTmp = MaxTemplateSize;
        private readonly byte[] CapTmp = new byte[MaxTemplateSize];
        private readonly int SignalDuration = 1000;

        private byte[] FpBuffer { get; set; }

        //AEngine
        //private string PrintTmp { get; set; }
        //private int PointsTmp { get; set; }
        public int DeviceIndex { get; set; }

        private volatile bool _isScanningOn = false;
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
                // Ensure capture loop stops before attempting to close the device
                if (_isScanningOn)
                {
                    Log.Info($"DisconnectDevice: stopping capture loop (index={DeviceIndex}).");
                    _isScanningOn = false;
                }

                Log.Info($"Disconnecting fingerprint device (index={DeviceIndex}, handle={DevHandle})...");
                if (DevHandle == IntPtr.Zero)
                {
                    Log.Warn("DisconnectDevice called but device handle is zero. Nothing to close.");
                    return true;
                }
                // Turn off all signals
                foreach (var code in AllSignalCodes)
                {
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
                DevHandle = IntPtr.Zero;
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

                    _isScanningOn = true;
                    while (_isScanningOn)
                    {
                        FpBuffer = new byte[mfpWidth * mfpHeight];
                        // Reset template size before each capture to the maximum buffer size. The SDK will update
                        // templateSize with the actual number of bytes written.
                        int templateSize = MaxTemplateSize;
                        int ret = zkfp2.AcquireFingerprint(DevHandle, FpBuffer, CapTmp, ref templateSize);
                        if (ret == zkfp.ZKFP_ERR_OK)
                        {
                            _acquireFailCount = 0; // reset on success
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
                            // Use the actual template size returned by the SDK
                            var fingerPrint = Convert.ToBase64String(CapTmp, 0, templateSize);
                            Log.Info($"Fingerprint acquired successfully (index={DeviceIndex}, imageSize={imageBase64?.Length ?? 0}, templateSize={templateSize}).");
                            Application.Current.Dispatcher.Invoke(() => FingerPrintCaptured?.Invoke(DeviceIndex, imageBase64, fingerPrint));
                        }
                        else
                        {
                            _acquireFailCount++;
                            if (_acquireFailCount % 20 == 1) // throttle logs to avoid flooding
                            {
                                Log.Debug($"AcquireFingerprint returned {ret} (index={DeviceIndex}), failCount={_acquireFailCount}.");
                            }
                            if (_acquireFailCount >= 200)
                            {
                                Log.Error($"AcquireFingerprint failed {_acquireFailCount} times consecutively. Assuming device disconnected (index={DeviceIndex}). Raising DeviceDisconnected and stopping capture.");
                                try
                                {
                                    Application.Current?.Dispatcher?.Invoke(() => DeviceDisconnected?.Invoke());
                                }
                                catch (Exception exInvoke)
                                {
                                    Log.Warn("Exception while invoking DeviceDisconnected event.", exInvoke);
                                }
                                _isScanningOn = false;
                                break;
                            }
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
                // Free DB handle if initialized
                try
                {
                    if (DBHandle != IntPtr.Zero)
                    {
                        zkfp2.DBFree(DBHandle);
                        DBHandle = IntPtr.Zero;
                    }
                }
                catch (Exception ex)
                {
                    Log.Warn("Exception while freeing DB handle.", ex);
                }

                // Close device on exit of capture logic
                DisconnectDevice();
            }
        }

        public void StopCapture()
        {
            try
            {
                Log.Info($"StopCapture requested (index={DeviceIndex}).");
                _isScanningOn = false;
            }
            catch (Exception ex)
            {
                Log.Warn("Exception while stopping capture.", ex);
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
            Log.Debug("ResetSignals: turning off indicator and buzzer signals.");
            foreach (var code in AllSignalCodes)
            {
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
                if (DevHandle == IntPtr.Zero)
                {
                    Log.Warn($"SignalOn ignored: device handle is zero (code={code}).");
                    return;
                }
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
                if (DevHandle == IntPtr.Zero)
                {
                    Log.Warn($"SignalOff ignored: device handle is zero (code={code}).");
                    return;
                }
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
