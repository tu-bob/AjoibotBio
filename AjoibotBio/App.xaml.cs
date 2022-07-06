using log4net;
using System.Windows;

namespace AjoibotBio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(App));

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ConfigureLogs();
        }

        private void ConfigureLogs()
        {
            log4net.Config.XmlConfigurator.Configure();
            log.Info("        =============  Started Logging  =============        ");
        }
    }
}
