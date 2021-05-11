using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace CloudSync.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class NotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value != null, nameof(value) + " != null");
            return !(value as bool?) ?? !bool.Parse(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value != null, nameof(value) + " != null");
            return !(value as bool?) ?? !bool.Parse(value.ToString());
        }
    }
}