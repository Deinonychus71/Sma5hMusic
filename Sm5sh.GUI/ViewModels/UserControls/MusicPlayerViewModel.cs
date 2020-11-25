using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using VGMMusic;

namespace Sm5sh.GUI.ViewModels
{
    public class MusicPlayerViewModel : ViewModelBase
    {
        private readonly IVGMMusicPlayer _musicPlayer;
        private const string PLAY = "\u25B6";
        private const string STOP = "\u23F9";
        private bool _isPlaying;
        private static MusicPlayerViewModel _currentPlayControl;

        public ReactiveCommand<Unit, Unit> ActionPlaySong { get; }

        public string Filename { get; private set; }

        [Reactive]
        public string Text { get; set; }

        public MusicPlayerViewModel(IVGMMusicPlayer musicPlayer, string filename)
        {
            _musicPlayer = musicPlayer;
            Filename = filename;

            Text = PLAY;

            ActionPlaySong = ReactiveCommand.Create(TriggerButton);
        }

        public void TriggerButton()
        {
            if (_isPlaying)
                StopSong();
            else
                PlaySong();
        }

        public void PlaySong()
        {
            if (_currentPlayControl != null)
                _currentPlayControl.StopSong();

            _musicPlayer.Play(Filename);
            Text = STOP;
            _currentPlayControl = this;
            _isPlaying = true;
        }

        public void StopSong()
        {
            _musicPlayer.Stop();
            Text = PLAY;
            _isPlaying = false;
        }
    }
}
