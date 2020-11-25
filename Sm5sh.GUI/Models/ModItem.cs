using Sm5sh.Mods.Music.Interfaces;

namespace Sm5sh.GUI.Models
{
    public class ModItem
    {
        public bool CreateFlag { get; }
        public IMusicMod Id { get;  }
        public string Label { get;  }

        public ModItem(IMusicMod id, string label, bool allFlag = false)
        {
            Id = id;
            Label = label;
            CreateFlag = allFlag;
        }
    }
}
