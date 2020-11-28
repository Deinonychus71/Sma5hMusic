using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Sm5sh.GUI.ViewModels
{
    public class PlaylistEntryValueViewModel : ReactiveObject
    {
        public string UiBgmId { get; private set; }

        [Reactive]
        public BgmEntryViewModel BgmReference { get; set; }

        public ushort Incidence { get; set; }
        public short Order { get; set; }

        public PlaylistEntryValueViewModel(string bgmId, short order, ushort incidence, BgmEntryViewModel vmBgmEntry = null)
        {
            UiBgmId = bgmId;
            Order = order;
            Incidence = incidence;
            //In case not found :)
            BgmReference = vmBgmEntry != null ? vmBgmEntry : new BgmEntryViewModel(null, null)
            {
                Title = bgmId
            };
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            PlaylistEntryValueViewModel p = obj as PlaylistEntryValueViewModel;
            if (p == null)
                return false;

            return p.UiBgmId == this.UiBgmId;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
