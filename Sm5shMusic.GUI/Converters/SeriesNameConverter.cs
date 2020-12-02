using Avalonia.Data.Converters;
using Sm5shMusic.GUI.Helpers;
using System;
using System.Globalization;

namespace Sm5shMusic.GUI.Converters
{
    public class SeriesNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueStr = value as string;
            return Constants.CONVERTER_SERIES.ContainsKey(valueStr) ? Constants.CONVERTER_SERIES[valueStr] : valueStr.Replace("ui_series_", string.Empty);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
