using Avalonia;
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
    public class GlobalSettingsModalWindow : ReactiveWindow<GlobalSettingsModalWindowViewModel>
    {
        private PropertyFolderPickupField OutputPathValidation => this.FindControl<PropertyFolderPickupField>("OutputPath");
        private PropertyFolderPickupField GameResourcesPathValidation => this.FindControl<PropertyFolderPickupField>("GameResourcesPath");
        private PropertyFolderPickupField ResourcesPathValidation => this.FindControl<PropertyFolderPickupField>("ResourcesPath");
        private PropertyFolderPickupField ModPathValidation => this.FindControl<PropertyFolderPickupField>("ModPath");
        private PropertyFolderPickupField ModOverridePathValidation => this.FindControl<PropertyFolderPickupField>("ModOverridePath");
        private PropertyFolderPickupField ToolsPathValidation => this.FindControl<PropertyFolderPickupField>("ToolsPath");

        public GlobalSettingsModalWindow()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposables =>
            {
                this.BindValidation(ViewModel, vm => vm.SelectedItem.OutputPath, view => view.OutputPathValidation.ValidationError)
                .DisposeWith(disposables);
                this.BindValidation(ViewModel, vm => vm.SelectedItem.GameResourcesPath, view => view.GameResourcesPathValidation.ValidationError)
                .DisposeWith(disposables);
                this.BindValidation(ViewModel, vm => vm.SelectedItem.ResourcesPath, view => view.ResourcesPathValidation.ValidationError)
                .DisposeWith(disposables);
                this.BindValidation(ViewModel, vm => vm.SelectedItem.ModPath, view => view.ModPathValidation.ValidationError)
                .DisposeWith(disposables);
                this.BindValidation(ViewModel, vm => vm.SelectedItem.ModOverridePath, view => view.ModOverridePathValidation.ValidationError)
                .DisposeWith(disposables);
                this.BindValidation(ViewModel, vm => vm.SelectedItem.ToolsPath, view => view.ToolsPathValidation.ValidationError)
                .DisposeWith(disposables);
            });
        }
    }
}
