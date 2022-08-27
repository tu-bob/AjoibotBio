using log4net;
using System.Collections.Generic;
using ZKFaceId;
using ZKFingerprint;
using ZKFaceId.Interface;

namespace AjoibotBio.MainWindow
{
    public static class MainViewModel
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static ZKCamera? Visible { get; set; }

        public static ZKCamera? NIR { get; set; }

        public static ZKHID? FaceRecognitionHID { get; set; }

        public static List<ZkTeckoScanner> ZkScanners { get; set; }

        public static string Uri { get; set; }

        public static bool IsFullscreen { get; set; }

        public static double WindowsWidth { get; set; }

        public static double WindowsHeight { get; set; }

        public static void CloseDevice(ICloseable closeable, string name)
        {
            try
            {
                var code = closeable.Close();
                Log.Debug($"{name} closed with code {code}");
            }
            catch(System.Exception e)
            {
                Log.Error($"Failed to close {name}", e);
            }

        }

        public static void CloseFaceIdDevices()
        {
            if (NIR != null)
            {
                CloseDevice(NIR, "NIR camera");
                NIR = null;
            }
            if (Visible != null)
            {
                CloseDevice(Visible, "Visible camera");
                Visible = null;
            }
            if (FaceRecognitionHID != null)
            {
                CloseDevice(FaceRecognitionHID, "Face recognition HID");
                FaceRecognitionHID = null;
            }
        }
    }
}
