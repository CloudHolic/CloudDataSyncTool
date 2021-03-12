using System;
using System.Globalization;
using System.Windows.Data;

namespace CloudSync.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class BooleanToButtonContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var flag = value is bool b && b;
            return flag ? "Cancel" : "Copy";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
