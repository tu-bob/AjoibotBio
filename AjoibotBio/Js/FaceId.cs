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
        public string Register()
        {
           return MainViewModel.Visible.RegisterFace();
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
