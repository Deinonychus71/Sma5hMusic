using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Sm5shMusic.GUI.Helpers;
using Sm5shMusic.GUI.Interfaces;
using Sm5shMusic.GUI.ViewModels;
using Splat;
using static Sm5shMusic.GUI.Helpers.StylesHelper;

namespace Sm5shMusic.GUI
{
    public class App : Application
    {
        public override void Initialize()
        {
            var uiScale = Program.Configuration.GetValue<UIScale>("Sm5shMusicGUI:UIScale");
            var uiTheme = Program.Configuration.GetValue<UITheme>("Sm5shMusicGUI:UITheme");

            Styles.Insert(0, StylesHelper.GetUITheme(uiTheme));
            AvaloniaXamlLoader.Load(this);
            Styles.Add(StylesHelper.GetUIScale(uiScale));
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
