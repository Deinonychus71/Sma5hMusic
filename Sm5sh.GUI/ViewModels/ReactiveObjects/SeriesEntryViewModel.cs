using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sm5sh.GUI.Helpers;

namespace Sm5sh.GUI.ViewModels
{
    public class SeriesEntryViewModel : ReactiveObject
    {
        public bool AllFlag { get; set; }
        public string SeriesId { get; }
        public string Title { get; set; }

        public SeriesEntryViewModel() { }

        public SeriesEntryViewModel(string seriesId)
        {
            SeriesId = seriesId;

            //Calculated Fields
            Title = Constants.GetSeriesDisplayName(SeriesId);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            SeriesEntryViewModel p = obj as SeriesEntryViewModel;
            if (p == null)
                return false;

            return p.SeriesId == this.SeriesId;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
