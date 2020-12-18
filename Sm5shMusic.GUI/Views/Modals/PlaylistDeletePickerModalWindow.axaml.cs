using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Sm5shMusic.GUI.Views
{
    public class PlaylistDeletePickerModalWindow : Window
    {
        public PlaylistDeletePickerModalWindow()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
