using Avalonia.Data.Converters;
using Sma5hMusic.GUI.Helpers;
using System;
using System.Globalization;

namespace Sma5hMusic.GUI.Converters
{
    public class LocaleNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueStr = value as string;
            return Constants.GetLocaleDisplayName(valueStr);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
