using System.Runtime.InteropServices;
using System.Windows;

namespace AjoibotBio.Js
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class Commands
    {
        public void ShutdownApp()
        {
            Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
        }
    }
}
