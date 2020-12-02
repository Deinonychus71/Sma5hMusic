using Avalonia;
using Avalonia.Markup.Xaml;
using Sm5shMusic.GUI.ViewModels;

namespace Sm5shMusic.GUI.Views.Fields
{
    public class PropertyMusicField : PropertyTextField
    {
        public static readonly StyledProperty<MusicPlayerViewModel> MusicPlayerProperty = AvaloniaProperty.Register<PropertyMusicField, MusicPlayerViewModel>(nameof(MusicPlayer), inherits: true, defaultBindingMode: Avalonia.Data.BindingMode.OneWay);

        public MusicPlayerViewModel MusicPlayer
        {
            get
            {
                return GetValue(MusicPlayerProperty);
            }
            set
            {
                SetValue(MusicPlayerProperty, value);
            }
        }

        public PropertyMusicField()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
