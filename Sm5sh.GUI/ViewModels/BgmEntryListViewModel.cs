using ReactiveUI.Fody.Helpers;
using Sm5sh.GUI.Helpers;
using Sm5sh.Mods.Music.Models;
using System.Collections.Generic;
using VGMMusic;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmEntryListViewModel : ViewModelBase
    {
        private readonly Dictionary<string, string> _title;
        private readonly Dictionary<string, string> _author;
        private readonly Dictionary<string, string> _copyright;
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
        [Reactive]
        public string Copyright { get; set; }
        [Reactive]
        public string Author { get; set; }
        public string Filename { get; private set; }
        public string RecordTypeId { get; private set; }
        public string RecordTypeLabel { get; private set; }
        public bool HiddenInSoundTest { get; private set; }
        public short SoundTestIndex { get; private set; }

        public string ModName { get; set; }
        public string ModAuthor { get; set; }
        public string ModWebsite { get; set; }
        public bool IsMod { get; private set; }
        public Mods.Music.Models.BgmEntryModels.EntrySource Source { get; private set; }

        public MusicPlayerViewModel MusicPlayer { get; set; }

        public BgmEntryListViewModel() { }

        public BgmEntryListViewModel(IVGMMusicPlayer musicPlayer, BgmEntry bgmEntry)
        {
            MusicPlayer = new MusicPlayerViewModel(musicPlayer, bgmEntry.Mod?.Filename);

            ToneId = bgmEntry.ToneId;
            SeriesId = bgmEntry.GameTitle?.SeriesId;
            SeriesTitle = Constants.GetSeriesDisplayName(SeriesId);
            GameId = bgmEntry.GameTitle?.GameTitleId;
            RecordTypeId = bgmEntry.RecordType;
            RecordTypeLabel = Constants.GetRecordTypeDisplayName(bgmEntry.RecordType);
            
            HiddenInSoundTest = bgmEntry.HiddenInSoundTest;
            SoundTestIndex = bgmEntry.SoundTestIndex;

            Source = bgmEntry.Source;
            IsMod = Source == Mods.Music.Models.BgmEntryModels.EntrySource.Mod;
            Filename = bgmEntry.Mod?.Filename;
            ModName = bgmEntry.Mod?.ModName;
            ModAuthor = bgmEntry.Mod?.ModAuthor;
            ModWebsite = bgmEntry.Mod?.ModWebsite;

            _title = bgmEntry.Title;
            _gameTitle = bgmEntry.GameTitle?.Title;
            _author = bgmEntry.Author;
            _copyright = bgmEntry.Copyright;
        }

        public void LoadLocalized(string locale)
        {
            if (_gameTitle != null && _gameTitle.ContainsKey(locale))
                GameTitle = _gameTitle[locale];
            else
                GameTitle = string.Empty;

            if (_title != null && _title.ContainsKey(locale))
                Title = _title[locale];
            else
                Title = string.Empty;

            if (_copyright != null && _copyright.ContainsKey(locale))
                Copyright = _copyright[locale];
            else
                Copyright = string.Empty;

            if (_author != null && _author.ContainsKey(locale))
                Author = _author[locale];
            else
                Author = string.Empty;
        }
    }
}
