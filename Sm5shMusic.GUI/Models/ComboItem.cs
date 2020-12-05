namespace Sm5shMusic.GUI.Models
{
    public class ComboItem
    {
        public bool DefaultFlag { get; }
        public string Id { get; }
        public string Label { get; }

        public ComboItem(string id, string label, bool defaultFlag = false)
        {
            Id = id;
            Label = label;
            DefaultFlag = defaultFlag;
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
