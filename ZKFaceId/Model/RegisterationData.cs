using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoCamApp.json
{
    public class RegistrationData
    {
        public Data data { get; set; }

        public string detail { get; set; }

        public int status { get; set; }
    }

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Attribute
        {
            public int age { get; set; }
            public int beauty { get; set; }
            public int cap { get; set; }
            public int expression { get; set; }
            public int eye { get; set; }
            public int gender { get; set; }
            public int glasses { get; set; }
            public int mouth { get; set; }
            public int mustache { get; set; }
            public int nation { get; set; }
            public int respirator { get; set; }
            public int respiratorLevel { get; set; }
            public int skinColor { get; set; }
            public int smile { get; set; }
        }

        public class Data
        {
            public List<Face> faces { get; set; }
        }

        public class Face
        {
            public FaceInfo faceInfo { get; set; }
            public Feature feature { get; set; }
            public Picture picture { get; set; }
        }

        public class FaceInfo
        {
            public Attribute attribute { get; set; }
            public Landmark landmark { get; set; }
            public Pose pose { get; set; }
            public Rect rect { get; set; }
            public double score { get; set; }
        }

        public class Feature
        {
            public string bioType { get; set; }
            public string data { get; set; }
            public int size { get; set; }
        }

        public class Landmark
        {
            public int count { get; set; }
            public string data { get; set; }
        }

        public class Picture
        {
            public string bioType { get; set; }
            public string data { get; set; }
            public string format { get; set; }
            public int height { get; set; }
            public int width { get; set; }
        }

        public class Pose
        {
            public double pitch { get; set; }
            public double roll { get; set; }
            public double yaw { get; set; }
        }

        public class Rect
        {
            public int bottom { get; set; }
            public int left { get; set; }
            public int right { get; set; }
            public int top { get; set; }
        }

        public class Root
        {
            public Data data { get; set; }
            public string detail { get; set; }
            public int status { get; set; }
        }
}
