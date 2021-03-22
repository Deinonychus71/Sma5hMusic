
using System;

namespace VGMMusic.EventHandlers
{
    public class PlaybackEventArgs : EventArgs
    {
        public string Filename { get; private set; }

        public PlaybackEventArgs(string filename)
        {
            Filename = filename;
        }
    }
}
