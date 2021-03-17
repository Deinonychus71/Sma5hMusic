using AutoMapper;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sma5h.Mods.Music.Models;
using Sma5hMusic.GUI.Helpers;
using Sma5hMusic.GUI.Interfaces;
using System;
using System.IO;
using VGMMusic;

namespace Sma5hMusic.GUI.ViewModels
{
    public class BgmPropertyEntryViewModel : BgmBaseViewModel<BgmPropertyEntry>
    {
        private readonly IVGMMusicPlayer _vgmPlayer;
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
                    MusicPlayer.AudioVolume = ConvertVolumeToMusicPlayer(value);
            }
        }

        //Music player
        public bool DoesFileExist { get; set; }
        public MusicPlayerViewModel MusicPlayer { get; set; }

        public BgmPropertyEntryViewModel(IVGMMusicPlayer vgmPlayer, IViewModelManager viewModelManager, IMapper mapper, BgmPropertyEntry bgmPropertyEntry)
            : base(viewModelManager, mapper, bgmPropertyEntry)
        {
            _vgmPlayer = vgmPlayer;
            NameId = bgmPropertyEntry.NameId;
            Filename = bgmPropertyEntry.Filename;

            DoesFileExist = File.Exists(Filename);
            if (DoesFileExist)
                MusicPlayer = new MusicPlayerViewModel(vgmPlayer, Filename)
                {
                    AudioVolume = ConvertVolumeToMusicPlayer(bgmPropertyEntry.AudioVolume)
                };
        }

        public override ReactiveObjectBaseViewModel GetCopy()
        {
            return _mapper.Map(this, new BgmPropertyEntryViewModel(_vgmPlayer, _viewModelManager, _mapper, GetReferenceEntity()));
        }

        public override ReactiveObjectBaseViewModel SaveChanges()
        {
            var original = _viewModelManager.GetBgmPropertyViewModel(NameId);
            _mapper.Map(this, original.GetReferenceEntity());
            _mapper.Map(this, original);
            return original;
        }

        private float ConvertVolumeToMusicPlayer(float volume)
        {
            if (volume == 0)
                return Constants.DefaultVolume;
            return (volume + Constants.MaximumGameVolume) / (Constants.MaximumGameVolume*2f);
        }
    }
}
