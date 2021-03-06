using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using Sma5hMusic.GUI.ViewModels;
using Sma5hMusic.GUI.Views.Fields;
using System.Reactive.Disposables;

namespace Sma5hMusic.GUI.Views
{
    public class BgmPropertiesModalWindow : ReactiveWindow<BgmPropertiesModalWindowViewModel>
    {
        private PropertyField GameIdValidation => this.FindControl<PropertyField>("GameId");

        public BgmPropertiesModalWindow()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposables =>
            {
                this.BindValidation(ViewModel, vm => vm.SelectedGameTitleViewModel, view => view.GameIdValidation.ValidationError)
                .DisposeWith(disposables);
            });
        }
    }
}
