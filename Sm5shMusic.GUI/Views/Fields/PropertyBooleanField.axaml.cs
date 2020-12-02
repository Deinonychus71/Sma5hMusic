using Avalonia;
using Avalonia.Markup.Xaml;

namespace Sm5shMusic.GUI.Views.Fields
{
    public class PropertyBooleanField : PropertyField
    {
        public static readonly StyledProperty<bool> IsCheckedProperty = AvaloniaProperty.Register<PropertyBooleanField, bool>(nameof(IsChecked), inherits: true, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsReadOnlyProperty = AvaloniaProperty.Register<PropertyShortField, bool>(nameof(IsReadOnly), inherits: true, defaultBindingMode: Avalonia.Data.BindingMode.OneWay);

        public bool IsChecked
        {
            get { return GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public bool IsReadOnly
        {
            get { return GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public PropertyBooleanField()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
