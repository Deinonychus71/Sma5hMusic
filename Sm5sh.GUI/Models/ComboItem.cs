namespace Sm5sh.GUI.Models
{
    public class ComboItem
    {
        public bool AllFlag { get; }
        public string Id { get;  }
        public string Label { get;  }

        public ComboItem(string id, string label, bool allFlag = false)
        {
            Id = id;
            Label = label;
            AllFlag = allFlag;
        }
    }
}
