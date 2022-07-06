using Microsoft.Web.WebView2.Core;
using System;

namespace AjoibotBio.Utils
{
    public static class WebView2Install
    {
        public static InstallInfo GetInfo()
        {
            var version = GetWebView2Version();

            return new InstallInfo(version);
        }

        private static string GetWebView2Version()
        {
            try
            {
                return CoreWebView2Environment.GetAvailableBrowserVersionString();
            }
            catch (Exception) { return ""; }
        }
    }

    public class InstallInfo
    {
        public InstallInfo(string version)
        {
            Version = version;

            if (Version.Contains("dev"))
                Type = InstallType.EdgeChromiumDev;
            else if (Version.Contains("beta"))
                Type = InstallType.EdgeChromiumBeta;
            else if (Version.Contains("canary"))
                Type = InstallType.EdgeChromiumCanary;
            else if (!string.IsNullOrEmpty(Version))
                Type = InstallType.WebView2;
            else
                Type = InstallType.NotInstalled;
        }

        public string Version { get; }

        public InstallType Type { get; set; }

    }
    public enum InstallType
    {
        WebView2, EdgeChromiumBeta, EdgeChromiumCanary, EdgeChromiumDev, NotInstalled
    }
}
