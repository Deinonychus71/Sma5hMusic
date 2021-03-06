using Avalonia.Data.Converters;
using Sma5hMusic.GUI.Helpers;
using System;
using System.Globalization;

namespace Sma5hMusic.GUI.Converters
{
    public class RecordTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueStr = value as string;
            if (string.IsNullOrEmpty(valueStr))
                return value;
            return Constants.CONVERTER_RECORD_TYPE.ContainsKey(valueStr) ? Constants.CONVERTER_RECORD_TYPE[valueStr] : valueStr.Replace("record_", string.Empty);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
