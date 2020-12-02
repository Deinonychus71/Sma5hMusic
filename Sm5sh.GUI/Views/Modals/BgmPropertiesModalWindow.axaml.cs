using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using Sm5sh.GUI.ViewModels;
using Sm5sh.GUI.Views.Fields;
using System.Reactive.Disposables;

namespace Sm5sh.GUI.Views
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
                this.BindValidation(ViewModel, vm => vm.SelectedBgmEntry.GameTitleViewModel, view => view.GameIdValidation.ValidationError)
                .DisposeWith(disposables);
            });
        }
    }
}
