using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Sma5hMusic.GUI.Views
{
    public class SeriesPickerModalWindow : Window
    {
        public SeriesPickerModalWindow()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
