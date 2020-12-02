using Avalonia;
using Avalonia.Markup.Xaml;

namespace Sm5shMusic.GUI.Views.Fields
{
    public class PropertyIntField : PropertyField
    {
        public static readonly StyledProperty<int> ValueProperty = AvaloniaProperty.Register<PropertyIntField, int>(nameof(Value), inherits: true, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsReadOnlyProperty = AvaloniaProperty.Register<PropertyIntField, bool>(nameof(IsReadOnly), inherits: true, defaultBindingMode: Avalonia.Data.BindingMode.OneWay);

        public int Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public bool IsReadOnly
        {
            get { return GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public PropertyIntField()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
