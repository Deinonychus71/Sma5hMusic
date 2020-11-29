using Avalonia.Controls;
using DynamicData;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;

namespace Sm5sh.GUI.ViewModels
{
    public class PlaylistPickerModalWindowViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private readonly ReadOnlyObservableCollection<PlaylistEntryViewModel> _playlist;

        public ReadOnlyObservableCollection<PlaylistEntryViewModel> Playlists { get { return _playlist; } }

        public ReactiveCommand<Window, Unit> ActionCancel { get; }
        public ReactiveCommand<Window, Unit> ActionSelect { get; }

        [Reactive]
        public PlaylistEntryViewModel SelectedPlaylistEntry { get; private set; }


        public PlaylistPickerModalWindowViewModel(ILogger<PlaylistPickerModalWindowViewModel> logger, IObservable<IChangeSet<PlaylistEntryViewModel, string>> observablePlaylists)
        {
            _logger = logger;

            //Bind observables
            observablePlaylists
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _playlist)
               .DisposeMany()
               .Subscribe();

            var canExecute = this.WhenAnyValue(x => x.SelectedPlaylistEntry, x => x != null && !string.IsNullOrEmpty(x.Title));
            ActionCancel = ReactiveCommand.Create<Window>(Cancel);
            ActionSelect = ReactiveCommand.Create<Window>(Select, canExecute);
        }

        private void Cancel(Window w)
        {
            SelectedPlaylistEntry = null;
            w.Close();
        }

        private void Select(Window window)
        {
            window.Close(window);
        }
    }
}
