using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;

namespace Sm5sh.GUI.Views.Fields
{
    public class PropertyField : UserControl
    {
        public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<PropertyField, string>(nameof(Label), inherits: true, defaultBindingMode: Avalonia.Data.BindingMode.OneWay);
        public static readonly StyledProperty<string> ToolTipProperty = AvaloniaProperty.Register<PropertyField, string>(nameof(ToolTip), inherits: true, defaultBindingMode: Avalonia.Data.BindingMode.OneWay);
        public static readonly StyledProperty<bool> IsReadOnlyProperty = AvaloniaProperty.Register<PropertyField, bool>(nameof(IsReadOnly), inherits: true, defaultBindingMode: Avalonia.Data.BindingMode.OneWay);
        public static readonly StyledProperty<bool> IsRequiredProperty = AvaloniaProperty.Register<PropertyField, bool>(nameof(IsRequired), inherits: true, defaultBindingMode: Avalonia.Data.BindingMode.OneWay);
        public static readonly StyledProperty<object> FieldContentProperty = AvaloniaProperty.Register<PropertyField, object>(nameof(FieldContent));

        public object FieldContent
        {
            get { return GetValue(FieldContentProperty); }
            set { SetValue(FieldContentProperty, value); }
        }

        public string ToolTip
        {
            get { return GetValue(ToolTipProperty); }
            set { SetValue(ToolTipProperty, value); }
        }

        public string Label
        {
            get { return GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public bool IsReadOnly
        {
            get { return GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public bool IsRequired
        {
            get { return GetValue(IsRequiredProperty); }
            set { SetValue(IsRequiredProperty, value); }
        }

        public PropertyField()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
