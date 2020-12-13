using Avalonia;
using Avalonia.Markup.Xaml;
using System.Windows.Input;

namespace Sm5shMusic.GUI.Views.Fields
{
    public class PropertyFolderPickupField : PropertyTextField
    {
        private ICommand _command;
        private readonly bool _commandCanExecute = true;

        public static readonly StyledProperty<string> ButtonParameterProperty = AvaloniaProperty.Register<PropertyFolderPickupField, string>(nameof(CommandParameter), inherits: true, defaultBindingMode: Avalonia.Data.BindingMode.OneWay);

        public static readonly DirectProperty<PropertyFolderPickupField, ICommand> CommandProperty = AvaloniaProperty.RegisterDirect<PropertyFolderPickupField, ICommand>(nameof(Command), button => button.Command, (button, command) => button.Command = command, enableDataValidation: true);

        protected override bool IsEnabledCore => base.IsEnabledCore && _commandCanExecute;

        public string CommandParameter
        {
            get { return GetValue(ButtonParameterProperty); }
            set { SetValue(ButtonParameterProperty, value); }
        }

        public ICommand Command
        {
            get { return _command; }
            set { SetAndRaise(CommandProperty, ref _command, value); }
        }

        public PropertyFolderPickupField()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
