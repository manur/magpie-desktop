using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Overlord
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public AppController AppController
        {
            get;
            private set;
        }

        private void ChangeLaunchAtStartupSetting(bool launchAtStartup)
        {
            try
            {
                AppController.SetLaunchAtStartup(launchAtStartup);
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format(StringResources.ErrorLaunchAtStartupSave, StringResources.ApplicationName, ex.Message));
            }
            this.Shutdown(0);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            this.AppController = new AppController();

            if (e.Args.Contains("enableLaunchAtStartup"))
            {
                ChangeLaunchAtStartupSetting(true);
            }
            else if (e.Args.Contains("disableLaunchAtStartup"))
            {
                ChangeLaunchAtStartupSetting(false);
            }

            AppController.OnAppStartup();
        }

        public static AppController Controller
        {
            get
            {
                App currentApp = (App)App.Current;
                return currentApp.AppController;
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            this.AppController.OnAppShutdown();
        }
    }
}
