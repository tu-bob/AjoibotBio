using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZKFaceId.Model
{
    public class CustomBioData
    {
        public int frame_index;

        public int width;

        public int height;

        public string bioData;

        public CustomBioData(CustomData data, string bioData)
        {
            frame_index = data.frame_index;
            width = data.width;
            height = data.height;
            this.bioData = bioData;
        }
    }
}
