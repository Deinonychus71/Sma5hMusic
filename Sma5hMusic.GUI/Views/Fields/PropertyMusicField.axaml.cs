using Avalonia;
using Avalonia.Markup.Xaml;
using Sma5hMusic.GUI.ViewModels;

namespace Sma5hMusic.GUI.Views.Fields
{
    public class PropertyMusicField : PropertyTextField
    {
        public static readonly StyledProperty<MusicButtonViewModel> MusicPlayerProperty = AvaloniaProperty.Register<PropertyMusicField, MusicButtonViewModel>(nameof(MusicPlayer), inherits: true, defaultBindingMode: Avalonia.Data.BindingMode.OneWay);

        public MusicButtonViewModel MusicPlayer
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
