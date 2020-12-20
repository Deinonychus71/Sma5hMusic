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
    public class GameDeletePickerModalWindow : ReactiveWindow<GameDeletePickerModalWindowViewModel>
    {
        private PropertyField GameDeletePickerValidation => this.FindControl<PropertyField>("GameDeletePicker");

        public GameDeletePickerModalWindow()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposables =>
            {
                this.BindValidation(ViewModel, vm => vm.SelectedItem, view => view.GameDeletePickerValidation.ValidationError)
                .DisposeWith(disposables);
            });
        }
    }
}
