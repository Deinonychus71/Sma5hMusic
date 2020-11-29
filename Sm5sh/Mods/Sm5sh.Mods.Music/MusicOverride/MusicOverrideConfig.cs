using Newtonsoft.Json;
using Sm5sh.Mods.Music.MusicMods.AdvancedMusicModModels;
using Sm5sh.Mods.Music.MusicOverride.MusicOverrideConfigModels;
using System.Collections.Generic;

namespace Sm5sh.Mods.Music.MusicOverride
{
    public class MusicOverrideConfig
    {
        public Dictionary<string, short> SoundTestOrder { get; set; }
        public Dictionary<string, List<PlaylistConfig>> Playlists { get; set; }
        public Dictionary<string, BgmConfig> CoreOverrides { get; set; }
        public Dictionary<string, StageConfig> StageOverrides { get; set; }
        public Dictionary<string, GameConfig> GameOverrides { get; set; }
    }

    namespace MusicOverrideConfigModels
    {
        public class PlaylistConfig
        {
            [JsonProperty("ui_bgm_id")]
            public string UiBgmId { get; set; }

            [JsonProperty("o0")]
            public short Order0 { get; set; }

            [JsonProperty("i0")]
            public ushort Incidence0 { get; set; }

            [JsonProperty("o1")]
            public short Order1 { get; set; }

            [JsonProperty("i1")]
            public ushort Incidence1 { get; set; }

            [JsonProperty("o2")]
            public short Order2 { get; set; }

            [JsonProperty("i2")]
            public ushort Incidence2 { get; set; }

            [JsonProperty("o3")]
            public short Order3 { get; set; }

            [JsonProperty("i3")]
            public ushort Incidence3 { get; set; }

            [JsonProperty("o4")]
            public short Order4 { get; set; }

            [JsonProperty("i4")]
            public ushort Incidence4 { get; set; }

            [JsonProperty("o5")]
            public short Order5 { get; set; }

            [JsonProperty("i5")]
            public ushort Incidence5 { get; set; }

            [JsonProperty("o6")]
            public short Order6 { get; set; }

            [JsonProperty("i6")]
            public ushort Incidence6 { get; set; }

            [JsonProperty("o7")]
            public short Order7 { get; set; }

            [JsonProperty("i7")]
            public ushort Incidence7 { get; set; }

            [JsonProperty("o8")]
            public short Order8 { get; set; }

            [JsonProperty("i8")]
            public ushort Incidence8 { get; set; }

            [JsonProperty("o9")]
            public short Order9 { get; set; }

            [JsonProperty("i9")]
            public ushort Incidence9 { get; set; }

            [JsonProperty("o10")]
            public short Order10 { get; set; }

            [JsonProperty("i10")]
            public ushort Incidence10 { get; set; }

            [JsonProperty("o11")]
            public short Order11 { get; set; }

            [JsonProperty("i11")]
            public ushort Incidence11 { get; set; }

            [JsonProperty("o12")]
            public short Order12 { get; set; }

            [JsonProperty("i12")]
            public ushort Incidence12 { get; set; }

            [JsonProperty("o13")]
            public short Order13 { get; set; }

            [JsonProperty("i13")]
            public ushort Incidence13 { get; set; }

            [JsonProperty("o14")]
            public short Order14 { get; set; }

            [JsonProperty("i14")]
            public ushort Incidence14 { get; set; }

            [JsonProperty("o15")]
            public short Order15 { get; set; }

            [JsonProperty("i15")]
            public ushort Incidence15 { get; set; }
        }

        public class StageConfig
        {
            [JsonProperty("playlist_id")]
            public string PlaylistId { get; set; }
            [JsonProperty("order_id")]
            public ushort OrderId { get; set; }
        }
    }
}
