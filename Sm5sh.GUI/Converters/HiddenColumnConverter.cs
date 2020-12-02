using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Sm5sh.GUI.Converters
{
    public class HiddenColumnConverter : IValueConverter
    {
        private IBrush _grayBrush = Brushes.Gray;
        private IBrush _whiteBrush = Brushes.White;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueStr = (short)value;
            if (valueStr == -1)
            {
                return _grayBrush;
            }
            return _whiteBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
