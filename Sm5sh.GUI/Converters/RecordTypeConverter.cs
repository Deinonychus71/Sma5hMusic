using Avalonia.Data.Converters;
using Sm5sh.GUI.Helpers;
using System;
using System.Globalization;

namespace Sm5sh.GUI.Converters
{
    public class RecordTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueStr = value as string;
            return Constants.CONVERTER_RECORD_TYPE.ContainsKey(valueStr) ? Constants.CONVERTER_RECORD_TYPE[valueStr] : valueStr.Replace("record_", string.Empty);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
