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
        public string DataName1 { get; set; }
        public string DataName2 { get; set; }
        public string DataName3 { get; set; }
        public string DataName4 { get; set; }
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

        //Getter Helpers
        public BgmPropertyEntryViewModel DataName0ViewModel { get { return _audioStateManager.GetBgmPropertyViewModel(DataName0); } }
        public BgmPropertyEntryViewModel DataName1ViewModel { get { return _audioStateManager.GetBgmPropertyViewModel(DataName1); } }
        public BgmPropertyEntryViewModel DataName2ViewModel { get { return _audioStateManager.GetBgmPropertyViewModel(DataName2); } }
        public BgmPropertyEntryViewModel DataName3ViewModel { get { return _audioStateManager.GetBgmPropertyViewModel(DataName3); } }
        public BgmPropertyEntryViewModel DataName4ViewModel { get { return _audioStateManager.GetBgmPropertyViewModel(DataName4); } }


        public BgmStreamPropertyEntryViewModel(IAudioStateViewModelManager audioStateManager, IMapper mapper, BgmStreamPropertyEntry bgmStreamPropertyEntry)
            : base(audioStateManager, mapper, bgmStreamPropertyEntry)
        {
            StreamId = bgmStreamPropertyEntry.StreamId;
        }

        public override BgmBaseViewModel<BgmStreamPropertyEntry> GetCopy()
        {
            return _mapper.Map(this, new BgmStreamPropertyEntryViewModel(_audioStateManager, _mapper, new BgmStreamPropertyEntry(StreamId, MusicMod)));
        }

        public override BgmBaseViewModel<BgmStreamPropertyEntry> SaveChanges()
        {
            var original = _audioStateManager.GetBgmStreamPropertyViewModel(StreamId);
            _mapper.Map(this, original.GetReferenceEntity());
            _mapper.Map(this, original);
            return original;
        }
    }
}
