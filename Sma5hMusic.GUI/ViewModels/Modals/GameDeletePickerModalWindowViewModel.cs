using DynamicData;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using Sma5h.Mods.Music.Models;
using Sma5hMusic.GUI.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace Sma5hMusic.GUI.ViewModels
{
    public class GameDeletePickerModalWindowViewModel : ModalBaseViewModel<GameTitleEntryViewModel>
    {
        private readonly ReadOnlyObservableCollection<GameTitleEntryViewModel> _games;
        private readonly ReadOnlyObservableCollection<BgmDbRootEntryViewModel> _bgmEntries;
        public ReadOnlyObservableCollection<GameTitleEntryViewModel> Games { get { return _games; } }

        public GameDeletePickerModalWindowViewModel(IViewModelManager viewModelManager)
        {
            //Bind observables
            viewModelManager.ObservableGameTitles.Connect()
               .Filter(p => p.Source == EntrySource.Mod)
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _games)
               .DisposeMany()
               .Subscribe();

            viewModelManager.ObservableDbRootEntries.Connect()
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
