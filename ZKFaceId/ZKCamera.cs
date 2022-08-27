using System;
using System.Runtime.InteropServices;
using ZKFaceId.Interface;
using ZKFaceId.Model;

namespace ZKFaceId
{
    public class ZKCamera : ICloseable
    {
        #region C++ library import

        private const string PathToDll = "lib/x86/ZKCameraLib.dll";

        [DllImport(PathToDll)]
        private static extern int ZKCamera_OpenDevice(int index, int w, int h, int fps, out IntPtr handle);

        [DllImport(PathToDll)]
        private static extern int ZKCamera_CloseDevice(IntPtr handle);

        [DllImport(PathToDll)]
        private static extern void ZKCamera_SetDataCallback(IntPtr handle, IntPtr videoCallback, IntPtr customCallback, IntPtr pUserParam);

        [DllImport(PathToDll)]
        private static extern void ZKCamera_FreePointer(IntPtr pPointer);
        #endregion

        private VideoDataCallback VideoCallbackDelegate;

        private CustomDataCallback CustomCallbackDelegate;

        public delegate void VideoDataCallback(IntPtr pUserParam, VideoData data);

        public delegate void CustomDataCallback(IntPtr pUserParam, CustomData data);

        public event EventHandler<byte[]> NewFrame;

        public event EventHandler<CustomBioData> NewCustomData;

        private int Index { get; set; }
        private int Width { get; set; }
        private int Height { get; set; }
        private int Fps { get; set; }

        private IntPtr Handle;

        //private ZKHID? HID { get; set; }

        public ZKCamera(int index)
        {
            Index = index;

            Width = 720;

            Height = 1280;

            Fps = 25;

            if (OpenDevice(out Handle) != 0)
                throw new Exception("Failed to init camera with index " + index);
        }

        private int OpenDevice(out IntPtr handle)
        {
            var res = ZKCamera_OpenDevice(Index, Width, Height, Fps, out handle);

            if (res != 0)
                throw new Exception("Failed to open device with index: " + Index);

            return res;
        }

        public int Close()
        {
            return ZKCamera_CloseDevice(Handle);
        }

        public void FreePointer(IntPtr pPointr)
        {
            ZKCamera_FreePointer(pPointr);
        }

        public void OnGetVideoData(IntPtr pUserParam, VideoData data)
        {
            var frame = new byte[data.data_length];
            Marshal.Copy(data.data, frame, 0, (int)data.data_length);
            FreePointer(data.data);
            EventHandler<byte[]> handler = NewFrame;
            if (handler != null) handler(this, frame);

        }

        //Throws error with x86 lib
        public void OnGetCustomData(IntPtr pUserParam, CustomData data)
        {
            //var cBioData = new CustomBioData(data, Marshal.PtrToStringUTF8(data.customData));
            //EventHandler<CustomBioData> handler = NewCustomData;
            //if (handler != null) handler(this, cBioData);
            //FreePointer(data.customData);
        }

        public void StartVideoStream()
        {
            VideoCallbackDelegate = new VideoDataCallback(OnGetVideoData);
            CustomCallbackDelegate = new CustomDataCallback(OnGetCustomData);
            var pUserParam = 1;
            ZKCamera_SetDataCallback(
                Handle,
                Marshal.GetFunctionPointerForDelegate(VideoCallbackDelegate),
                //Marshal.GetFunctionPointerForDelegate(CustomCallbackDelegate),
                IntPtr.Zero,
                //IntPtr.Zero,
                (IntPtr)pUserParam
                );
        }
    }
}
