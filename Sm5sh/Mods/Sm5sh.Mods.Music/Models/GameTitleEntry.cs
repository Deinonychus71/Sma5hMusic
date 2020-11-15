using System.Collections.Generic;

namespace Sm5sh.Mods.Music.Models
{
    public class GameTitleEntry
    {
        public string GameTitleId { get; set; }

        public string NameId { get; set; } //Need because Game Title Id doesn't always have a direct match with NameId

        public string SeriesId { get; set; }

        public Dictionary<string, string> Title { get; set; }

        public override string ToString()
        {
            return GameTitleId;
        }
    }
}
