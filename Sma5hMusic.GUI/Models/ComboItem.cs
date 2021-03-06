namespace Sma5hMusic.GUI.Models
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

            if (!(obj is ComboItem p))
                return false;

            return p.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
