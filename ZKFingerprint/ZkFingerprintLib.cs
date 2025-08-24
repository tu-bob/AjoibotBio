using libzkfpcsharp;

namespace ZKFingerprint
{
    public static class ZkFingerprintLib
    {
        // Expose the common pieces used by the app without requiring it to reference the vendor namespace directly
        public static readonly int ZKFP_ERR_OK = zkfperrdef.ZKFP_ERR_OK;

        public static int Init()
        {
            return zkfp2.Init();
        }

        public static void Terminate()
        {
            zkfp2.Terminate();
        }

        public static int GetDeviceCount()
        {
            return zkfp2.GetDeviceCount();
        }
    }
}
