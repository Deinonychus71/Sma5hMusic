using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Sma5hMusic.GUI.Helpers
{
    public class StylesHelper
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum UIScale
        {
            Normal = 0,
            Small = 1
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum UITheme
        {
            WindowsDark = 0,
            WindowsLight = 1,
            Dark = 2,
            Light = 3
        }

        private static readonly Styles FluentDark = new Styles
        {
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/FluentDark.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Sma5hMusic.GUI/Assets/Themes/CustomFluentTheme.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Sma5hMusic.GUI/Assets/Themes/CustomThemeDark.xaml")
            }
        };

        private static readonly Styles FluentLight = new Styles
        {
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/FluentLight.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Sma5hMusic.GUI/Assets/Themes/CustomFluentTheme.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Sma5hMusic.GUI/Assets/Themes/CustomThemeLight.xaml")
            }
        };

        private static readonly Styles DefaultLight = new Styles
        {
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/Accents/Base.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/Accents/BaseLight.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Default/Accents/BaseLight.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Default/DefaultTheme.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Default.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Sma5hMusic.GUI/Assets/Themes/CustomWindowsTheme.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Sma5hMusic.GUI/Assets/Themes/CustomThemeWindowsLight.xaml")
            }
        };

        private static readonly Styles DefaultDark = new Styles
        {
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/Accents/Base.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/Accents/BaseDark.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Default/Accents/BaseDark.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Default/DefaultTheme.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Default.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Sma5hMusic.GUI/Assets/Themes/CustomWindowsTheme.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Sma5hMusic.GUI/Assets/Themes/CustomThemeWindowsDark.xaml")
            }
        };

        private static readonly Styles DefaultUIScale = new Styles
        {
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Sma5hMusic.GUI/Assets/Themes/CustomThemeNormalUI.xaml")
            }
        };

        private static readonly Styles SmallUIScale = new Styles
        {
            new StyleInclude(new Uri("resm:Styles?assembly=Sma5hMusic.GUI"))
            {
                Source = new Uri("avares://Sma5hMusic.GUI/Assets/Themes/CustomThemeSmallUI.xaml")
            }
        };

        public static Styles GetUITheme(UITheme themeStyle)
        {
            return themeStyle switch
            {
                UITheme.WindowsDark => DefaultDark,
                UITheme.WindowsLight => DefaultLight,
                UITheme.Dark => FluentDark,
                UITheme.Light => FluentLight,
                _ => throw new NotImplementedException("Theme not implemented"),
            };
        }

        public static Styles GetUIScale(UIScale scaleStyle)
        {
            return scaleStyle switch
            {
                UIScale.Normal => DefaultUIScale,
                UIScale.Small => SmallUIScale,
                _ => throw new NotImplementedException("Theme not implemented"),
            };
        }
    }
}
