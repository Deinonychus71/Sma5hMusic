using Sm5shMusic.Helpers;
using System.Collections.Generic;

namespace Sm5shMusic.Models
{
    public class BgmDbNewEntry
    {
        public string ToneName { get; set; }
        public string GameTitleId { get; set; }

        //Db Root
        public string UiBgmId { get { return $"{Constants.InternalIds.BgmIdPrefix}{ToneName}"; } }
        public string StreamSetId { get { return $"{Constants.InternalIds.SetIdPrefix}{ToneName}"; } }
        public string Rarity { get; set; }
        public string RecordType { get; set; }
        public string NameId { get; set; }
        public string UiGameTitleId { get { return $"{Constants.InternalIds.GameTitleIdPrefix}{GameTitleId}"; } }
        public bool IsDlc { get; set; }
        public bool IsPatch { get; set; }

        //Stream Set
        public string Info0 { get { return $"{Constants.InternalIds.InfoPrefix}{ToneName}"; } }

        //AssignedInfo
        public string InfoId { get { return $"{Constants.InternalIds.InfoPrefix}{ToneName}"; } }
        public string StreamId { get { return $"{Constants.InternalIds.StreamPrefix}{ToneName}"; } }

        //StreamProperty
        public string DataName0 { get { return ToneName; } }

        //Playlist
        public List<BgmPlaylistEntry> Playlists { get; set; }
    }

    public class BgmPlaylistEntry
    {
        public string Id { get; set; }
    }
}
