using System;

namespace Sm5sh.Mods.Music.Models
{
    public class ModEntry
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Website { get; set; }
        public string Path { get; }
        public Guid Id { get; }

        public ModEntry(Guid id, string path)
        {
            Id = id;
            Path = path;
        }
    }
}
