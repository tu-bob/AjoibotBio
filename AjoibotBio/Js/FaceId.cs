using AjoibotBio.MainWindow;
using System.Runtime.InteropServices;

namespace AjoibotBio.Js
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class FaceId
    {
        public string? Register(string config)
        {
           return MainViewModel.FaceRecognitionHID?.RegisterFace(config);
        }

        public int? SetConfig(int type, string json)
        {
            return MainViewModel.FaceRecognitionHID?.SetConfig(type, json);
        }

        public string? PollMatchResult()
        {
            return MainViewModel.FaceRecognitionHID?.PollMatchResult();
        }

        public string? ManageModuleData(int type, string json)
        {
            return MainViewModel.FaceRecognitionHID?.ManageModuleData(type, json);
        }
    }
}
