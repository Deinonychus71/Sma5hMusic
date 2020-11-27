using Sm5sh.Mods.Music.Interfaces;

namespace Sm5sh.GUI.Models
{
    public class ModItem
    {
        public bool CreateFlag { get; set; }
        public IMusicMod Id { get;  }
        public string Label { get; set; }

        public ModItem() { }

        public ModItem(IMusicMod id, string label, bool allFlag = false)
        {
            Id = id;
            Label = label;
            CreateFlag = allFlag;
        }
    }
}
