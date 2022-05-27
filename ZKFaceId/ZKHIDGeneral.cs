using System;
using System.Runtime.InteropServices;

namespace ZKFaceId
{
    public class ZKHIDGeneral
    {
        #region C++ library import

        private const string PathToDll = "lib/x64/ZKHIDLib.dll";

        [DllImport(PathToDll)]
        private static extern int ZKHID_Init();

        [DllImport(PathToDll)]
        private static extern int ZKHID_Terminate();

        [DllImport(PathToDll)]
        private static extern int ZKHID_GetCount(out int count);
        #endregion

        public int Init()
        {
            return ZKHID_Init();
        }
        public int Terminate(int index)
        {
            return ZKHID_Terminate();
        }

        public int GetCount(out int count)
        {
            return ZKHID_GetCount(out count);
        }
    }
}
