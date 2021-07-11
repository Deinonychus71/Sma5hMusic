using DynamicData;
using ReactiveUI;
using Sma5hMusic.GUI.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace Sma5hMusic.GUI.ViewModels
{
    public class PlaylistPickerModalWindowViewModel : ModalBaseViewModel<PlaylistEntryViewModel>
    {
        private readonly ReadOnlyObservableCollection<PlaylistEntryViewModel> _playlist;

        public ReadOnlyObservableCollection<PlaylistEntryViewModel> Playlists { get { return _playlist; } }

        public PlaylistPickerModalWindowViewModel(IViewModelManager viewModelManager)
        {
            //Bind observables
            viewModelManager.ObservablePlaylistsEntries.Connect()
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
