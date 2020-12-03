using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sm5shMusic.GUI.Converters
{
    public class MSBTValueConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            var valueDict = values[0] as Dictionary<string, string>;
            var locale = values[1] as string;
            return locale != null && valueDict.ContainsKey(locale) ? valueDict[locale] : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
