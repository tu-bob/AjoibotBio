using DemoCamApp.json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DemoCamApp
{
    internal class ZKHID
    {
        private IntPtr Handle;

        #region C++ library import

        private const string PathToDll = "lib/x64/ZKHIDLib.dll";


        [DllImport(PathToDll)]
        private static extern int ZKHID_Open(int index, out IntPtr handle);

        [DllImport(PathToDll)]
        private static extern int ZKHID_Close(IntPtr handle);

        [DllImport(PathToDll)]
        private static extern int ZKHID_GetConfig(IntPtr handle, int type, out string json,out int len);

        [DllImport(PathToDll)]
        private static extern int ZKHID_SetConfig(IntPtr handle, int type, string json);

        [DllImport(PathToDll, CallingConvention = CallingConvention.StdCall)]
        private static extern int ZKHID_RegisterFace(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string json, StringBuilder faceData, out int len);

        [DllImport(PathToDll)]
        private static extern int ZKHID_SnapShot(IntPtr handle, int snapType, StringBuilder snapData, out int size);

        #endregion
        
        public int Open(int index)
        {
            return ZKHID_Open(index, out Handle);
        }

        public int Close()
        {
            return ZKHID_Close(Handle);
        }

        public void RegisterFace()
        {
            int length = 1024 * 1024;
       
            var config = new RegisterFaceConfig(true, true, true);
            string json = JsonSerializer.Serialize(config);

            StringBuilder faceData = new StringBuilder(new String(' ', length));

            //int size = 2 * 1024 * 1024;
            //var snapData = new StringBuilder(new String(' ', size));

            //var image = ZKHID_SnapShot(Handle, 0, snapData, out size);

            var res = ZKHID_RegisterFace(Handle, json, faceData, out length);

            RegistrationData registrationData = JsonSerializer.Deserialize<RegistrationData>(faceData.ToString());
            var m = registrationData.data;
            var ress = faceData.ToString().Trim();
        }
    }
}
