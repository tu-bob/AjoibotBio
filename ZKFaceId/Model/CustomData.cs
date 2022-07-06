using System;

namespace ZKFaceId.Model
{
    public struct CustomData
    {
        public int frame_index;

        public int width;

        public int height;
 
        public IntPtr customData;

        //json
        //public string? bioData;
    }
}
