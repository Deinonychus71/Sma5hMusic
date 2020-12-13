using System;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Sm5shMusic.GUI.Helpers
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
            Dark = 0,
            Light = 1
        }

        private static readonly Styles FluentDark = new Styles
        {
            /*new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/FluentDark.xaml")
            },*/
            new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/Accents/FluentDark.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/FluentTheme.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Sm5shMusic.GUI/Assets/Themes/CustomThemeDark.xaml")
            }
        };

        private static readonly Styles FluentLight = new Styles
        {
            /*new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/FluentLight.xaml")
            },*/
            new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/Accents/FluentLight.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/FluentTheme.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Sm5shMusic.GUI/Assets/Themes/CustomThemeLight.xaml")
            }
        };

        private static readonly Styles DefaultLight = new Styles
        {
            new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/Accents/Base.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/Accents/BaseLight.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Default/Accents/BaseLight.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Default/DefaultTheme.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Default.xaml")
            }
        };

        private static readonly Styles DefaultDark = new Styles
        {
            new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/Accents/Base.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/Accents/BaseDark.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Default/Accents/BaseDark.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Themes.Default/DefaultTheme.xaml")
            },
            new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Default.xaml")
            }
        };

        private static readonly Styles DefaultUIScale = new Styles
        {
            new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Sm5shMusic.GUI/Assets/Themes/CustomThemeNormalUI.xaml")
            }
        };

        private static readonly Styles SmallUIScale = new Styles
        {
            new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Sm5shMusic.GUI/Assets/Themes/CustomThemeSmallUI.xaml")
            }
        };

        public static Styles GetUITheme(UITheme themeStyle)
        {
            return themeStyle switch
            {
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
