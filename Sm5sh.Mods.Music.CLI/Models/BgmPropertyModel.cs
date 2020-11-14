using System;
using System.Collections.Generic;
using System.Text;

namespace Sm5shMusic.Models
{
    public class BgmPropertyModel
    {
        public string NameId { get; set; }
        public ulong LoopStartMs { get; set; }
        public ulong LoopStartSample { get; set; }
        public ulong LoopEndMs { get; set; }
        public ulong LoopEndSample { get; set; }
        public ulong TotalTimeMs { get; set; }
        public ulong TotalSamples { get; set; }
    }
}
