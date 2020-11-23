using Avalonia.Controls;
using Avalonia.Data.Converters;
using Sm5sh.GUI.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sm5sh.GUI.Converters
{
    public class ParametizedDialogConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count == 2) {
                return new ParametizedDialogHelper()
                {
                    Parameter = values[0] as string,
                    Window = values[1] as Window
                };
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
