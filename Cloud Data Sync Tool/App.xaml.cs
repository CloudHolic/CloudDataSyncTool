using System;
using System.Windows;
using CloudSync.Contents;
using CloudSync.Models;

namespace CloudSync
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var openWindow = new OpenWindow();

            openWindow.ShowDialog();
            if (openWindow.Tag is not Tuple<Connection, Connection> result)
                Current.Shutdown();
            else
            {
                var mainWindow = new MainWindow(result);
                Current.MainWindow = mainWindow;
                Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                mainWindow.Show();
            }
        }
    }
}
