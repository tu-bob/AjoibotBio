using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZKFaceId.Model
{
    public class Tracker
    {        public double blur { get; set; }
        public Landmark landmark { get; set; }
        public Pose pose { get; set; }
        public Rect rect { get; set; }
        public string snapType { get; set; }
        public int trackId { get; set; }
    }
}
