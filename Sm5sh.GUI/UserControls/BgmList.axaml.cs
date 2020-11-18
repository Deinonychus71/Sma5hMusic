using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Sm5sh.GUI.UserControls
{
    public class BgmList : UserControl
    {
        public BgmList()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
