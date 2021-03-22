
using System;

namespace VGMMusic.EventHandlers
{
    public class PlaybackPositionEventArgs : EventArgs
    {
        public long Position { get; set; }

        public PlaybackPositionEventArgs(long position)
        {
            Position = position;
        }
    }
}
