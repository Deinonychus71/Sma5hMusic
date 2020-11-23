using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Sm5sh.GUI.Views
{
    public class BgmPropertiesWindow : Window
    {
        public BgmPropertiesWindow()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
