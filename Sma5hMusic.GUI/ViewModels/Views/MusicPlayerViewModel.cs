using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using System.Threading.Tasks;
using VGMMusic;

namespace Sma5hMusic.GUI.ViewModels
{
    public class MusicPlayerViewModel : ViewModelBase
    {
        private readonly IVGMMusicPlayer _musicPlayer;
        public const float DefaultMusicPlayerVolume = 0.5f;
        private float _audioVolume;
        private const string PLAY = "\u25B6";
        private const string STOP = "\u23F9";
        private bool _isPlaying;
        private bool _isExecutingAction;
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
                _audioVolume = value;
                if (_musicPlayer != null)
                    _musicPlayer.Volume = _audioVolume;
            }
        }

        public string Filename { get; private set; }

        [Reactive]
        public string Text { get; set; }

        public MusicPlayerViewModel(IVGMMusicPlayer musicPlayer, string filename)
        {
            _musicPlayer = musicPlayer;
            Filename = filename;

            Text = PLAY;

            ActionPlaySong = ReactiveCommand.CreateFromTask(TriggerButton, this.WhenAnyValue(p => p._isExecutingAction, p => p == false));
        }

        public async Task ChangeFilename(string filename)
        {
            if (_isPlaying)
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

            _musicPlayer.Volume = AudioVolume;
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
    }
}
