using System.Runtime.InteropServices;

namespace ZKFaceId
{
    public class ZKCameraLib
    {
        private const string PathToDll = "lib/x86/ZKCameraLib.dll";

        [DllImport(PathToDll)]
        private static extern int ZKCamera_Init();

        [DllImport(PathToDll)]
        private static extern int ZKCamera_Terminate();

        [DllImport(PathToDll)]
        private static extern int ZKCamera_GetDeviceCount();

        [DllImport(PathToDll)]
        private static extern int ZKCamera_GetDeviceType(int index);

        public static int Init()
        {
            return ZKCamera_Init();
        }

        public static int Terminate()
        {
            return ZKCamera_Terminate();
        }
        public static int GetDeviceCount()
        {
            return ZKCamera_GetDeviceCount();
        }

        public static int GetDeviceType(int index)
        {
            return ZKCamera_GetDeviceType(index);
        }
    }
}
