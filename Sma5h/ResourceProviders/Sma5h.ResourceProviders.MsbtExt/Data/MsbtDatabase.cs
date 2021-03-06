using Sma5h.Interfaces;
using System.Collections.Generic;

namespace Sma5h.Data
{
    public class MsbtDatabase : IStateManagerDb
    {
        public Dictionary<string, string> Entries { get; set; }
    }
}
