using Avalonia.Data.Converters;
using Sm5shMusic.GUI.Helpers;
using System;
using System.Globalization;

namespace Sm5shMusic.GUI.Converters
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
