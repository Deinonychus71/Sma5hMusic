using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Microsoft.Extensions.DependencyInjection;
using System;
using Sm5sh.Mods.Music.Interfaces;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using System.Reactive;

namespace Sm5sh.GUI.ViewModels
{
    public class ModPropertiesModalWindowViewModel : ViewModelBase
    {
        private const string REGEX_REPLACE = @"[^a-zA-Z0-9\-_ ]";
        private readonly ILogger _logger;

        public IMusicMod ModManager { get; }

        [Reactive]
        public string ModName { get; set; }
        [Reactive]
        public string ModPath { get; set; }
        [Reactive]
        public string ModWebsite { get; set; }
        [Reactive]
        public string ModAuthor { get; set; }
        [Reactive]
        public string ModDescription { get; set; }

        public ReactiveCommand<Window, Unit> ActionOK { get; }
        public ReactiveCommand<Window, Unit> ActionCancel { get; }

        public ModPropertiesModalWindowViewModel(ILogger<ModPropertiesModalWindowViewModel> logger)
        {
            _logger = logger;

            this.WhenAnyValue(p => p.ModName).Subscribe((o) => { FormatModPath(o); });

            var canExecute = this.WhenAnyValue(x => x.ModName, x => x.ModPath, (n, p) =>
            !string.IsNullOrEmpty(n) && !string.IsNullOrEmpty(p));
            ActionOK = ReactiveCommand.Create<Window>(SubmitDialogOK, canExecute);
            ActionCancel = ReactiveCommand.Create<Window>(SubmitDialogCancel);
        }

        private void FormatModPath(string modName)
        {
            if (string.IsNullOrEmpty(modName))
                ModPath = string.Empty;
            else
                ModPath = Regex.Replace(modName, REGEX_REPLACE, string.Empty);
        }

        public void SubmitDialogOK(Window window)
        {
            window.Close(window);
        }

        public void SubmitDialogCancel(Window window)
        {
            window.Close();
        }
    }
}
