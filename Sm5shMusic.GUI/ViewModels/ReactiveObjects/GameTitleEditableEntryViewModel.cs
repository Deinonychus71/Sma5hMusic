using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sm5sh.Mods.Music.Models;
using System.Collections.Generic;

namespace Sm5shMusic.GUI.ViewModels
{
    public class GameTitleEditableEntryViewModel : ReactiveObject
    {
        protected readonly GameTitleEntry _refGameTitleEntry;

        public string UiGameTitleId { get { return _refGameTitleEntry?.UiGameTitleId; } }
        public EntrySource Source { get { return _refGameTitleEntry.Source; } }
        public string NameId { get; set; }
        public string UiSeriesId { get; set; }
        public bool Unk1 { get; set; }
        public int Release { get; set; }
        public Dictionary<string, string> MSBTTitle { get; set; }

        [Reactive]
        public SeriesEntryViewModel SeriesViewModel { get; set; }

        public GameTitleEditableEntryViewModel() { }

        public GameTitleEditableEntryViewModel(GameTitleEntry gameTitleEntry)
        {
            _refGameTitleEntry = gameTitleEntry;
        }

        public GameTitleEntry GetGameEntryReference()
        {
            return _refGameTitleEntry;
        }
    }

    namespace GameTitleEditableEntryViewModels
    {

    }
}
