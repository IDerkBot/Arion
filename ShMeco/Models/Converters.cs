using System;
using System.Globalization;
using System.Windows.Data;

namespace ArionCameraXrayDefender.Models
{
    // true превращает в false для Binding
    public class BoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool val = (bool)value;
            return !val;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}