using AutoMapper;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sma5h.Mods.Music.Models;
using Sma5hMusic.GUI.Interfaces;
using System.IO;
using VGMMusic;

namespace Sma5hMusic.GUI.ViewModels
{
    public class BgmPropertyEntryViewModel : BgmBaseViewModel<BgmPropertyEntry>
    {
        private readonly IVGMMusicPlayer _vgmPlayer;
        private readonly bool _inGameVolume;
        private float _audioVolume;
        private uint _totalTimeMs;
        private uint _totalSamples;

        public string NameId { get; }
        [Reactive]
        public uint LoopStartMs { get; set; }
        [Reactive]
        public uint LoopStartSample { get; set; }
        [Reactive]
        public uint LoopEndMs { get; set; }
        [Reactive]
        public uint LoopEndSample { get; set; }
        public uint TotalTimeMs
        {
            get => _totalTimeMs;
            set
            {
                this.RaiseAndSetIfChanged(ref _totalTimeMs, value);
                this.RaisePropertyChanged(nameof(Frequency));
            }
        }
        public uint TotalSamples
        {
            get => _totalSamples;
            set
            {
                this.RaiseAndSetIfChanged(ref _totalSamples, value);
                this.RaisePropertyChanged(nameof(Frequency));
            }
        }
        public uint Frequency { get { return TotalTimeMs == 0 ? 0 : (uint)((double)TotalSamples / (double)TotalTimeMs * 1000.0); } }
        [Reactive]
        public string Filename { get; set; }
        //[Reactive]
        public float AudioVolume
        {
            get
            {
                return _audioVolume;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _audioVolume, value);
                if (MusicPlayer != null)
                    MusicPlayer.AudioVolume = value;
            }
        }

        //Music player
        public bool DoesFileExist { get; set; }
        public MusicButtonViewModel MusicPlayer { get; set; }

        public BgmPropertyEntryViewModel(IVGMMusicPlayer vgmPlayer, IViewModelManager viewModelManager, IMapper mapper, BgmPropertyEntry bgmPropertyEntry, bool inGameVolume = false)
            : base(viewModelManager, mapper, bgmPropertyEntry)
        {
            _vgmPlayer = vgmPlayer;
            _inGameVolume = inGameVolume;
            NameId = bgmPropertyEntry.NameId;
            Filename = bgmPropertyEntry.Filename;

            DoesFileExist = File.Exists(Filename);
            if (DoesFileExist)
                MusicPlayer = new MusicButtonViewModel(vgmPlayer, Filename, inGameVolume)
                {
                    AudioVolume = bgmPropertyEntry.AudioVolume
                };
        }

        public override ReactiveObjectBaseViewModel GetCopy()
        {
            return _mapper.Map(this, new BgmPropertyEntryViewModel(_vgmPlayer, _viewModelManager, _mapper, GetReferenceEntity(), _inGameVolume));
        }

        public override ReactiveObjectBaseViewModel SaveChanges()
        {
            var original = _viewModelManager.GetBgmPropertyViewModel(NameId);
            _mapper.Map(this, original.GetReferenceEntity());
            _mapper.Map(this, original);
            return original;
        }
    }
}
