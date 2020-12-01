using System.Collections.Generic;

namespace Sm5sh.Mods.Music.Models
{
    public class PlaylistEntry
    {
        public string Id { get; }
        public string Title { get; set; }
        public List<PlaylistEntryModels.PlaylistValueEntry> Tracks { get; set; }

        public PlaylistEntry(string id, string title = null)
        {
            Id = id;
            Title = title;
            Tracks = new List<PlaylistEntryModels.PlaylistValueEntry>();
        }
    }

    namespace PlaylistEntryModels
    {
        public class PlaylistValueEntry
        {
            public string UiBgmId { get; set; }
            public short Order0 { get; set; }
            public ushort Incidence0 { get; set; }
            public short Order1 { get; set; }
            public ushort Incidence1 { get; set; }
            public short Order2 { get; set; }
            public ushort Incidence2 { get; set; }
            public short Order3 { get; set; }
            public ushort Incidence3 { get; set; }
            public short Order4 { get; set; }
            public ushort Incidence4 { get; set; }
            public short Order5 { get; set; }
            public ushort Incidence5 { get; set; }
            public short Order6 { get; set; }
            public ushort Incidence6 { get; set; }
            public short Order7 { get; set; }
            public ushort Incidence7 { get; set; }
            public short Order8 { get; set; }
            public ushort Incidence8 { get; set; }
            public short Order9 { get; set; }
            public ushort Incidence9 { get; set; }
            public short Order10 { get; set; }
            public ushort Incidence10 { get; set; }
            public short Order11 { get; set; }
            public ushort Incidence11 { get; set; }
            public short Order12 { get; set; }
            public ushort Incidence12 { get; set; }
            public short Order13 { get; set; }
            public ushort Incidence13 { get; set; }
            public short Order14 { get; set; }
            public ushort Incidence14 { get; set; }
            public short Order15 { get; set; }
            public ushort Incidence15 { get; set; }
        }
    }
}
