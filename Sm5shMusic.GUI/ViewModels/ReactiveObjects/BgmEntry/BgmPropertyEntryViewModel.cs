using AutoMapper;
using ReactiveUI.Fody.Helpers;
using Sm5sh.Mods.Music.Models;
using Sm5shMusic.GUI.Interfaces;
using System.IO;
using VGMMusic;

namespace Sm5shMusic.GUI.ViewModels
{
    public class BgmPropertyEntryViewModel : BgmBaseViewModel<BgmPropertyEntry>
    {
        private readonly IVGMMusicPlayer _vgmPlayer;

        public string NameId { get; }
        public uint LoopStartMs { get; set; }
        public uint LoopStartSample { get; set; }
        public uint LoopEndMs { get; set; }
        public uint LoopEndSample { get; set; }
        public uint TotalTimeMs { get; set; }
        public uint TotalSamples { get; set; }
        public uint Frequency { get { return TotalTimeMs == 0 ? 0 : (uint)((double)TotalSamples / (double)TotalTimeMs * 1000.0); } }
        public string Filename { get; }
        public string AudioVolume { get; set; }

        //Music player
        public bool DoesFileExist { get; set; }
        public MusicPlayerViewModel MusicPlayer { get; set; }

        public BgmPropertyEntryViewModel(IVGMMusicPlayer vgmPlayer, IAudioStateViewModelManager audioStateManager, IMapper mapper, BgmPropertyEntry bgmPropertyEntry)
            : base(audioStateManager, mapper, bgmPropertyEntry)
        {
            _vgmPlayer = vgmPlayer;
            NameId = bgmPropertyEntry.NameId;
            Filename = bgmPropertyEntry.Filename;

            DoesFileExist = File.Exists(Filename);
            if (DoesFileExist)
                MusicPlayer = new MusicPlayerViewModel(vgmPlayer, Filename);
        }

        public override BgmBaseViewModel<BgmPropertyEntry> Clone()
        {
            return _mapper.Map(this, new BgmPropertyEntryViewModel(_vgmPlayer, _audioStateManager, _mapper, new BgmPropertyEntry(NameId, Filename, MusicMod)));
        }
    }
}
