using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Sm5shMusic.GUI.Helpers;
using Sm5shMusic.GUI.Interfaces;
using Sm5shMusic.GUI.ViewModels;
using Splat;

namespace Sm5shMusic.GUI
{
    public class App : Application
    {
        public override void Initialize()
        {
            Styles.Insert(0, StylesHelper.FluentDark);
            AvaloniaXamlLoader.Load(this);
            Styles.Add(StylesHelper.DefaultUIScale);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = Locator.Current.GetService<IDialogWindow>() as Window;
                mainWindow.DataContext = Locator.Current.GetService<MainWindowViewModel>();
                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
