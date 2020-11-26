using Newtonsoft.Json;
using Sm5sh.Mods.Music.MusicOverride.MusicOverrideConfigModels;
using System.Collections.Generic;

namespace Sm5sh.Mods.Music.MusicOverride
{
    public class MusicOverrideConfig
    {
        public Dictionary<string, short> SoundTestOrder { get; set; }
        public Dictionary<string, List<BgmPlaylistConfig>> Playlists { get; set; }
    }

    namespace MusicOverrideConfigModels
    {
        public class BgmPlaylistConfig
        {
            [JsonProperty("ui_bgm_id")]
            public string UiBgmId { get; set; }

            [JsonProperty("order0")]
            public short Order0 { get; set; }

            [JsonProperty("incidence0")]
            public ushort Incidence0 { get; set; }

            [JsonProperty("order1")]
            public short Order1 { get; set; }

            [JsonProperty("incidence1")]
            public ushort Incidence1 { get; set; }

            [JsonProperty("order2")]
            public short Order2 { get; set; }

            [JsonProperty("incidence2")]
            public ushort Incidence2 { get; set; }

            [JsonProperty("order3")]
            public short Order3 { get; set; }

            [JsonProperty("incidence3")]
            public ushort Incidence3 { get; set; }

            [JsonProperty("order4")]
            public short Order4 { get; set; }

            [JsonProperty("incidence4")]
            public ushort Incidence4 { get; set; }

            [JsonProperty("order5")]
            public short Order5 { get; set; }

            [JsonProperty("incidence5")]
            public ushort Incidence5 { get; set; }

            [JsonProperty("order6")]
            public short Order6 { get; set; }

            [JsonProperty("incidence6")]
            public ushort Incidence6 { get; set; }

            [JsonProperty("order7")]
            public short Order7 { get; set; }

            [JsonProperty("incidence7")]
            public ushort Incidence7 { get; set; }

            [JsonProperty("order8")]
            public short Order8 { get; set; }

            [JsonProperty("incidence8")]
            public ushort Incidence8 { get; set; }

            [JsonProperty("order9")]
            public short Order9 { get; set; }

            [JsonProperty("incidence9")]
            public ushort Incidence9 { get; set; }

            [JsonProperty("order10")]
            public short Order10 { get; set; }

            [JsonProperty("incidence10")]
            public ushort Incidence10 { get; set; }

            [JsonProperty("order11")]
            public short Order11 { get; set; }

            [JsonProperty("incidence11")]
            public ushort Incidence11 { get; set; }

            [JsonProperty("order12")]
            public short Order12 { get; set; }

            [JsonProperty("incidence12")]
            public ushort Incidence12 { get; set; }

            [JsonProperty("order13")]
            public short Order13 { get; set; }

            [JsonProperty("incidence13")]
            public ushort Incidence13 { get; set; }

            [JsonProperty("order14")]
            public short Order14 { get; set; }

            [JsonProperty("incidence14")]
            public ushort Incidence14 { get; set; }

            [JsonProperty("order15")]
            public short Order15 { get; set; }

            [JsonProperty("incidence15")]
            public ushort Incidence15 { get; set; }
        }
    }
}
