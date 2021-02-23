using System.Windows.Input;
using System.Text.RegularExpressions;
using MahApps.Metro.Controls;

namespace CloudSync.Contents
{
    /// <summary>
    /// OpenWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OpenWindow : MetroWindow
    {
        public OpenWindow()
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
