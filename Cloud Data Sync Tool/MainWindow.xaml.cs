using System;
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
            DataContext = new MainWindowViewModel(cons);
        }
    }
}
