using AutoMapper;
using ReactiveUI.Fody.Helpers;
using Sm5sh.Mods.Music.Models;
using Sm5shMusic.GUI.Interfaces;

namespace Sm5shMusic.GUI.ViewModels
{
    public class BgmAssignedInfoEntryViewModel : BgmBaseViewModel<BgmAssignedInfoEntry>
    {
        public string InfoId { get; }
        public string StreamId { get; set; }
        public BgmStreamPropertyEntryViewModel StreamPropertyViewModel { get { return _audioStateManager.GetBgmStreamPropertyViewModel(StreamId); } }
        public string Condition { get; set; }
        public string ConditionProcess { get; set; }
        public int StartFrame { get; set; }
        public int ChangeFadeInFrame { get; set; }
        public int ChangeStartDelayFrame { get; set; }
        public int ChangeFadoutFrame { get; set; }
        public int ChangeStopDelayFrame { get; set; }
        public int MenuChangeFadeInFrame { get; set; }
        public int MenuChangeStartDelayFrame { get; set; }
        public int MenuChangeFadeOutFrame { get; set; }
        public int Unk1 { get; set; }

        public BgmAssignedInfoEntryViewModel(IAudioStateViewModelManager audioStateManager, IMapper mapper, BgmAssignedInfoEntry bgmAssignedInfoEntry)
            : base(audioStateManager, mapper, bgmAssignedInfoEntry)
        {
            InfoId = bgmAssignedInfoEntry.InfoId;
        }

        public override BgmBaseViewModel<BgmAssignedInfoEntry> Clone()
        {
            return _mapper.Map(this, new BgmAssignedInfoEntryViewModel(_audioStateManager, _mapper, new BgmAssignedInfoEntry(InfoId, MusicMod)));
        }
    }
}
