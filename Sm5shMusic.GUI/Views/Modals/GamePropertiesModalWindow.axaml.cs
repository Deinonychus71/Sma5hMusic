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
    public class GamePropertiesModalWindow : ReactiveWindow<GamePropertiesModalWindowViewModel>
    {
        private PropertyTextField GameIdValidation => this.FindControl<PropertyTextField>("GameId");
        private PropertyField SeriesValidation => this.FindControl<PropertyField>("Series");
        private PropertyField TitleValidation => this.FindControl<PropertyField>("Title");

        public GamePropertiesModalWindow()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposables =>
            {
                this.BindValidation(ViewModel, vm => vm.SelectedSeries, view => view.SeriesValidation.ValidationError)
                .DisposeWith(disposables);
                this.BindValidation(ViewModel, vm => vm.UiGameTitleId, view => view.GameIdValidation.ValidationError)
                .DisposeWith(disposables);
                this.BindValidation(ViewModel, vm => vm.MSBTTitleEditor.CurrentLocalizedValue, view => view.TitleValidation.ValidationError)
                .DisposeWith(disposables);
            });
        }
    }
}
