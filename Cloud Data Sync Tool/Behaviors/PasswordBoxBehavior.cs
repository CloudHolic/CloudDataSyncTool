using Microsoft.Xaml.Behaviors;
using System.Security;
using System.Windows;
using System.Windows.Controls;

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
            if (box == null)
                return;
            if (((SecureString) e.NewValue).Length == 0)
                box.Password = string.Empty;
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
