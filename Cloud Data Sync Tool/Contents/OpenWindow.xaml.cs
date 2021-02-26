using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace CloudSync.Contents
{
    /// <summary>
    /// OpenWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OpenWindow : MetroWindow
    {
        private Binding _prevHostBinding, _prevPortBinding, _prevIdBinding, _prevDbBinding;

        public OpenWindow()
        {
            InitializeComponent();
        }

        private void PortTextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ConnectionCheckbox_OnChecked(object sender, RoutedEventArgs e)
        {
            _prevHostBinding = DstHost.GetBindingExpression(TextBox.TextProperty)?.ParentBinding;
            _prevPortBinding = DstPort.GetBindingExpression(TextBox.TextProperty)?.ParentBinding;
            _prevIdBinding = DstId.GetBindingExpression(TextBox.TextProperty)?.ParentBinding;
            _prevDbBinding = DstDb.GetBindingExpression(TextBox.TextProperty)?.ParentBinding;

            DstHost.SetBinding(TextBox.TextProperty, new Binding("Text") {Source = SrcHost});
            DstPort.SetBinding(TextBox.TextProperty, new Binding("Text") {Source = SrcPort});
            DstId.SetBinding(TextBox.TextProperty, new Binding("Text") {Source = SrcId});
            DstDb.SetBinding(TextBox.TextProperty, new Binding("Text") {Source = SrcDb});

            // TODO: 1. Save previous binding of DstPw to _prevPwBinding.
            // TODO: 2. Change binding BoundPasswordProperty of DstPw to that of SrcPw. (Interaction.GetBehaviors(DstPw)[0])
        }

        private void ConnectionCheckbox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if(_prevHostBinding != null)
                DstHost.SetBinding(TextBox.TextProperty, _prevHostBinding);

            if(_prevPortBinding != null)
                DstPort.SetBinding(TextBox.TextProperty, _prevPortBinding);

            if(_prevIdBinding != null)
                DstId.SetBinding(TextBox.TextProperty, _prevIdBinding);
            
            if(_prevDbBinding != null)
                DstDb.SetBinding(TextBox.TextProperty, _prevDbBinding);

            // TODO: Change binding BoundPasswordProperty of DstPw to _prevPwBinding.
        }
    }
}
