using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using libzkfpcsharp;
using ZKFingerprint;

namespace AjoibotBio.MainWindow
{
    public partial class MainWindow : Window
    {
        public void InitFingerprintScanner(object sender, EventArgs e)
        {
            MainViewModel.ZkScanners = new List<ZkTeckoScanner>();

            Task.Run(() =>
            {
                int res = 0;
                if ((res = zkfp2.Init()) == zkfperrdef.ZKFP_ERR_OK)
                {
                    int dCount = zkfp2.GetDeviceCount();
                    dCount = dCount > 4 ? 4 : dCount;

                    //SetWindowsSize();

                    for (int i = 0; i < dCount; i++)
                    {
                        var index = i;
                        Task.Factory.StartNew(state =>
                        {
                            DoCapture(index);
                        }, $"Thread{index}");
                    }
            }
            });
        }

        private void DoCapture(int index)
        {
            ZkTeckoScanner scanner = new ZkTeckoScanner
            {
                DeviceIndex = index
            };

            if (!scanner.ConnectDevice())
            {
                Thread.CurrentThread.Interrupt();
            }
            else
            {
                MainViewModel.ZkScanners.Add(scanner);
            }

            scanner.FingerPrintCaptured += AjoibotFingerInvoke;
            scanner.DeviceDisconnected += () => { MainViewModel.ZkScanners.Remove(scanner); };

            scanner.CapturePrint();
        }

        private void AjoibotFingerInvoke(int index, string image, string print)
        {
            MainWebView.ExecuteScriptAsync($"AjoibotFinger('{print}', '{image}', '{index}')");
        }
    }
}
