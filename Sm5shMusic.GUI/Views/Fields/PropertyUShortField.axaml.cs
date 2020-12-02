using Avalonia;
using Avalonia.Markup.Xaml;

namespace Sm5sh.GUI.Views.Fields
{
    public class PropertyUShortField : PropertyField
    {
        public static readonly StyledProperty<ushort> ValueProperty = AvaloniaProperty.Register<PropertyUShortField, ushort>(nameof(Value), inherits: true, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsReadOnlyProperty = AvaloniaProperty.Register<PropertyUShortField, bool>(nameof(IsReadOnly), inherits: true, defaultBindingMode: Avalonia.Data.BindingMode.OneWay);

        public ushort Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public bool IsReadOnly
        {
            get { return GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public PropertyUShortField()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
