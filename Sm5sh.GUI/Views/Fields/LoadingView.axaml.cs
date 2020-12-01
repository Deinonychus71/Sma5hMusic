using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Sm5sh.GUI.Views.Fields
{
    public class LoadingView : UserControl
    {
        public LoadingView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
