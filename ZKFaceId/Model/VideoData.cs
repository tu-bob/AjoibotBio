using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZKFaceId.Model
{    public struct VideoData
    {
        public uint frame_index;

        public uint ori_length;

        public uint data_length;

        public IntPtr data;

        public byte[]? frame;
    }
}
