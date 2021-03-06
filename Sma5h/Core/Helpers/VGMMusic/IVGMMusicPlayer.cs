﻿using System.Threading.Tasks;

namespace VGMMusic
{
    public interface IVGMMusicPlayer
    {
        int TotalTime { get; }
        int CurrentTime { get; }
        bool Loaded { get; }
        bool Play();
        float Volume { get; set; }
        Task<bool> Play(string filename);
        Task<VGMAudioCuePoints> GetAudioCuePoints(string filename);
        Task<bool> Stop();
    }
}
