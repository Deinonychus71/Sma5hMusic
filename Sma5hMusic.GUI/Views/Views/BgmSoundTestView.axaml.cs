using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Sma5hMusic.GUI.Views
{
    public class BgmSoundTestView : UserControl
    {
        public BgmSoundTestView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
