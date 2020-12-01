using Avalonia;
using Avalonia.Markup.Xaml;

namespace Sm5sh.GUI.Views.Fields
{
    public class PropertyUIntField : PropertyField
    {
        public static readonly StyledProperty<uint> ValueProperty = AvaloniaProperty.Register<PropertyUIntField, uint>(nameof(Value), inherits: true, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsReadOnlyProperty = AvaloniaProperty.Register<PropertyUIntField, bool>(nameof(IsReadOnly), inherits: true, defaultBindingMode: Avalonia.Data.BindingMode.OneWay);

        public uint Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public bool IsReadOnly
        {
            get { return GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public PropertyUIntField()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
