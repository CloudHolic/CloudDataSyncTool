using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace CloudSync.Converters
{
    public class MultiParameterToVisualBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var tables = values[0] as ObservableCollection<string>;
            var isOpened = values[1] is bool b && b;

            if (tables == null || tables.Count < 1)
                return new VisualBrush {Stretch = Stretch.Uniform};
            return new VisualBrush(new PackIconBootstrapIcons
            {
                Kind = isOpened ? PackIconBootstrapIconsKind.CaretDownFill : PackIconBootstrapIconsKind.CaretRightFill
            }) {Stretch = Stretch.Uniform};
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
