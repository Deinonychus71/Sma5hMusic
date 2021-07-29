using DynamicData;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using Sma5h.Mods.Music.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace Sma5hMusic.GUI.ViewModels
{
    public class SeriesDeletePickerModalWindowViewModel : ModalBaseViewModel<SeriesEntryViewModel>
    {
        private readonly ReadOnlyObservableCollection<SeriesEntryViewModel> _series;
        private readonly ReadOnlyObservableCollection<GameTitleEntryViewModel> _gameTitleEntries;
        public ReadOnlyObservableCollection<SeriesEntryViewModel> Series { get { return _series; } }

        public SeriesDeletePickerModalWindowViewModel(IObservable<IChangeSet<SeriesEntryViewModel, string>> observableSeries,
            IObservable<IChangeSet<GameTitleEntryViewModel, string>> observableGameTitleEntries)
        {
            //Bind observables
            observableSeries
               .Filter(p => p.Source == EntrySource.Mod)
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _series)
               .DisposeMany()
               .Subscribe();

            observableGameTitleEntries
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _gameTitleEntries)
               .DisposeMany()
               .Subscribe();

            this.ValidationRule(p => p.SelectedItem, x =>
                x != null &&
                !string.IsNullOrEmpty(x.UiSeriesId) &&
                x.Source == EntrySource.Mod &&
                _gameTitleEntries.FirstOrDefault(p => p.UiSeriesId == x.UiSeriesId) == null,
            $"Please select a custom series that is not associated to any game");
        }

        protected override IObservable<bool> GetValidationRule()
        {
            return this.WhenAnyValue(x => x.SelectedItem, x =>
                x != null &&
                !string.IsNullOrEmpty(x.UiSeriesId) &&
                x.Source == EntrySource.Mod &&
                _gameTitleEntries.FirstOrDefault(p => p.UiSeriesId == x.UiSeriesId) == null);
        }
    }
}
