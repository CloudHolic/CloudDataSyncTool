using System.Text.RegularExpressions;
using System.Windows.Input;
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
        }

        private void PortTextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
