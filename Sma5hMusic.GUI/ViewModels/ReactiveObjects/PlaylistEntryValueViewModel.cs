using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;

namespace Sma5hMusic.GUI.ViewModels
{
    public class PlaylistEntryValueViewModel : ReactiveObject
    {
        public string UiBgmId { get; private set; }

        //Necessary for Grid as some values aren't unique
        public string UniqueId { get; set; }

        public PlaylistEntryViewModel Parent { get; }

        [Reactive]
        public BgmDbRootEntryViewModel BgmReference { get; set; }

        [Reactive]
        public ushort Incidence { get; set; }

        public string IncidencePercentage { get { return $"{Math.Round((double)(Incidence / 100), 2)} %"; } }

        [Reactive]
        public bool Hidden { get; set; }

        [Reactive]
        public short Order { get; set; }

        public PlaylistEntryValueViewModel(PlaylistEntryViewModel parent, string bgmId, short order, ushort incidence, BgmDbRootEntryViewModel vmBgmEntry = null)
        {
            Parent = parent;
            UniqueId = Guid.NewGuid().ToString(); // $"{bgmId}{orderId}";
            UiBgmId = bgmId;
            Order = order;
            if (order == -1)
                Hidden = true;
            Incidence = incidence;
            //In case not found :)
            BgmReference = vmBgmEntry ?? new BgmDbRootEntryViewModel(null, null, null)
            {
                Title = bgmId
            };
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is PlaylistEntryValueViewModel p))
                return false;

            return p.UniqueId == this.UniqueId;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
