using System;

namespace DemoCamApp.json
{
    public class RegisterFaceConfig
    {
        public bool feature { get; set;}
        public bool faceInfo {get;set;}
        public bool picture { get; set; }


        public RegisterFaceConfig(bool feature, bool faceInfo, bool picture)
        {
            this.feature = feature;
            this.faceInfo = faceInfo;
            this.picture = picture;
        }
    }
}
