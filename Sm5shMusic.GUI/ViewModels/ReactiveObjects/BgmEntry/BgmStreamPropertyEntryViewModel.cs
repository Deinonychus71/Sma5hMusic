using AutoMapper;
using ReactiveUI.Fody.Helpers;
using Sm5sh.Mods.Music.Models;
using Sm5shMusic.GUI.Interfaces;

namespace Sm5shMusic.GUI.ViewModels
{
    public class BgmStreamPropertyEntryViewModel : BgmBaseViewModel<BgmStreamPropertyEntry>
    {
        public string StreamId { get; }
        public string DataName0 { get; set; }
        public BgmPropertyEntryViewModel DataName0ViewModel { get { return _audioStateManager.GetBgmPropertyViewModel(DataName0); } }
        public string DataName1 { get; set; }
        public BgmPropertyEntryViewModel DataName1ViewModel { get { return _audioStateManager.GetBgmPropertyViewModel(DataName1); } }
        public string DataName2 { get; set; }
        public BgmPropertyEntryViewModel DataName2ViewModel { get { return _audioStateManager.GetBgmPropertyViewModel(DataName2); } }
        public string DataName3 { get; set; }
        public BgmPropertyEntryViewModel DataName3ViewModel { get { return _audioStateManager.GetBgmPropertyViewModel(DataName3); } }
        public string DataName4 { get; set; }
        public BgmPropertyEntryViewModel DataName4ViewModel { get { return _audioStateManager.GetBgmPropertyViewModel(DataName4); } }
        public byte Loop { get; set; }
        public string EndPoint { get; set; }
        public ushort FadeOutFrame { get; set; }
        public string StartPointSuddenDeath { get; set; }
        public string StartPointTransition { get; set; }
        public string StartPoint0 { get; set; }
        public string StartPoint1 { get; set; }
        public string StartPoint2 { get; set; }
        public string StartPoint3 { get; set; }
        public string StartPoint4 { get; set; }

        public BgmStreamPropertyEntryViewModel(IAudioStateViewModelManager audioStateManager, IMapper mapper, BgmStreamPropertyEntry bgmStreamPropertyEntry)
            : base(audioStateManager, mapper, bgmStreamPropertyEntry)
        {
            StreamId = bgmStreamPropertyEntry.StreamId;
        }

        public override BgmBaseViewModel<BgmStreamPropertyEntry> Clone()
        {
            return _mapper.Map(this, new BgmStreamPropertyEntryViewModel(_audioStateManager, _mapper, new BgmStreamPropertyEntry(StreamId, MusicMod)));
        }
    }
}
