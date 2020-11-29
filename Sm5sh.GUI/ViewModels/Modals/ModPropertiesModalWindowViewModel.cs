using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using System;
using Sm5sh.Mods.Music.Interfaces;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using System.Reactive;
using DynamicData;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using ReactiveUI.Validation.Helpers;
using ReactiveUI.Validation.Extensions;
using System.IO;
using Microsoft.Extensions.Options;
using Sm5sh.Mods.Music;

namespace Sm5sh.GUI.ViewModels
{
    public class ModPropertiesModalWindowViewModel : ReactiveValidationObject
    {
        private readonly IOptions<Sm5shMusicOptions> _config;
        private readonly ReadOnlyObservableCollection<ModEntryViewModel> _mods;
        private const string REGEX_REPLACE = @"[^a-zA-Z0-9\-_ ]";
        private string REGEX_VALIDATION = @"^[\w\-. ]+$";
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

        [Reactive]
        public bool IsEdit { get; set; }

        public ReadOnlyObservableCollection<ModEntryViewModel> Series { get { return _mods; } }

        public ReactiveCommand<Window, Unit> ActionOK { get; }
        public ReactiveCommand<Window, Unit> ActionCancel { get; }

        public ModPropertiesModalWindowViewModel(ILogger<ModPropertiesModalWindowViewModel> logger, IOptions<Sm5shMusicOptions> config, IObservable<IChangeSet<ModEntryViewModel, string>> observableMods)
        {
            _config = config;
            _logger = logger;

            this.WhenAnyValue(p => p.ModName).Subscribe((o) => { FormatModPath(o); });

            //Bind observables
            observableMods
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _mods)
                .DisposeMany()
                .Subscribe();

            this.ValidationRule(p => p.ModPath,
                p => !string.IsNullOrEmpty(p) && ((Regex.IsMatch(p, REGEX_VALIDATION) && !Directory.Exists(Path.Combine(_config.Value.Sm5shMusic.ModPath, p))) || IsEdit),
                $"The folder name is invalid or the folder already exists.");

            //Validation
            this.ValidationRule(p => p.ModName,
                p => !string.IsNullOrEmpty(p),
                $"Please enter a Title.");

            var canExecute = this.WhenAnyValue(x => x.ValidationContext.IsValid);
            ActionOK = ReactiveCommand.Create<Window>(SubmitDialogOK, canExecute);
            ActionCancel = ReactiveCommand.Create<Window>(SubmitDialogCancel);
        }

        public void LoadMusicMod(IMusicMod musicMod)
        {
            if(musicMod == null)
            {
                ModName = string.Empty;
                ModPath = string.Empty;
                ModWebsite = string.Empty;
                ModAuthor = string.Empty;
                ModDescription = string.Empty;
                IsEdit = false;
            }
            else
            {
                IsEdit = true;
                ModName = musicMod.Mod.Name;
                ModPath = musicMod.ModPath;
                ModWebsite = musicMod.Mod.Website;
                ModAuthor = musicMod.Mod.Author;
                ModDescription = musicMod.Mod.Description;
            }
        }

        private void FormatModPath(string modName)
        {
            if (!IsEdit)
            {
                if (string.IsNullOrEmpty(modName))
                    ModPath = string.Empty;
                else
                    ModPath = Regex.Replace(modName, REGEX_REPLACE, string.Empty).ToLower();
            }
        }

        public void SubmitDialogOK(Window window)
        {
            if (IsEdit)
            {
                ModManager.Mod.Author = this.ModAuthor;
                ModManager.Mod.Description = this.ModDescription;
                ModManager.Mod.Name = this.ModName;
            }

            window.Close(window);
        }

        public void SubmitDialogCancel(Window window)
        {
            window.Close();
        }
    }
}
