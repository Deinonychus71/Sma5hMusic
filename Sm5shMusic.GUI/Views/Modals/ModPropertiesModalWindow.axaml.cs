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
