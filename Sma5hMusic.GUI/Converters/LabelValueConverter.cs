using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Sma5hMusic.GUI.Converters
{
    public class LabelValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueStr = value as string;
            return valueStr.Replace("{{", string.Empty).Replace("}}", string.Empty);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
