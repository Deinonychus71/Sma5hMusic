using DynamicData;
using ReactiveUI;
using Sma5h.Mods.Music.Helpers;
using Sma5hMusic.GUI.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace Sma5hMusic.GUI.ViewModels
{
    public class SeriesPickerModalWindowViewModel : ModalBaseViewModel<SeriesEntryViewModel>
    {
        private readonly ReadOnlyObservableCollection<SeriesEntryViewModel> _series;
        public ReadOnlyObservableCollection<SeriesEntryViewModel> Series { get { return _series; } }

        public SeriesPickerModalWindowViewModel(IViewModelManager viewModelManager)
        {
            //Bind observables
            viewModelManager.ObservableSeries.Connect()
               .Filter(p => p.UiSeriesId != MusicConstants.InternalIds.SERIES_ID_DEFAULT && !MusicConstants.INVALID_SERIES.Contains(p.UiSeriesId))
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _series)
               .DisposeMany()
               .Subscribe();
        }

        protected override IObservable<bool> GetValidationRule()
        {
            return this.WhenAnyValue<SeriesPickerModalWindowViewModel, bool, SeriesEntryViewModel>(x => x.SelectedItem, x => x != null);
        }
    }
}
