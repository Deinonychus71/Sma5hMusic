using ReactiveUI.Fody.Helpers;
using Sm5sh.GUI.Helpers;
using Sm5sh.Mods.Music.Models;
using System.Collections.Generic;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmEntryListViewModel : ViewModelBase
    {
        private readonly Dictionary<string, string> _title;
        private readonly Dictionary<string, string> _gameTitle;

        public bool AllFlag { get; set; }
        public string ToneId { get; private set; }
        public string SeriesId { get; set; }
        public string GameId { get; set; }
        public string SeriesTitle { get; set; }
        [Reactive]
        public string GameTitle { get; set; }
        [Reactive]
        public string Title { get; private set; }
        public string Filename { get; private set; }
        public string RecordTypeId { get; private set; }
        public string RecordTypeLabel { get; private set; }
        public bool HiddenInSoundTest { get; private set; }

        public string ModName { get; set; }
        public bool IsMod { get; private set; }
        public Mods.Music.Models.BgmEntryModels.EntrySource Source { get; private set; }

        public BgmEntryListViewModel() { }

        public BgmEntryListViewModel(BgmEntry bgmEntry)
        {
            ToneId = bgmEntry.ToneId;
            SeriesId = bgmEntry.GameTitle?.SeriesId;
            SeriesTitle = Constants.GetSeriesDisplayName(SeriesId);
            GameId = bgmEntry.GameTitle?.GameTitleId;
            Filename = bgmEntry.FileName;
            RecordTypeId = bgmEntry.RecordType;
            RecordTypeLabel = Constants.GetRecordTypeDisplayName(bgmEntry.RecordType);
            Source = bgmEntry.Source;
            ModName = bgmEntry.ModName;
            HiddenInSoundTest = bgmEntry.HiddenInSoundTest;
            IsMod = Source == Mods.Music.Models.BgmEntryModels.EntrySource.Mod;

            _title = bgmEntry.Title;
            _gameTitle = bgmEntry.GameTitle?.Title;
        }

        public void LoadLocalized(string locale)
        {
            if (_gameTitle != null && _gameTitle.ContainsKey(locale))
                GameTitle = _gameTitle[locale];
            else
                GameTitle = GameId;

            if (_title != null && _title.ContainsKey(locale))
                Title = _title[locale];
            else
                Title = ToneId;
        }
    }
}
