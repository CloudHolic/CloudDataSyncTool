using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using CloudSync.ViewModels;
using MahApps.Metro.Controls;

namespace CloudSync.Contents
{
    /// <summary>
    /// OpenWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OpenWindow : MetroWindow
    {
        private Binding _prevHostBinding, _prevPortBinding, _prevIdBinding;//, _prevPwBinding;

        public OpenWindow()
        {
            InitializeComponent();
            DataContext = new OpenWindowViewModel();
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
            //_prevPwBinding = BindingOperations.GetBinding(Interaction.GetBehaviors(DstPw)[0], PasswordBoxBehavior.BoundPasswordProperty);

            DstHost.SetBinding(TextBox.TextProperty, new Binding("Text") {Source = SrcHost});
            DstPort.SetBinding(TextBox.TextProperty, new Binding("Text") {Source = SrcPort});
            DstId.SetBinding(TextBox.TextProperty, new Binding("Text") {Source = SrcId});
            //BindingOperations.SetBinding(Interaction.GetBehaviors(DstPw)[0], PasswordBoxBehavior.BoundPasswordProperty,
            //    new Binding("BoundPassword") {Source = ((PasswordBoxBehavior) Interaction.GetBehaviors(SrcPw)[0]).BoundPassword});
        }

        private void ConnectionCheckbox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if(_prevHostBinding != null)
                DstHost.SetBinding(TextBox.TextProperty, _prevHostBinding);

            if(_prevPortBinding != null)
                DstPort.SetBinding(TextBox.TextProperty, _prevPortBinding);

            if(_prevIdBinding != null)
                DstId.SetBinding(TextBox.TextProperty, _prevIdBinding);

            //if (_prevPwBinding != null)
            //    BindingOperations.SetBinding(Interaction.GetBehaviors(DstPw)[0], PasswordBoxBehavior.BoundPasswordProperty, _prevPwBinding);
        }
    }
}
