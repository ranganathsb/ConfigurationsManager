using System;
using System.Windows;

namespace ConfigurationsManager
{
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Something went wrong");
            Application.Current.Shutdown();
        }
    }
}
