using DynamicData;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace Sma5hMusic.GUI.ViewModels
{
    public class SeriesPickerModalWindowViewModel : ModalBaseViewModel<SeriesEntryViewModel>
    {
        private readonly ReadOnlyObservableCollection<SeriesEntryViewModel> _series;
        public ReadOnlyObservableCollection<SeriesEntryViewModel> Games { get { return _series; } }

        public SeriesPickerModalWindowViewModel(IObservable<IChangeSet<SeriesEntryViewModel, string>> observableSeries)
        {
            //Bind observables
            observableSeries
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
