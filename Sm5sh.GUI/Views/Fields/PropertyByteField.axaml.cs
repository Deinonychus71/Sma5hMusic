using Avalonia;
using Avalonia.Markup.Xaml;

namespace Sm5sh.GUI.Views.Fields
{
    public class PropertyByteField : PropertyField
    {
        public static readonly StyledProperty<byte> ValueProperty = AvaloniaProperty.Register<PropertyByteField, byte>(nameof(Value), inherits: true, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsReadOnlyProperty = AvaloniaProperty.Register<PropertyByteField, bool>(nameof(IsReadOnly), inherits: true, defaultBindingMode: Avalonia.Data.BindingMode.OneWay);

        public byte Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public bool IsReadOnly
        {
            get { return GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public PropertyByteField()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
