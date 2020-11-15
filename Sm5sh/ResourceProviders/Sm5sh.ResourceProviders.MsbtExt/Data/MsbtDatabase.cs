using Sm5sh.Interfaces;
using System.Collections.Generic;

namespace Sm5sh.Data
{
    public class MsbtDatabase : IStateManagerDb
    {
        public Dictionary<string, string> Entries { get; set; }
    }
}
