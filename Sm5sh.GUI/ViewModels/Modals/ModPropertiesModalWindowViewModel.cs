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

namespace Sm5sh.GUI.ViewModels
{
    public class ModPropertiesModalWindowViewModel : ViewModelBase
    {
        private readonly ReadOnlyObservableCollection<ModEntryViewModel> _mods;
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

        [Reactive]
        public bool IsEdit { get; set; }

        public ReadOnlyObservableCollection<ModEntryViewModel> Series { get { return _mods; } }

        public ReactiveCommand<Window, Unit> ActionOK { get; }
        public ReactiveCommand<Window, Unit> ActionCancel { get; }

        public ModPropertiesModalWindowViewModel(ILogger<ModPropertiesModalWindowViewModel> logger, IObservable<IChangeSet<ModEntryViewModel, string>> observableMods)
        {
            _logger = logger;

            this.WhenAnyValue(p => p.ModName).Subscribe((o) => { FormatModPath(o); });

            //Bind observables
            observableMods
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _mods)
                .DisposeMany()
                .Subscribe();

            var canExecute = this.WhenAnyValue(x => x.ModName, x => x.ModPath, (n, p) =>
            !string.IsNullOrEmpty(n) && !string.IsNullOrEmpty(p));
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
                ModName = musicMod.Mod.Name;
                ModPath = musicMod.ModPath;
                ModWebsite = musicMod.Mod.Website;
                ModAuthor = musicMod.Mod.Author;
                ModDescription = musicMod.Mod.Description;
                IsEdit = true;
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
            window.Close(window);
        }

        public void SubmitDialogCancel(Window window)
        {
            window.Close();
        }
    }
}
