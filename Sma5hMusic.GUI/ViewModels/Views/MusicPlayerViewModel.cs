using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using VGMMusic;

namespace Sma5hMusic.GUI.ViewModels
{
    public class MusicPlayerViewModel : ViewModelBase
    {
        private readonly ReadOnlyObservableCollection<BgmDbRootEntryViewModel> _items;
        private readonly IVGMMusicPlayer _musicPlayer;
        private float _audioVolume;
        private const string PLAY = "\u25B6";
        private const string PAUSE = "\u23F8";

        public ReactiveCommand<Unit, Unit> ActionPlaySong { get; }

        [Reactive]
        public bool IsVisible { get; set; }

        public float Volume
        {
            get
            {
                return _audioVolume;
            }
            set
            {
                _audioVolume = value;
                if (_musicPlayer != null)
                    _musicPlayer.GlobalVolume = _audioVolume;
            }
        }

        public string Filename { get; private set; }

        [Reactive]
        public string Title { get; private set; }

        [Reactive]
        public string GameTitle { get; private set; }

        [Reactive]
        public string PlaybackButtonText { get; set; }

        public MusicPlayerViewModel(IVGMMusicPlayer musicPlayer, IObservable<IChangeSet<BgmDbRootEntryViewModel, string>> observableDbRootEntries)
        {
            _musicPlayer = musicPlayer;
            _musicPlayer.PlaybackStarted += OnPlaybackStarted;
            _musicPlayer.PlaybackStopped += OnPlaybackStopped;
            _musicPlayer.PlaybackPaused += OnPlaybackPaused;
            _musicPlayer.PlaybackPositionChanged += OnPlaybackPositionChanged;

            observableDbRootEntries
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _items)
                .DisposeMany()
                .Subscribe();

            PlaybackButtonText = PLAY;
            Volume = 1.0f;

            ActionPlaySong = ReactiveCommand.CreateFromTask(TriggerButton);
        }

        private void OnPlaybackPositionChanged(object sender, VGMMusic.EventHandlers.PlaybackPositionEventArgs e)
        {
        }

        private void OnPlaybackStopped(object sender, VGMMusic.EventHandlers.PlaybackEventArgs e)
        {
            this.IsVisible = false;
        }

        private void OnPlaybackPaused(object sender, VGMMusic.EventHandlers.PlaybackEventArgs e)
        {
            PlaybackButtonText = PLAY;
        }

        private void OnPlaybackStarted(object sender, VGMMusic.EventHandlers.PlaybackEventArgs e)
        {
            _musicPlayer.GlobalVolume = _audioVolume;
            PlaybackButtonText = PAUSE;
            var refVmDbRootEntry = _items.FirstOrDefault(p => p.Filename == e.Filename);
            if (refVmDbRootEntry != null)
            {
                Title = refVmDbRootEntry.Title;
                GameTitle = refVmDbRootEntry.GameTitleViewModel?.Title;
            }
            this.IsVisible = true;
        }

        public async Task TriggerButton()
        {
            if (_musicPlayer.IsPlaying)
                await StopSong();
            else
                await PlaySong();
        }

        public async Task PlaySong()
        {
            await _musicPlayer.Play(Filename);
        }

        public async Task StopSong()
        {
            await _musicPlayer.Stop();
        }
    }
}
