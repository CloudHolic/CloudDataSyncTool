using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Microsoft.Xaml.Behaviors;

namespace CloudSync.Behaviors
{
    public class SchemaButtonBehavior : Behavior<Button>
    {
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(SchemaButtonBehavior),
            new UIPropertyMetadata());

        public bool IsChecked
        {
            get => (bool) GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        private void AssociatedObjectOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var parent = AssociatedObject.TryFindParent<StackPanel>();
            if (parent != null)
            {
                if (parent.Children.Count < 1)
                    return;

                foreach (var child in parent.Children)
                {
                    var schemaButton = ((DependencyObject) child).FindChild<Button>();
                    if(schemaButton != null)
                        ((SchemaButtonBehavior)Interaction.GetBehaviors(schemaButton)[0]).IsChecked = false;
                }
            }

            IsChecked = true;
        }

        protected override void OnAttached()
        {
            AssociatedObject.PreviewMouseDown += AssociatedObjectOnMouseDown;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
                AssociatedObject.PreviewMouseDown -= AssociatedObjectOnMouseDown;

            base.OnDetaching();
        }
    }
}
