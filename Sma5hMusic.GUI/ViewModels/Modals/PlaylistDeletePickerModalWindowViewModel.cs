using DynamicData;
using ReactiveUI;
using Sma5hMusic.GUI.Helpers;
using Sma5hMusic.GUI.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace Sma5hMusic.GUI.ViewModels
{
    public class PlaylistDeletePickerModalWindowViewModel : ModalBaseViewModel<PlaylistEntryViewModel>
    {
        private readonly ReadOnlyObservableCollection<PlaylistEntryViewModel> _playlist;

        public ReadOnlyObservableCollection<PlaylistEntryViewModel> Playlists { get { return _playlist; } }

        public PlaylistDeletePickerModalWindowViewModel(IViewModelManager viewModelManager)
        {
            //Bind observables
            viewModelManager.ObservablePlaylistsEntries.Connect()
               .Filter(p => !Constants.CONVERTER_CORE_PLAYLISTS.ContainsKey(p.Id))
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _playlist)
               .DisposeMany()
               .Subscribe();
        }

        protected override IObservable<bool> GetValidationRule()
        {
            return this.WhenAnyValue(x => x.SelectedItem, x => x != null && !string.IsNullOrEmpty(x.Title) && !Constants.CONVERTER_CORE_PLAYLISTS.ContainsKey(x.Id));
        }
    }
}
