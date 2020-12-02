using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.IO;

namespace Sm5sh.GUI.Converters
{
    public class FilePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueStr = value as string;
            return Path.GetFileName(valueStr);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
