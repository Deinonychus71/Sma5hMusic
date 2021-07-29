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
    public class SeriesDeletePickerModalWindow : ReactiveWindow<SeriesDeletePickerModalWindowViewModel>
    {
        private PropertyField SeriesDeletePickerValidation => this.FindControl<PropertyField>("SeriesDeletePicker");

        public SeriesDeletePickerModalWindow()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposables =>
            {
                this.BindValidation(ViewModel, vm => vm.SelectedItem, view => view.SeriesDeletePickerValidation.ValidationError)
                .DisposeWith(disposables);
            });
        }
    }
}
