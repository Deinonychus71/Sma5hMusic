using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sma5h.Mods.Music.Models;
using Sma5hMusic.GUI.Helpers;

namespace Sma5hMusic.GUI.ViewModels
{
    public class StageEntryViewModel : ReactiveObject
    {
        private readonly StageEntry _refStageEntry;

        public string UiStageId { get { return _refStageEntry.UiStageId; } }

        public string Title { get { return Constants.GetStageDisplayName(UiStageId); } }

        public string TitleHidden { get { var name = Title; return name.StartsWith("(H)") ? name : ""; } }

        [Reactive]
        public string PlaylistId { get; set; }

        [Reactive]
        public byte OrderId { get; set; }

        [Reactive]
        public bool BgmSelector { get; set; }

        [Reactive]
        public sbyte DispOrder { get; set; }

        public bool Hidden { get { return DispOrder == -1 || Title.StartsWith("(H)"); } }

        public StageEntryViewModel() { }

        public StageEntryViewModel(StageEntry stageEntry)
        {
            _refStageEntry = stageEntry;
            PlaylistId = _refStageEntry.BgmSetId;
            OrderId = _refStageEntry.BgmSettingNo;
            BgmSelector = _refStageEntry.BgmSelector;
            DispOrder = _refStageEntry.DispOrder;
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
