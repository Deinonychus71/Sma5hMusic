using ReactiveUI.Fody.Helpers;
using Sm5sh.Mods.Music.Models;

namespace Sm5shMusic.GUI.ViewModels
{
    public class GameTitleEntryViewModel : GameTitleEditableEntryViewModel
    {
        //Getters/Private Setters - For This View Only
        public bool AllFlag { get; set; }
        public string SeriesId { get { return SeriesViewModel.SeriesId; } }

        //To obtain reactive change for locale
        [Reactive]
        public string Title { get; set; }

        public GameTitleEntryViewModel() { }

        public GameTitleEntryViewModel(GameTitleEntry gameTitleEntry)
            : base(gameTitleEntry)
        {
        }

        public void LoadLocalized(string locale)
        {
            if (string.IsNullOrEmpty(locale))
                return;

            if (MSBTTitle != null && MSBTTitle.ContainsKey(locale))
                Title = MSBTTitle[locale];
            else
                Title = UiGameTitleId;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            GameTitleEntryViewModel p = obj as GameTitleEntryViewModel;
            if (p == null)
                return false;

            return p.UiGameTitleId == this.UiGameTitleId;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
