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
    public class ModPropertiesModalWindow : ReactiveWindow<ModPropertiesModalWindowViewModel>
    {
        private PropertyTextField ModPathValidation => this.FindControl<PropertyTextField>("ModPath");
        private PropertyTextField ModTitleValidation => this.FindControl<PropertyTextField>("ModTitle");

        public ModPropertiesModalWindow()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposables =>
            {
                this.BindValidation(ViewModel, vm => vm.ModPath, view => view.ModPathValidation.ValidationError)
                .DisposeWith(disposables);
                this.BindValidation(ViewModel, vm => vm.ModName, view => view.ModTitleValidation.ValidationError)
                .DisposeWith(disposables);
            });
        }
    }
}
