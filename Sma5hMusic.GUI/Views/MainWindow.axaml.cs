using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Sma5hMusic.GUI.Interfaces;

namespace Sma5hMusic.GUI.Views
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
