using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ZKFingerprint;

namespace AjoibotBio.MainWindow
{
    public partial class MainWindow : Window
    {

        public void InitFingerprintScanner(object sender, EventArgs e)
        {
            Log.Info("Initialize fingerprint scanner library");

            MainViewModel.ZkScanners = new List<ZkTeckoScanner>();

            Task.Run(() =>
            {
                int res = 0;
                if ((res = ZkFingerprintLib.Init()) == ZkFingerprintLib.ZKFP_ERR_OK)
                {
                    int dCount = ZkFingerprintLib.GetDeviceCount();
                    dCount = dCount > 4 ? 4 : dCount;

                    Log.Info($"Detected {dCount} fingerprint scanners");

                    for (int i = 0; i < dCount; i++)
                    {
                        var index = i;
                        Task.Factory.StartNew(state =>
                        {
                            Log.Info($"Start capture for  fingerprint scanner with index of {index}");
                            DoCapture(index);
                        }, $"Thread{index}");
                    }
            }
                else
                {
                    Log.Error("Failed to initiazlie zkfp2 library");
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
                Log.Error($"Failed to connect to fingerpint scanner with index of {index}");
                Thread.CurrentThread.Interrupt();
            }
            else
            {
                Log.Info($"Connected to fingerpint scanner with index of {index}");
                MainViewModel.ZkScanners.Add(scanner);
            }

            scanner.FingerPrintCaptured += AjoibotFingerInvoke;
            scanner.DeviceDisconnected += () => {
                Log.Info($"Fingerpint scanner with index of {index} was disconnected");
                MainViewModel.ZkScanners.Remove(scanner);
            };

            scanner.StartCapture();
        }

        private void AjoibotFingerInvoke(int index, string image, string print)
        {
            Log.Info($"Fingerprint was captured by scanner with index of {index}. Executing js function AjoibtoFinger");
            MainWebView.ExecuteScriptAsync($"AjoibotFinger('{print}', '{image}', '{index}')");
        }
    }
}
