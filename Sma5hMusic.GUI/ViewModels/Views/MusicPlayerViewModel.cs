using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sma5hMusic.GUI.Helpers;
using System;
using System.Reactive;
using System.Threading.Tasks;
using VGMMusic;

namespace Sma5hMusic.GUI.ViewModels
{
    public class MusicPlayerViewModel : ViewModelBase
    {
        private readonly IVGMMusicPlayer _musicPlayer;
        private float _audioVolume;
        private const string PLAY = "\u25B6";
        private const string STOP = "\u23F9";
        private bool _isPlaying;
        private bool _isExecutingAction;
        private readonly bool _inGameVolume;
        private static MusicPlayerViewModel _currentPlayControl;

        public ReactiveCommand<Unit, Unit> ActionPlaySong { get; }

        public float AudioVolume
        {
            get
            {
                return _audioVolume;
            }
            set
            {
                _audioVolume = ConvertVolumeToMusicPlayer(value);
                if (_musicPlayer != null && _isPlaying)
                    _musicPlayer.InGameVolume = _audioVolume;
            }
        }

        public string Filename { get; private set; }

        [Reactive]
        public string Text { get; set; }

        public MusicPlayerViewModel(IVGMMusicPlayer musicPlayer, string filename, bool inGameVolume = false)
        {
            _musicPlayer = musicPlayer;
            _inGameVolume = inGameVolume;
            Filename = filename;

            Text = PLAY;

            ActionPlaySong = ReactiveCommand.CreateFromTask(TriggerButton, this.WhenAnyValue(p => p._isExecutingAction, p => p == false));
        }

        public async Task ChangeFilename(string filename)
        {
            if (_isPlaying && Filename != filename)
                await StopSong();
            Filename = filename;
        }

        public async Task TriggerButton()
        {
            _isExecutingAction = true;
            if (_isPlaying)
                await StopSong();
            else
                await PlaySong();
            // _isExecutingAction = false;
        }

        public async Task PlaySong()
        {
            if (_currentPlayControl != null)
                await _currentPlayControl.StopSong();

            _musicPlayer.InGameVolume = AudioVolume;
            await _musicPlayer.Play(Filename);
            Text = STOP;
            _currentPlayControl = this;
            _isPlaying = true;
        }

        public async Task StopSong()
        {
            await _musicPlayer.Stop();
            Text = PLAY;
            _isPlaying = false;
        }

        private float ConvertVolumeToMusicPlayer(float volume)
        {
            if (!_inGameVolume)
                return Constants.DefaultVolume;

            /*
             * -15      0.016666668
             * -10      0.033333335
             *  -5	    0.06666667
             *  -2	    0.09166667
             *  -1	    0.09583333
             *   0	    0.11093858
             *   1	    0.12522507
             *   2	    0.14385904
             *   5	    0.19510613
             *   6.2    0.246151
             *  10	    0.36688885
             *  15	    0.59292334
             *  20	    1
             */
            var y = 5.29809 / (1 + 45.2203 * Math.Exp(-0.117516 * volume));
            return y >= 1.0 ? 1.0f : Convert.ToSingle(y);
        }
    }
}
