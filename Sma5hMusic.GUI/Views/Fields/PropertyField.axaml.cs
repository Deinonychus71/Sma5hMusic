﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Sma5hMusic.GUI.Views.Fields
{
    public class PropertyField : UserControl
    {
        public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<PropertyField, string>(nameof(Label), inherits: true, defaultBindingMode: Avalonia.Data.BindingMode.OneWay);
        public static readonly StyledProperty<string> ToolTipProperty = AvaloniaProperty.Register<PropertyField, string>(nameof(ToolTip), inherits: true, defaultBindingMode: Avalonia.Data.BindingMode.OneWay);
        public static readonly StyledProperty<bool> IsRequiredProperty = AvaloniaProperty.Register<PropertyField, bool>(nameof(IsRequired), inherits: true, defaultBindingMode: Avalonia.Data.BindingMode.OneWay);
        public static readonly StyledProperty<object> FieldContentProperty = AvaloniaProperty.Register<PropertyField, object>(nameof(FieldContent));
        public static readonly StyledProperty<string> ValidationErrorProperty = AvaloniaProperty.Register<PropertyField, string>(nameof(ValidationError), inherits: true, defaultBindingMode: Avalonia.Data.BindingMode.OneWay);

        public string ValidationError
        {
            get { return GetValue(ValidationErrorProperty); }
            set { SetValue(ValidationErrorProperty, value); }
        }

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
