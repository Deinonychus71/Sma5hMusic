using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sm5sh.Mods.Music.Models;
using Sm5shMusic.GUI.Helpers;

namespace Sm5shMusic.GUI.ViewModels
{
    public class StageEntryViewModel : ReactiveObject
    {
        private readonly StageEntry _refStageEntry;

        public string UiStageId { get { return _refStageEntry.UiStageId; } }

        public string Title { get { return Constants.GetStageDisplayName(UiStageId); } }

        [Reactive]
        public string PlaylistId { get; set; }

        [Reactive]
        public byte OrderId { get; set; }

        public StageEntryViewModel() { }

        public StageEntryViewModel(StageEntry stageEntry)
        {
            _refStageEntry = stageEntry;
            PlaylistId = _refStageEntry.BgmSetId;
            OrderId = _refStageEntry.BgmSettingNo;
        }

        public StageEntry GetStageEntryReference()
        {
            return _refStageEntry;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is StageEntryViewModel p))
                return false;

            return p.UiStageId == this.UiStageId;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
