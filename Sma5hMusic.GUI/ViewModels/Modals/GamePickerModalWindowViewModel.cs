using DynamicData;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace Sma5hMusic.GUI.ViewModels
{
    public class GamePickerModalWindowViewModel : ModalBaseViewModel<GameTitleEntryViewModel>
    {
        private readonly ReadOnlyObservableCollection<GameTitleEntryViewModel> _games;
        public ReadOnlyObservableCollection<GameTitleEntryViewModel> Games { get { return _games; } }

        public GamePickerModalWindowViewModel(IObservable<IChangeSet<GameTitleEntryViewModel, string>> observableGames)
        {
            //Bind observables
            observableGames
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _games)
               .DisposeMany()
               .Subscribe();
        }

        protected override IObservable<bool> GetValidationRule()
        {
            return this.WhenAnyValue(x => x.SelectedItem, x => x != null && !string.IsNullOrEmpty(x.UiSeriesId));
        }
    }
}
