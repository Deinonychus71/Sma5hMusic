using DynamicData;
using ReactiveUI;
using Sma5h.Mods.Music.Models;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using ReactiveUI.Validation.Extensions;

namespace Sma5hMusic.GUI.ViewModels
{
    public class GameDeletePickerModalWindowViewModel : ModalBaseViewModel<GameTitleEntryViewModel>
    {
        private readonly ReadOnlyObservableCollection<GameTitleEntryViewModel> _games;
        private readonly ReadOnlyObservableCollection<BgmDbRootEntryViewModel> _bgmEntries;
        public ReadOnlyObservableCollection<GameTitleEntryViewModel> Games { get { return _games; } }

        public GameDeletePickerModalWindowViewModel(IObservable<IChangeSet<GameTitleEntryViewModel, string>> observableGames,
            IObservable<IChangeSet<BgmDbRootEntryViewModel, string>> observableBgmEntries)
        {
            //Bind observables
            observableGames
               .Filter(p => p.Source == EntrySource.Mod)
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _games)
               .DisposeMany()
               .Subscribe();

            observableBgmEntries
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _bgmEntries)
               .DisposeMany()
               .Subscribe();

            this.ValidationRule(p => p.SelectedItem, x =>
                x != null &&
                !string.IsNullOrEmpty(x.UiSeriesId) &&
                x.Source == EntrySource.Mod &&
                _bgmEntries.FirstOrDefault(p => p.UiGameTitleId == x.UiGameTitleId) == null,
            $"Please select a custom game that is not associated to any song");
        }

        protected override IObservable<bool> GetValidationRule()
        {
            return this.WhenAnyValue(x => x.SelectedItem, x =>
                x != null &&
                !string.IsNullOrEmpty(x.UiSeriesId) &&
                x.Source == EntrySource.Mod &&
                _bgmEntries.FirstOrDefault(p => p.UiGameTitleId == x.UiGameTitleId) == null);
        }
    }
}
