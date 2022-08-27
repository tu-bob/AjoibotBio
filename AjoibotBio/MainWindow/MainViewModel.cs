using System.Collections.Generic;
using ZKFaceId;
using ZKFingerprint;

namespace AjoibotBio.MainWindow
{
    public static class MainViewModel
    {
        public static ZKCamera? Visible { get; set; }

        public static ZKCamera? NIR { get; set; }

        public static List<ZkTeckoScanner> ZkScanners { get; set; }

        public static string Uri {get;set;}

        public static bool IsFullscreen { get; set; }
        public static double WindowsWidth { get; set; }
        public static double WindowsHeight { get; set; }
    }
}
