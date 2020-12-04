using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using Sm5shMusic.GUI.ViewModels;
using Sm5shMusic.GUI.Views.Fields;
using System.Reactive.Disposables;

namespace Sm5shMusic.GUI.Views
{
    public class ToneIdCreationModalWindow : ReactiveWindow<ToneIdCreationModalWindowModel>
    {
        private PropertyTextField ToneIdValidation => this.FindControl<PropertyTextField>("ToneId");

        public ToneIdCreationModalWindow()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposables =>
            {
                this.BindValidation(ViewModel, vm => vm.UiBgmId, view => view.ToneIdValidation.ValidationError)
                .DisposeWith(disposables);
            });
        }
    }
}
