using System;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

namespace Sm5shMusic.GUI.Helpers
{
    public class StylesHelper
    {
        private static readonly StyleInclude DataGridFluent = new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
        {
            Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml")
        };

        private static readonly StyleInclude DataGridDefault = new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
        {
            Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Default.xaml")
        };

        public static Styles FluentDark = new Styles
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
                Source = new Uri("avares://Sm5shMusic.GUI/Assets/Themes/CustomThemeDark.xaml")
            },
            DataGridFluent
        };

        public static Styles FluentLight = new Styles
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
                Source = new Uri("avares://Sm5shMusic.GUI/Assets/Themes/CustomThemeLight.xaml")
            },
            DataGridFluent
        };

        public static Styles DefaultLight = new Styles
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
            DataGridDefault
        };

        public static Styles DefaultDark = new Styles
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
            DataGridDefault
        };

        public static Styles DefaultUIScale = new Styles
        {
            new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Sm5shMusic.GUI/Assets/Themes/CustomThemeNormalUI.xaml")
            }
        };

        public static Styles SmallUIScale = new Styles
        {
            new StyleInclude(new Uri("resm:Styles?assembly=Sm5shMusic.GUI"))
            {
                Source = new Uri("avares://Sm5shMusic.GUI/Assets/Themes/CustomThemeSmallUI.xaml")
            }
        };
    }
}
