using System;
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
            if (!OperatingSystem.IsWindowsVersionAtLeast(7))
                return;

            BoundPassword = AssociatedObject.SecurePassword;
        }

        public static void OnBoundPasswordChanged(object s, DependencyPropertyChangedEventArgs e)
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(7))
                return;
            
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
            if (!OperatingSystem.IsWindowsVersionAtLeast(7))
                return;

            AssociatedObject.PasswordChanged += AssociatedObjectOnPasswordChanged;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(7))
                return;

            if (AssociatedObject != null)
                AssociatedObject.PasswordChanged -= AssociatedObjectOnPasswordChanged;

            base.OnDetaching();
        }
    }
}
