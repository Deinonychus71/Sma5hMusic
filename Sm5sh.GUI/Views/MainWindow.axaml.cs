using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Sm5sh.GUI.Interfaces;

namespace Sm5sh.GUI.Views
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
