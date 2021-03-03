using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace CloudSync.Behaviors
{
    public class TreeViewBehavior : Behavior<TreeView>
    {
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem",
            typeof(object), typeof(TreeViewBehavior), new FrameworkPropertyMetadata(OnSelectedItemChanged));

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        private void AssociatedObjectOnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItem = e.NewValue;
        }

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var item = e.NewValue as TreeViewItem;
            item?.SetValue(TreeViewItem.IsSelectedProperty, true);
        }

        protected override void OnAttached()
        {
            AssociatedObject.SelectedItemChanged += AssociatedObjectOnTreeViewSelectedItemChanged;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
                AssociatedObject.SelectedItemChanged -= AssociatedObjectOnTreeViewSelectedItemChanged;

            base.OnDetaching();
        }
    }
}
