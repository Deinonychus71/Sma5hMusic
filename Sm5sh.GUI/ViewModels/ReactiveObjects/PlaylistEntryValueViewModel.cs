using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;

namespace Sm5sh.GUI.ViewModels
{
    public class PlaylistEntryValueViewModel : ReactiveObject
    {
        public string UiBgmId { get; private set; }

        //Necessary for Grid as some values aren't unique
        public string UniqueId { get; set; }

        public PlaylistEntryViewModel Parent { get; }

        [Reactive]
        public BgmEntryViewModel BgmReference { get; set; }

        [Reactive]
        public ushort Incidence { get; set; }

        public string IncidencePercentage { get { return $"{Math.Round((double)(Incidence / 100), 2)} %"; } }

        [Reactive]
        public short Order { get; set; }

        public PlaylistEntryValueViewModel(PlaylistEntryViewModel parent, short orderId, string bgmId, short order, ushort incidence, BgmEntryViewModel vmBgmEntry = null)
        {
            Parent = parent;
            UniqueId = Guid.NewGuid().ToString(); // $"{bgmId}{orderId}";
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

            return p.UniqueId == this.UniqueId;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
