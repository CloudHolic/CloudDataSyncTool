using System;
using System.Globalization;
using System.Windows.Data;

namespace CloudSync.Converters
{
    public class MultiBooleanToIsEnabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var flag1 = values[0] is bool b1 && b1;
            var flag2 = values[1] is bool b2 && b2;

            return flag1 && !flag2;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
