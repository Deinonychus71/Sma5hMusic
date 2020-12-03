using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Sm5shMusic.GUI.Helpers;
using System;

namespace Sm5shMusic.GUI.Views
{
    public class DebugConsole : UserControl
    {
        public DebugConsole()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            var txtConsole = this.FindControl<TextBox>("txtConsole");
            var scrollConsole = this.FindControl<ScrollViewer>("scrollConsole");
            var writer = new ControlWriter(txtConsole, scrollConsole);
            Console.SetOut(writer);
            Console.SetError(writer);
        }
    }
}
