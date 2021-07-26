using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sma5h.Mods.Music;
using Sma5hMusic.GUI.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sma5hMusic.GUI.Dialogs
{
    public class MessageDialog : IMessageDialog
    {
        private readonly ILogger _logger;
        private readonly IDialogWindow _rootDialogWindow;
        private readonly Style _style = Style.DarkMode;

        public MessageDialog(IDialogWindow rootDialogWindow, IOptionsMonitor<ApplicationSettings> appSettings, ILogger<MessageDialog> logger)
        {
            _logger = logger;
            _rootDialogWindow = rootDialogWindow;
            _style = 
                appSettings.CurrentValue.Sma5hMusicGUI.UITheme == Helpers.StylesHelper.UITheme.WindowsDark ||
                appSettings.CurrentValue.Sma5hMusicGUI.UITheme == Helpers.StylesHelper.UITheme.Dark  ? 
                Style.DarkMode : Style.Windows;
        }

        public async Task<bool> ShowWarningConfirm(string title, string message)
        {
            _logger.LogWarning("Showing Warning Confirm: {Title} - {Message}", title, message);
            var msBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.OkCancel,
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
                ContentTitle = title,
                ContentMessage = message,
                Icon = Icon.Warning,
                Style = _style
            });
            var result = await msBoxStandardWindow.ShowDialog(_rootDialogWindow.Window);
            return result == ButtonResult.Ok;
        }

        public async Task ShowError(string title, string message, Exception e = null)
        {
            if (e != null)
                _logger.LogError(e, "Showing Error: {Title} - {Message}", title, message);
            else
                _logger.LogError("Showing Error: {Title} - {Message}", title, message);
            var msBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
                ContentTitle = title,
                ContentMessage = message,
                Icon = Icon.Error,
                Style = _style
            });
            await msBoxStandardWindow.ShowDialog(_rootDialogWindow.Window);
        }

        public async Task ShowInformation(string title, string message)
        {
            _logger.LogDebug("Showing Information: {Title} - {Message}", title, message);
            var msBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
                ContentTitle = title,
                ContentMessage = message,
                Icon = Icon.Info,
                Style = _style
            });
            await msBoxStandardWindow.ShowDialog(_rootDialogWindow.Window);
        }

        public async Task<string> PromptInput(string title, string message)
        {
            _logger.LogDebug("Showing PromptInput: {Title} - {Message}", title, message);
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
            return result.Message == "OK" ? result.Button : string.Empty;
        }
    }
}
