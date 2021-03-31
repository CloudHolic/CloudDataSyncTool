using Microsoft.Xaml.Behaviors;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using CloudSync.Utils;

namespace CloudSync.Behaviors
{
    public class PasswordBoxBehavior : Behavior<PasswordBox>
    {
        public static readonly DependencyProperty BoundPasswordProperty = DependencyProperty.Register("BoundPassword",
            typeof(SecureString), typeof(PasswordBoxBehavior), new FrameworkPropertyMetadata(OnBoundPasswordChanged));

        public SecureString BoundPassword
        {
            get => (SecureString) GetValue(BoundPasswordProperty);
            set => SetValue(BoundPasswordProperty, value);
        }

        private void AssociatedObjectOnPasswordChanged(object s, RoutedEventArgs e)
        {
            BoundPassword = AssociatedObject.SecurePassword;
        }

        public static void OnBoundPasswordChanged(object s, DependencyPropertyChangedEventArgs e)
        {
            var box = ((PasswordBoxBehavior) s).AssociatedObject;
            var securePassword = (SecureString) e.NewValue;
            if (box == null)
                return;
            if (securePassword == null || securePassword.Length == 0)
                box.Password = string.Empty;
            if (box.Password == SecureStringUtils.ConvertToString(securePassword))
                return;

            box.Password = SecureStringUtils.ConvertToString(securePassword);
        }

        protected override void OnAttached()
        {
            AssociatedObject.PasswordChanged += AssociatedObjectOnPasswordChanged;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
                AssociatedObject.PasswordChanged -= AssociatedObjectOnPasswordChanged;

            base.OnDetaching();
        }
    }
}
