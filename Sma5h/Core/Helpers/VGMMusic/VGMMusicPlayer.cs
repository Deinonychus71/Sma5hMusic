using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VGMMusic
{
    public class VGMMusicPlayer : IVGMMusicPlayer, IDisposable
    {
        private readonly ILogger _logger;
        private VGMStreamReader _reader;
        private WaveOutEvent _outputDevice;
        private string _filename;
        private bool _requestStop;
        private float _volume;

        public int TotalTime { get { return _reader != null ? _reader.TotalSecondsToPlay : 0; } }
        public int CurrentTime { get { return _reader != null ? _reader.TotalPlayed : 0; } }
        public bool Loaded { get { return _reader != null && _reader.FileLoaded; } }
        public float Volume { get { return _volume; } set { _volume = value; SetVolume(value); } }
        public bool IsPlaying { get; private set; }

        public VGMMusicPlayer(ILogger<IVGMMusicPlayer> logger)
        {
            _logger = logger;

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

            Task.Run(() => { InternalPlay(); });

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

        private void SetVolume(float volume)
        {
            try
            {
                if (_outputDevice != null)
                    _outputDevice.Volume = volume;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, "Error while setting volume: {Volume}", volume);
            }
        }

        public async Task<bool> Stop()
        {
            if (_reader != null && IsPlaying)
            {
                _requestStop = true;
                while (IsPlaying && _requestStop)
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

        private void InternalPlay()
        {
            try
            {
                _outputDevice = new WaveOutEvent();
                if (_reader != null)
                {
                    _outputDevice.Init(_reader);
                    _outputDevice.Volume = Volume;
                    _outputDevice.Play();
                    IsPlaying = true;
                    while (_outputDevice.PlaybackState == PlaybackState.Playing && !_requestStop)
                    {
                        Thread.Sleep(500);
                    }
                }
                _requestStop = false;

                /*using (var outputDevice = new WaveOutEvent())
                {
                    if (_reader != null)
                    {
                        outputDevice.Init(_reader);
                        outputDevice.Play();
                        IsPlaying = true;
                        while (outputDevice.PlaybackState == PlaybackState.Playing && !_requestStop)
                        {
                            Thread.Sleep(500);
                        }
                    }
                }
                _requestStop = false;*/
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, "Error while initializing/playing the song {FileName}", _filename);
            }
            finally
            {
                InternalStop();
            }
        }

        private void InternalStop()
        {
            _outputDevice?.Dispose();
            _outputDevice = null;
            _reader?.Dispose();
            _reader = null;
            _requestStop = false;
            IsPlaying = false;
        }

        public void Dispose()
        {
            InternalStop();
        }
    }
}
