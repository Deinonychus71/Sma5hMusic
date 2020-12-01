using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia.Models;
using Microsoft.Extensions.Logging;
using Sm5sh.GUI.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sm5sh.GUI.Dialogs
{
    public class MessageDialog : IMessageDialog
    {
        private ILogger _logger;
        private IDialogWindow _rootDialogWindow;
        private const Style STYLE = Style.DarkMode;

        public MessageDialog(IDialogWindow rootDialogWindow, ILogger<MessageDialog> logger)
        {
            _logger = logger;
            _rootDialogWindow = rootDialogWindow;
        }

        public async Task<bool> ShowWarningConfirm(string title, string message)
        {
            var msBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.OkCancel,
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
                ContentTitle = title,
                ContentMessage = message,
                Icon = Icon.Warning,
                Style = STYLE
            });
            var result = await msBoxStandardWindow.ShowDialog(_rootDialogWindow.Window);
            return result == ButtonResult.Ok;
        }

        public async Task ShowError(string title, string message)
        {
            var msBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
                ContentTitle = title,
                ContentMessage = message,
                Icon = Icon.Error,
                Style = STYLE
            });
            await msBoxStandardWindow.ShowDialog(_rootDialogWindow.Window);
        }

        public async Task ShowInformation(string title, string message)
        {
            var msBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
                ContentTitle = title,
                ContentMessage = message,
                Icon = Icon.Info,
                Style = STYLE
            });
            await msBoxStandardWindow.ShowDialog(_rootDialogWindow.Window);
        }

        public async Task<string> PromptTest(string title, string message)
        {
            var msBoxStandardWindow = MessageBoxManager.GetMessageBoxInputWindow(new MessageBoxInputParams
            {
                ButtonDefinitions = new List<ButtonDefinition>() { new ButtonDefinition() { Name = "Cancel", Type = ButtonType.Default }, new ButtonDefinition() { Name = "OK", Type = ButtonType.Default } },
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
                ContentTitle = title,
                ContentMessage = message,
                Icon = Icon.Info,
                Style = Style.DarkMode
            });
            var result = await msBoxStandardWindow.ShowDialog(_rootDialogWindow.Window);
            return result.Button == "OK" ? result.Message : string.Empty;
        }
    }
}
