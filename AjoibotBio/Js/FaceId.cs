using AjoibotBio.MainWindow;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using ZKFaceId.Model;

namespace AjoibotBio.Js
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class FaceId
    {
        public string Register(string config)
        {
           return MainViewModel.Visible.RegisterFace(config);
        }

        public int SetConfig(int type, string json)
        {
            return MainViewModel.Visible.SetConfig(type, json);
        }

        public string PollMatchResult()
        {
            return MainViewModel.Visible.PollMatchResult();
        }

        public string ManageModuleData(int type, string json)
        {
            return MainViewModel.Visible.ManageModuleData(type, json);
        }
    }
}
