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
                this.BindValidation(ViewModel, vm => vm.ToneId, view => view.ToneIdValidation.ValidationError)
                .DisposeWith(disposables);
            });
        }
    }
}
