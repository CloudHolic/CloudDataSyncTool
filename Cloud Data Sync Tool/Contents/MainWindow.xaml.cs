using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CloudSync.Models;
using CloudSync.Utils;
using CloudSync.ViewModels;
using MahApps.Metro.Controls;

namespace CloudSync.Contents
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow(Tuple<Connection, Connection> cons)
        {
            InitializeComponent();
            
            WindowStartupLocation = ConfigManager.Instance.Config.WindowLeft == 0 && ConfigManager.Instance.Config.WindowTop == 0
                ? WindowStartupLocation.CenterScreen : WindowStartupLocation.Manual;
            
            Width = ConfigManager.Instance.Config.WindowWidth;
            Height = ConfigManager.Instance.Config.WindowHeight;
            Left = ConfigManager.Instance.Config.WindowLeft;
            Top = ConfigManager.Instance.Config.WindowTop;

            DataContext = new MainWindowViewModel(cons, new Tuple<StackPanel, StackPanel>(SrcPanel, DstPanel));
        }

        private void ScrollPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if(e.Delta > 0)
                scrollViewer?.LineUp();
            else
                scrollViewer?.LineDown();
            e.Handled = true;
        }
    }
}
