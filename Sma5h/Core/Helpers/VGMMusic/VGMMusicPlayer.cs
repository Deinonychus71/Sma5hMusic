using Microsoft.Extensions.Logging;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;
using System.Threading.Tasks;
using VGMMusic.EventHandlers;

namespace VGMMusic
{
    public class VGMMusicPlayer : IVGMMusicPlayer, IDisposable
    {
        private readonly ILogger _logger;
        private VGMStreamReader _reader;
        private VolumeSampleProvider _sampleProvider;
        private WaveOutEvent _outputDevice;
        private string _filename;
        private bool _requestStop;
        private float _inGameVolume;
        private float _globalVolume;

        public delegate void PlaybackEventHandler(object sender, PlaybackEventArgs e);
        public delegate void PlaybackPositionEventHandler(object sender, PlaybackPositionEventArgs e);
        public event PlaybackEventHandler PlaybackStarted;
        public event PlaybackEventHandler PlaybackPaused;
        public event PlaybackEventHandler PlaybackStopped;
        public event PlaybackPositionEventHandler PlaybackPositionChanged;

        public int TotalTime { get { return _reader != null ? _reader.TotalSecondsToPlay : 0; } }
        public int CurrentTime { get { return _reader != null ? _reader.TotalPlayed : 0; } }
        public bool Loaded { get { return _reader != null && _reader.FileLoaded; } }
        public float InGameVolume { get { return _inGameVolume; } set { _inGameVolume = value; SetInGameVolume(value); } }
        public float GlobalVolume { get { return _globalVolume; } set { _globalVolume = value; SetGlobalVolume(value); } }
        public bool IsPlaying { get { return _outputDevice != null && _outputDevice.PlaybackState == PlaybackState.Playing; } }

        public VGMMusicPlayer(ILogger<IVGMMusicPlayer> logger)
        {
            _logger = logger;
            GlobalVolume = 1.0f;
        }

        public async Task<bool> LoadFile(string filename)
        {
            //Test file exist
            if (!File.Exists(filename))
            {
                _logger.LogError("Error while loading {FileName}, file doesn't exist.", filename);
                return false;
            }

            //Dispose current stream, if exist
            await Stop();

            //Attempt to load file
            _reader = new VGMStreamReader(filename);
            _sampleProvider = new VolumeSampleProvider(_reader.ToSampleProvider())
            {
                Volume = GlobalVolume
            };

            if (!_reader.FileLoaded)
            {
                _logger.LogError("Error while loading {FileName}. VGMStreamReader could not load the file. If this file plays on foobar make sure that libvgmstream is properly installed.", filename);
                return false;
            }

            _filename = filename;

            return true;
        }

        public async Task<VGMAudioCuePoints> GetAudioCuePoints(string filename)
        {
            await LoadFile(filename);
            var audioCuePoints = new VGMAudioCuePoints()
            {
                LoopEndMs = _reader.LoopEndMilliseconds,
                LoopEndSample = _reader.LoopEndSample,
                LoopStartMs = _reader.LoopStartMilliseconds,
                LoopStartSample = _reader.LoopStartSample,
                TotalTimeMs = _reader.TotalMilliseconds,
                TotalSamples = _reader.TotalSamples,
            };
            await Stop();
            return audioCuePoints;
        }

        public bool Play()
        {
            if (!Loaded)
            {
                _logger.LogError("Error starting playback. The stream is not ready.");
                return false;
            }

            try
            {
                _outputDevice = new WaveOutEvent();
                if (_reader != null)
                {
                    _outputDevice.PlaybackStopped += OnPlaybackStopped;
                    _outputDevice.Init(_sampleProvider);
                    _outputDevice.Volume = InGameVolume;
                    _outputDevice.Play();
                    PlaybackStarted?.Invoke(this, new PlaybackEventArgs(_filename));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, "Error while initializing/playing the song {FileName}", _filename);
                InternalStop();
            }

            return true;
        }

        public async Task<bool> Play(string filename)
        {
            if (await LoadFile(filename))
            {
                return Play();
            }
            return false;
        }

        private void SetInGameVolume(float volume)
        {
            try
            {
                if (_outputDevice != null)
                    _outputDevice.Volume = volume;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, "Error while setting in-game volume: {Volume}", volume);
            }
        }

        private void SetGlobalVolume(float volume)
        {
            try
            {
                if (_sampleProvider != null)
                    _sampleProvider.Volume = GlobalVolume;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, "Error while setting global volume: {Volume}", volume);
            }
        }

        public async Task<bool> Stop()
        {
            if (_reader != null && IsPlaying)
            {
                _outputDevice?.Stop();
                PlaybackStopped?.Invoke(this, new PlaybackEventArgs(_filename));
                _requestStop = true;
                while (IsPlaying || _requestStop)
                {
                    await Task.Delay(200);
                }
            }
            else
            {
                InternalStop();
            }

            return true;
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            InternalStop();
        }

        private void InternalStop()
        {
            if(_outputDevice != null)
                _outputDevice.PlaybackStopped -= OnPlaybackStopped;
            _outputDevice?.Dispose();
            _outputDevice = null;
            _reader?.Dispose();
            _reader = null;
            _requestStop = false;
        }

        public void Dispose()
        {
            InternalStop();
        }
    }
}
