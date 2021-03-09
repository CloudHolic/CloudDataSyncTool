using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CloudSync.Controls
{
    public class MultiSelectionListBox : ListBox
    {
        public static readonly DependencyProperty BindableSelectedItemsProperty = DependencyProperty.Register("BindableSelectedItems",
            typeof(object), typeof(MultiSelectionListBox),
            new FrameworkPropertyMetadata(default(ICollection<object>), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBindableSelectedItemsChanged));

        public dynamic BindableSelectedItems
        {
            get => GetValue(BindableSelectedItemsProperty);
            set => SetValue(BindableSelectedItemsProperty, value);
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (BindableSelectedItems == null || !IsInitialized)
                return;

            if (e.AddedItems.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(SelectedValuePath))
                {
                    foreach(var item in e.AddedItems)
                        if (!BindableSelectedItems.Contains((dynamic) item.GetType().GetProperty(SelectedValuePath)?.GetValue(item, null)))
                            BindableSelectedItems.Add((dynamic) item.GetType().GetProperty(SelectedValuePath)?.GetValue(item, null));
                }
                else
                {
                    foreach(var item in e.AddedItems)
                        if (!BindableSelectedItems.Contains((dynamic) item))
                            BindableSelectedItems.Add((dynamic) item);
                }
            }

            if (e.RemovedItems.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(SelectedValuePath))
                {
                    foreach(var item in e.RemovedItems)
                        if (BindableSelectedItems.Contains((dynamic) item.GetType().GetProperty(SelectedValuePath)?.GetValue(item, null)))
                            BindableSelectedItems.Remove((dynamic) item.GetType().GetProperty(SelectedValuePath)?.GetValue(item, null));
                }
                else
                {
                    foreach(var item in e.RemovedItems)
                        if (BindableSelectedItems.Contains((dynamic) item))
                            BindableSelectedItems.Remove((dynamic) item);
                }
            }
        }

        private static void OnBindableSelectedItemsChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            if (s is MultiSelectionListBox listBox)
            {
                var newSelection = new List<dynamic>();

                if (!string.IsNullOrWhiteSpace(listBox.SelectedValuePath))
                {
                    foreach (var item in listBox.BindableSelectedItems)
                    {
                        foreach (var lbItem in listBox.Items)
                        {
                            var lbItemValue = lbItem.GetType().GetProperty(listBox.SelectedValuePath)?.GetValue(lbItem, null);
                            if ((dynamic) lbItemValue == item)
                                newSelection.Add(lbItem);
                        }
                    }
                }
                else
                    newSelection = listBox.BindableSelectedItems as List<dynamic>;

                listBox.SetSelectedItems(newSelection);
            }
        }
    }
}
