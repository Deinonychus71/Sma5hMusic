using Avalonia.Data.Converters;
using Avalonia.Media;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using static Sma5hMusic.GUI.Helpers.StylesHelper;

namespace Sma5hMusic.GUI.Converters
{
    public class HiddenColumnConverter : IValueConverter
    {
        private readonly IBrush _grayBrush = Brushes.Gray;
        private readonly IBrush _defaultBrush = 
            Program.Configuration.GetValue<UITheme>("Sma5hMusicGUI:UITheme") == UITheme.Dark ||
            Program.Configuration.GetValue<UITheme>("Sma5hMusicGUI:UITheme") == UITheme.WindowsDark ? 
            Brushes.White : Brushes.Black;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueStr = (short)value;
            if (valueStr == -1)
            {
                return _grayBrush;
            }
            return _defaultBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
