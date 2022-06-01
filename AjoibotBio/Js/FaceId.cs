using AjoibotBio.MainWindow;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;

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
    }
}
