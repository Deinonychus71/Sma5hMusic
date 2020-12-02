namespace Sm5shMusic.GUI.Models
{
    public class ComboItem
    {
        public bool AllFlag { get; }
        public string Id { get; }
        public string Label { get; }

        public ComboItem(string id, string label, bool allFlag = false)
        {
            Id = id;
            Label = label;
            AllFlag = allFlag;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            ComboItem p = obj as ComboItem;
            if (p == null)
                return false;

            return p.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
