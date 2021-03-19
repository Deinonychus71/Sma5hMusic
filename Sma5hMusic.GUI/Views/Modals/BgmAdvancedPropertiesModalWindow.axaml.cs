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
    public class BgmAdvancedPropertiesModalWindow : ReactiveWindow<BgmPropertiesModalWindowViewModel>
    {
        private PropertyField GameIdValidation => this.FindControl<PropertyField>("GameId");
        private PropertyTextField StartPoint0Validation => this.FindControl<PropertyTextField>("StartPoint0");
        private PropertyTextField StartPoint1Validation => this.FindControl<PropertyTextField>("StartPoint1");
        private PropertyTextField StartPoint2Validation => this.FindControl<PropertyTextField>("StartPoint2");
        private PropertyTextField StartPoint3Validation => this.FindControl<PropertyTextField>("StartPoint3");
        private PropertyTextField StartPoint4Validation => this.FindControl<PropertyTextField>("StartPoint4");
        private PropertyTextField StartPointTransitionValidation => this.FindControl<PropertyTextField>("StartPointTransition");
        private PropertyTextField StartPointSuddenDeathValidation => this.FindControl<PropertyTextField>("StartPointSuddenDeath");
        private PropertyTextField EndPointValidation => this.FindControl<PropertyTextField>("EndPoint");

        public BgmAdvancedPropertiesModalWindow()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposables =>
            {
                this.BindValidation(ViewModel, vm => vm.SelectedGameTitleViewModel, view => view.GameIdValidation.ValidationError)
                .DisposeWith(disposables);
                this.BindValidation(ViewModel, vm => vm.StreamPropertyViewModel.StartPoint0, view => view.StartPoint0Validation.ValidationError)
                .DisposeWith(disposables);
                this.BindValidation(ViewModel, vm => vm.StreamPropertyViewModel.StartPoint1, view => view.StartPoint1Validation.ValidationError)
                .DisposeWith(disposables);
                this.BindValidation(ViewModel, vm => vm.StreamPropertyViewModel.StartPoint2, view => view.StartPoint2Validation.ValidationError)
                .DisposeWith(disposables);
                this.BindValidation(ViewModel, vm => vm.StreamPropertyViewModel.StartPoint3, view => view.StartPoint3Validation.ValidationError)
                .DisposeWith(disposables);
                this.BindValidation(ViewModel, vm => vm.StreamPropertyViewModel.StartPoint4, view => view.StartPoint4Validation.ValidationError)
                .DisposeWith(disposables);
                this.BindValidation(ViewModel, vm => vm.StreamPropertyViewModel.EndPoint, view => view.EndPointValidation.ValidationError)
                .DisposeWith(disposables);
                this.BindValidation(ViewModel, vm => vm.StreamPropertyViewModel.StartPointTransition, view => view.StartPointTransitionValidation.ValidationError)
                .DisposeWith(disposables);
                this.BindValidation(ViewModel, vm => vm.StreamPropertyViewModel.StartPointSuddenDeath, view => view.StartPointSuddenDeathValidation.ValidationError)
                .DisposeWith(disposables);
            });
        }
    }
}
