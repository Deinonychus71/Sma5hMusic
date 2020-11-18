using Sm5sh.Interfaces;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Sm5sh.Mods.Music.Data.Sound.Config
{
    public class BinBgmProperty : IStateManagerDb
    {
        public Dictionary<string, BgmPropertyStructs.BgmPropertyEntry> Entries { get; set; }
    }

    namespace BgmPropertyStructs
    {
        public class BgmPropertyEntry
        {
            [YamlMember(Alias = "name_id")]
            public string NameId { get; set; }

            [YamlMember(Alias = "loop_start_ms")]
            public ulong LoopStartMs { get; set; }

            [YamlMember(Alias = "loop_start_sample")]
            public ulong LoopStartSample { get; set; }

            [YamlMember(Alias = "loop_end_ms")]
            public ulong LoopEndMs { get; set; }

            [YamlMember(Alias = "loop_end_sample")]
            public ulong LoopEndSample { get; set; }

            [YamlMember(Alias = "total_time_ms")]
            public ulong TotalTimeMs { get; set; }

            [YamlMember(Alias = "total_samples")]
            public ulong TotalSamples { get; set; }

            public override string ToString()
            {
                return NameId;
            }
        }
    }
}
