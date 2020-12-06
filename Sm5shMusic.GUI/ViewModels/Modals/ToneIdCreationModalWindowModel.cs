using Avalonia.Controls;
using DynamicData;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using Sm5sh.Mods.Music.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;

namespace Sm5shMusic.GUI.ViewModels
{
    public class ToneIdCreationModalWindowModel : ReactiveValidationObject
    {
        private readonly ILogger _logger;
        private readonly ReadOnlyObservableCollection<BgmPropertyEntryViewModel> _bgmPropertyEntries;
        private const string REGEX_VALIDATION = @"^[a-z0-9_]+$";

        public ReactiveCommand<Window, Unit> ActionCancel { get; }
        public ReactiveCommand<Window, Unit> ActionCreate { get; }

        [Reactive]
        public string Filename { get; set; }

        [Reactive]
        public string ToneId { get; set; }

        public MusicModEntries NewMusicModEntries { get; private set; }

        public ToneIdCreationModalWindowModel(ILogger<ToneIdCreationModalWindowModel> logger, IObservable<IChangeSet<BgmPropertyEntryViewModel, string>> observableBgmPropertyEntries)
        {
            _logger = logger;

            //Bind observables
            observableBgmPropertyEntries
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _bgmPropertyEntries)
               .DisposeMany()
               .Subscribe();

            this.ValidationRule(p => p.ToneId,
                p => !string.IsNullOrEmpty(p) && Regex.IsMatch(p, REGEX_VALIDATION),
                $"The ToneId can only contain lowercase letters, digits and underscore.");

            this.ValidationRule(p => p.ToneId,
               p => !string.IsNullOrEmpty(p) && !_bgmPropertyEntries.Select(p2 => p2.NameId).Contains(p),
               $"The ToneId already exists in the database");

            var canExecute = this.WhenAnyValue(x => x.ValidationContext.IsValid);
            ActionCancel = ReactiveCommand.Create<Window>(Cancel);
            ActionCreate = ReactiveCommand.Create<Window>(Select, canExecute);
        }

        private void Cancel(Window w)
        {
            _logger.LogDebug("Clicked Cancel");
            w.Close();
        }

        private void Select(Window window)
        {
            _logger.LogDebug("Clicked OK");
            window.Close(window);
        }
    }
}
