using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Sm5sh.GUI.ViewModels;
using System;

namespace Sm5sh.GUI.Views
{
    public class BgmListView : UserControl
    {
        public BgmListView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
