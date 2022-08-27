using DemoCamApp.json;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ZKFaceId
{
    public class ZKHID
    {

        #region C++ library import

        private const string PathToDll = "lib/x86/ZKHIDLib.dll";


        [DllImport(PathToDll)]
        private static extern int ZKHID_Open(int index, out IntPtr handle);

        [DllImport(PathToDll)]
        private static extern int ZKHID_Close(IntPtr handle);

        [DllImport(PathToDll)]
        private static extern int ZKHID_Init();

        [DllImport(PathToDll)]
        private static extern int ZKHID_Terminate();

        [DllImport(PathToDll)]
        private static extern int ZKHID_GetCount(out int count);

        [DllImport(PathToDll)]
        private static extern int ZKHID_GetConfig(IntPtr handle, int type, out string json, out int len);

        [DllImport(PathToDll)]
        private static extern int ZKHID_SetConfig(IntPtr handle, int type, string json);

        [DllImport(PathToDll, CallingConvention = CallingConvention.StdCall)]
        private static extern int ZKHID_RegisterFace(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string json, StringBuilder faceData, out int len);

        [DllImport(PathToDll)]
        private static extern int ZKHID_SnapShot(IntPtr handle, int snapType, StringBuilder snapData, out int size);

        [DllImport(PathToDll)]
        private static extern int ZKHID_PollMatchResult(IntPtr handle, StringBuilder json, out int len);

        [DllImport(PathToDll)]
        private static extern int ZKHID_ManageModuleData(IntPtr handle, int type, string json, StringBuilder result, out int len);

        #endregion

        private int Index;

        private IntPtr Handle;

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public ZKHID(int index)
        {
            Index = index;
        }
        public int Init()
        {
            return ZKHID_Init();
        }
        public int Terminate()
        {
            return ZKHID_Terminate();
        }

        public void StartDevice()
        {
            var initialized = Init();
            if (initialized != 0)
                throw new Exception($"Failed to init HIDLibrary. Code {initialized}");

            var open = Open();
            if (open != 0)
                throw new Exception($"Failed to open HID device. Code {open}");
        }

        public int GetCount(out int count)
        {
            return ZKHID_GetCount(out count);
        }

        public int SetConfig(int type, string json)
        {
            return ZKHID_SetConfig(Handle, type, json);
        }

        public int Open()
        {
            return ZKHID_Open(Index, out Handle);
        }

        public int Close()
        {
            int res = -100;

            try
            {
                res = ZKHID_Close(Handle);
            }
            catch (Exception)
            {
            }

            return res;
        }

        public string RegisterFace(string config)
        {
            int length = 20 * 1024 * 1024;

            StringBuilder faceData = new StringBuilder(new String(' ', length));

            var res = ZKHID_RegisterFace(Handle, config, faceData, out length);

            return faceData.ToString();
        }

        public string ManageModuleData(int type, string json)
        {
            int length = 20 * 1024 * 1024;

            StringBuilder result = new StringBuilder(new String(' ', length));

            var res = ZKHID_ManageModuleData(Handle, type, json, result, out length);

            return result.ToString();
        }

        public string PollMatchResult()
        {
            int length = 40 * 1024;

            StringBuilder json = new StringBuilder(new String(' ', length));

            var res = ZKHID_PollMatchResult(Handle, json, out length);

            return json.ToString();
        }
    }
}
