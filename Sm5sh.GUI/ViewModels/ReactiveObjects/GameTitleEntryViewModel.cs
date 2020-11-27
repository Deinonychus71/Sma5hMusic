using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sm5sh.Mods.Music.Models;

namespace Sm5sh.GUI.ViewModels
{
    public class GameTitleEntryViewModel : ReactiveObject
    {
        private readonly GameTitleEntry _refGameTitleEntry;

        public bool AllFlag { get; set; }

        public SeriesEntryViewModel SeriesViewModel { get; set; }
        public string SeriesId { get { return SeriesViewModel.SeriesId; } }

        public string GameId { get; }
        [Reactive]
        public string Title { get; set; }

        public GameTitleEntryViewModel() { }

        public GameTitleEntryViewModel(GameTitleEntry gameTitleEntry)
        {
            _refGameTitleEntry = gameTitleEntry;
            GameId = gameTitleEntry.UiGameTitleId;
        }

        public void LoadLocalized(string locale)
        {
            if (_refGameTitleEntry.MSBTTitle != null && _refGameTitleEntry.MSBTTitle.ContainsKey(locale))
                Title = _refGameTitleEntry.MSBTTitle[locale];
            else
                Title = GameId;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            GameTitleEntryViewModel p = obj as GameTitleEntryViewModel;
            if (p == null)
                return false;

            return p.GameId == this.GameId;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
