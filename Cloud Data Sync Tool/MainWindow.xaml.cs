using System;
using System.Windows.Controls;
using System.Windows.Input;
using CloudSync.Models;
using CloudSync.ViewModels;
using MahApps.Metro.Controls;

namespace CloudSync
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow(Tuple<Connection, Connection> cons)
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(cons, SrcPanel);
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
