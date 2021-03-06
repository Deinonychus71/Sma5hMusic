using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Sma5hMusic.GUI.Helpers;
using Sma5hMusic.GUI.Interfaces;
using Sma5hMusic.GUI.ViewModels;
using Splat;
using static Sma5hMusic.GUI.Helpers.StylesHelper;

namespace Sma5hMusic.GUI
{
    public class App : Application
    {
        public override void Initialize()
        {
            var uiScale = Program.Configuration.GetValue<UIScale>("Sma5hMusicGUI:UIScale");
            var uiTheme = Program.Configuration.GetValue<UITheme>("Sma5hMusicGUI:UITheme");

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
