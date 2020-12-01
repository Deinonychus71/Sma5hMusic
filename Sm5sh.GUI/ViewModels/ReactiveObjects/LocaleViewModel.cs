using ReactiveUI;

namespace Sm5sh.GUI.ViewModels
{
    public class LocaleViewModel : ReactiveObject
    {
        public bool AllFlag { get; }
        public string Id { get; }
        public string Label { get; }

        public LocaleViewModel(string id, string label, bool allFlag = false)
        {
            Id = id;
            Label = label;
            AllFlag = allFlag;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            LocaleViewModel p = obj as LocaleViewModel;
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
