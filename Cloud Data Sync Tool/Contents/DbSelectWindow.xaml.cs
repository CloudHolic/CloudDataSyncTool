using System.Windows;
using System.Windows.Controls;
using CloudSync.ViewModels;
using MahApps.Metro.Controls;

namespace CloudSync.Contents
{
    /// <summary>
    /// DbSelectWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DbSelectWindow : MetroWindow
    {
        public DbSelectWindow()
        {
            InitializeComponent();
            DataContext = new DbSelectWindowViewModel();
        }

        private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if(e.OriginalSource is TextBox textBox)
                textBox.SelectAll();
        }
    }
}
