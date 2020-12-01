﻿using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Sm5sh.GUI.Views
{
    public class ContextMenuView : UserControl
    {
        public ContextMenuView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
