using System;

namespace ZKFaceId.Model
{    public struct VideoData
    {
        public uint frame_index;

        public uint ori_length;

        public uint data_length;

        public IntPtr data;

        //public byte[]? frame;
    }
}
