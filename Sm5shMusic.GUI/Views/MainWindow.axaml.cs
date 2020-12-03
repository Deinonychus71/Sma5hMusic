using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Sm5shMusic.GUI.Interfaces;

namespace Sm5shMusic.GUI.Views
{
    public class MainWindow : Window, IDialogWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public Window Window => this;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
