using Avalonia.Controls;
using DynamicData;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;

namespace Sm5shMusic.GUI.ViewModels
{
    public class ToneIdCreationModalWindowModel : ReactiveValidationObject
    {
        private readonly ILogger _logger;
        private readonly ReadOnlyObservableCollection<BgmEntryViewModel> _bgmEntries;
        private const string REGEX_VALIDATION = @"^[a-z0-9_]+$";

        public ReadOnlyObservableCollection<BgmEntryViewModel> Mods { get { return _bgmEntries; } }

        public ReactiveCommand<Window, Unit> ActionCancel { get; }
        public ReactiveCommand<Window, Unit> ActionCreate { get; }

        [Reactive]
        public string Filename { get; set; }

        [Reactive]
        public string ToneId { get; set; }


        public ToneIdCreationModalWindowModel(ILogger<ToneIdCreationModalWindowModel> logger, IObservable<IChangeSet<BgmEntryViewModel, string>> observableBgmEntries)
        {
            _logger = logger;

            //Bind observables
            observableBgmEntries
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _bgmEntries)
               .DisposeMany()
               .Subscribe();

            this.ValidationRule(p => p.ToneId,
                p => !string.IsNullOrEmpty(p) && Regex.IsMatch(p, REGEX_VALIDATION),
                $"The ToneId can only contain lowercase letters, digits and underscore.");

            this.ValidationRule(p => p.ToneId,
               p => !string.IsNullOrEmpty(p) && !_bgmEntries.Select(p2 => p2.ToneId).Contains(p),
               $"The ToneId already exists in the database");

            var canExecute = this.WhenAnyValue(x => x.ValidationContext.IsValid);
            ActionCancel = ReactiveCommand.Create<Window>(Cancel);
            ActionCreate = ReactiveCommand.Create<Window>(Select, canExecute);
        }

        private void Cancel(Window w)
        {
            w.Close();
        }

        private void Select(Window window)
        {
            window.Close(window);
        }
    }
}
