using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Sm5shMusic.GUI.Views
{
    public class PlaylistPickerModalWindow : Window
    {
        public PlaylistPickerModalWindow()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
