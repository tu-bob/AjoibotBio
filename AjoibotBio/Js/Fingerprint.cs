using AjoibotBio.MainWindow;
using System.Runtime.InteropServices;

namespace AjoibotBio.Js
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class Fingerprint
    {
        public bool IsDeviceConnected()
        {
            return MainViewModel.ZkScanners.Count > 0;
        }

        public int CountDevices()
        {
            return MainViewModel.ZkScanners.Count;
        }
    }
}
