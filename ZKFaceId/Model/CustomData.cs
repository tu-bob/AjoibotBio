using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ZKFaceId.Model
{
    public struct CustomData
    {
        public int frame_index;

        public int width;

        public int height;

        [JsonIgnore]
        public IntPtr customData;

        //json
        public string? bioData;
    }
}
