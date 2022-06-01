using DemoCamApp.json;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using ZKFaceId.Model;

namespace ZKFaceId
{
    public class ZKCamera
    {
        #region C++ library import

        private const string PathToDll = "lib/x64/ZKCameraLib.dll";

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

        public delegate void VideoDataCallback(IntPtr pUserParam, VideoData data);

        public delegate void CustomDataCallback(IntPtr pUserParam, CustomData data);

        public event EventHandler<VideoData> NewFrame;

        private int Index { get; set; }
        private int Width { get; set; }
        private int Height { get; set; }
        private int Fps { get; set; }

        private IntPtr Handle { get; set; }

        public ZKCamera(int index)
        {
            Index = index;

            Width = 720;

            Height = 1280;

            Fps = 25;

            var handle = IntPtr.Zero;
            //TODO handle errors
            OpenDevice(out handle);

            Handle = handle;
        }

        private int OpenDevice(out IntPtr handle)
        {
            return ZKCamera_OpenDevice(Index, Width, Height, Fps, out handle);
        }

        public int CloseDevice()
        {
            return ZKCamera_CloseDevice(Handle);
        }

        public void FreePointer(IntPtr pPointr)
        {
            ZKCamera_FreePointer(pPointr);
        }

        public void OnGetVideoData(IntPtr pUserParam, VideoData data)
        {
            data.frame = new byte[data.data_length];
            Marshal.Copy(data.data, data.frame, 0, (int)data.data_length);
            EventHandler<VideoData> handler = NewFrame;
            if (handler != null) handler(this, data);
            FreePointer(data.data);
        }


        public void OnGetCustomData(IntPtr pUserParam, CustomData data)
        {

        }

        public void StartVideoStream()
        {
            VideoCallbackDelegate = new VideoDataCallback(OnGetVideoData);
            var customDelegate = new CustomDataCallback(OnGetCustomData);
            var pUserParam = 1;
            ZKCamera_SetDataCallback(Handle, Marshal.GetFunctionPointerForDelegate(VideoCallbackDelegate), IntPtr.Zero, (IntPtr)pUserParam);
        }


        public RegistrationData RegisterFace()
        {
            var hid = new ZKHID(0);

            hid.StartDevice();

            var data = hid.RegisterFace(new RegisterFaceConfig(true, true, true));

            hid.Close();

            return data;
        }
    }
}
