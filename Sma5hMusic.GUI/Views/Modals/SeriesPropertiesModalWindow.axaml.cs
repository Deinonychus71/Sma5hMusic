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
    public class SeriesPropertiesModalWindow : ReactiveWindow<SeriesPropertiesModalWindowViewModel>
    {
        private PropertyTextField SeriesIdValidation => this.FindControl<PropertyTextField>("SeriesId");
        private PropertyField TitleValidation => this.FindControl<PropertyField>("Title");

        public SeriesPropertiesModalWindow()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposables =>
            {
                this.BindValidation(ViewModel, vm => vm.UiSeriesId, view => view.SeriesIdValidation.ValidationError)
                .DisposeWith(disposables);
                this.BindValidation(ViewModel, vm => vm.MSBTTitleEditor.CurrentLocalizedValue, view => view.TitleValidation.ValidationError)
                .DisposeWith(disposables);
            });
        }
    }
}
