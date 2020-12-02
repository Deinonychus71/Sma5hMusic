using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Sm5shMusic.GUI.Views
{
    public class BgmSongsView : UserControl
    {
        public BgmSongsView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
