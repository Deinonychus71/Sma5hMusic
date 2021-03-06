using DynamicData;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace Sma5hMusic.GUI.ViewModels
{
    public class PlaylistPickerModalWindowViewModel : ModalBaseViewModel<PlaylistEntryViewModel>
    {
        private readonly ReadOnlyObservableCollection<PlaylistEntryViewModel> _playlist;

        public ReadOnlyObservableCollection<PlaylistEntryViewModel> Playlists { get { return _playlist; } }

        public PlaylistPickerModalWindowViewModel(IObservable<IChangeSet<PlaylistEntryViewModel, string>> observablePlaylists)
        {
            //Bind observables
            observablePlaylists
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _playlist)
               .DisposeMany()
               .Subscribe();
        }

        protected override IObservable<bool> GetValidationRule()
        {
            return this.WhenAnyValue(x => x.SelectedItem, x => x != null && !string.IsNullOrEmpty(x.Title));
        }
    }
}
