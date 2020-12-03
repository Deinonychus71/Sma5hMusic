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
    public class PlaylistPropertiesModalWindow : ReactiveWindow<PlaylistPropertiesModalWindowViewModel>
    {
        private PropertyTextField PlaylistIdValidation => this.FindControl<PropertyTextField>("PlaylistId");
        private PropertyTextField PlaylistTitleValidation => this.FindControl<PropertyTextField>("PlaylistTitle");

        public PlaylistPropertiesModalWindow()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposables =>
            {
                this.BindValidation(ViewModel, vm => vm.PlaylistId, view => view.PlaylistIdValidation.ValidationError)
                .DisposeWith(disposables);
                this.BindValidation(ViewModel, vm => vm.PlaylistTitle, view => view.PlaylistTitleValidation.ValidationError)
                .DisposeWith(disposables);
            });
        }
    }
}
