//From lioncash
//https://github.com/lioncash/vgmstreamSharp
using System;
using NAudio.Wave;
using VGMMusic.Native;

namespace VGMMusic
{
    /// <summary>
    /// Class for VGMStream playback.
    /// </summary>
    public sealed class VGMStreamReader : WaveStream, IDisposable
    {
        private readonly WaveFormat _waveFormat;
        private readonly IntPtr _vgmstream;

        private readonly int _totalSamplesToPlay;           // Total samples to play
        private readonly int _channels;      // Number of channels this VGMSTREAM uses.
        private readonly int _sampleRate;    // Sample rate of this VGMSTREAM.
        private readonly int loopCount = 1; // Number of times to loop. // TODO: Make configurable.
        private readonly int _loopStartSample;
        private readonly int _loopEndSample;
        private readonly int _totalSamples;
        private readonly bool _fileLoaded = false;
        private int _totalPlayed = 0;

        public int TotalPlayed { get { return _totalPlayed / _sampleRate; } }

        public int TotalSamplesToPlay { get { return _totalSamplesToPlay; } }
        public int TotalSecondsToPlay { get { return _totalSamplesToPlay / _sampleRate; } }
        public int LoopStartSample { get { return _loopStartSample; } }
        public int LoopEndSample { get { return _loopEndSample; } }
        public int TotalSamples { get { return _totalSamples; } }
        public int LoopStartMilliseconds { get { return (int)(_loopStartSample / (_sampleRate / 1000.00)); } }
        public int LoopEndMilliseconds { get { return (int)(_loopEndSample / (_sampleRate / 1000.00)); } }
        public int TotalMilliseconds { get { return (int)(_totalSamples / (_sampleRate / 1000.00)); } }
        public bool FileLoaded { get { return _fileLoaded; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filename">File to open in VGMStream.</param>
        public VGMStreamReader(string filename)
        {
            _vgmstream = VGMStreamNative.InitVGMStream(filename);
            if (_vgmstream == IntPtr.Zero)
            {
                _fileLoaded = false;
                return;
            }

            _fileLoaded = true;
            _sampleRate = VGMStreamNative.GetVGMStreamSampleRate(_vgmstream);
            _channels = VGMStreamNative.GetVGMStreamChannelCount(_vgmstream);
            _totalSamplesToPlay = VGMStreamNative.GetVGMStreamPlaySamples(loopCount, 0, 0, _vgmstream);

            _loopStartSample = VGMStreamNative.GetVGMStreamLoopStartSample(_vgmstream);
            _loopEndSample = VGMStreamNative.GetVGMStreamLoopEndSample(_vgmstream) - 1; //Smash values always seem to be 1 less.
            _totalSamples = VGMStreamNative.GetVGMStreamTotalSamples(_vgmstream);

            _waveFormat = new WaveFormat(_sampleRate, 16, _channels);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            short[] sBuffer = new short[count / 2];

            // Generate samples.
            VGMStreamNative.RenderVGMStream(sBuffer, sBuffer.Length / _channels, _vgmstream);

            // Convert the short samples to byte samples and place them
            // in the NAudio byte sample buffer.
            Buffer.BlockCopy(sBuffer, 0, buffer, 0, buffer.Length);

            _totalPlayed += sBuffer.Length / _channels;

            return buffer.Length;
        }

        public override WaveFormat WaveFormat
        {
            get { return _waveFormat; }
        }

        public override long Length
        {
            get
            {
                int lengthInMs = VGMStreamNative.GetVGMStreamPlaySamples(0, 0, 0, _vgmstream) * 1000 / _sampleRate;
                return lengthInMs; // TODO: This should actually be in samples or bytes.
            }
        }

        // TODO: Add seeking support.
        public override long Position
        {
            get;
            set;
        }

        public void ResetVGM()
        {
            _totalPlayed = 0;
            VGMStreamNative.ResetVGMStream(_vgmstream);
        }

        #region IDisposable Methods

        public new void Dispose()
        {
            base.Dispose();

            try
            {
                VGMStreamNative.CloseVGMStream(_vgmstream);
            }
            catch
            {
            }
        }

        #endregion
    }
}