using ReactiveUI.Fody.Helpers;
using Sm5sh.GUI.Helpers;
using Sm5sh.Mods.Music.Models;
using VGMMusic;
using System.Linq;
using System;
using System.IO;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmEntryListViewModel : ViewModelBase
    {
        private readonly BgmEntry _refBgmEntry;

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
        public string ModPath { get; set; }
        public Mods.Music.Models.BgmEntryModels.EntrySource Source { get; private set; }
        public string PlaylistIds { get; private set; }
        public string SpecialCategoryLabel { get; private set; }
        public string SpecialParam1Label { get; private set; }
        public string SpecialParam2Label { get; private set; }
        public string SpecialParam3Label { get; private set; }
        public string SpecialParam4Label { get; private set; }

        public MusicPlayerViewModel MusicPlayer { get; set; }
        public bool IsMod { get { return Source == Mods.Music.Models.BgmEntryModels.EntrySource.Mod; } }


        public BgmEntryListViewModel(IVGMMusicPlayer musicPlayer, BgmEntry bgmEntry)
        {
            _refBgmEntry = bgmEntry;

            if (bgmEntry.Source == Mods.Music.Models.BgmEntryModels.EntrySource.Mod)
                MusicPlayer = new MusicPlayerViewModel(musicPlayer, bgmEntry.Filename);

            //1:1 Mapping with BgmEntry
            Source = bgmEntry.Source;
            ToneId = bgmEntry.ToneId;
            RecordTypeId = bgmEntry.RecordType;
            SoundTestIndex = bgmEntry.SoundTestIndex;
            HiddenInSoundTest = bgmEntry.HiddenInSoundTest;
            SeriesId = bgmEntry.GameTitle?.SeriesId;
            GameId = bgmEntry.GameTitle?.GameTitleId;
            Filename = bgmEntry.Filename;
            ModName = bgmEntry.Mod?.ModName;
            ModAuthor = bgmEntry.Mod?.ModAuthor;
            ModWebsite = bgmEntry.Mod?.ModWebsite;
            ModPath = bgmEntry.Mod?.ModPath;

            //Calculated Fields
            SeriesTitle = Constants.GetSeriesDisplayName(SeriesId);
            RecordTypeLabel = Constants.GetRecordTypeDisplayName(bgmEntry.RecordType);
            PlaylistIds = string.Join(Environment.NewLine, bgmEntry.Playlists.Select(p => p.Id));
            if (bgmEntry.SpecialCategory != null)
            {
                SpecialCategoryLabel = Constants.GetSpecialCategoryDisplayName(bgmEntry.SpecialCategory.Id);
                if (bgmEntry.SpecialCategory.Parameters.Count >= 1)
                    SpecialParam1Label = bgmEntry.SpecialCategory.Parameters[0];
                if (bgmEntry.SpecialCategory.Parameters.Count >= 2)
                    SpecialParam2Label = bgmEntry.SpecialCategory.Parameters[1];
                if (bgmEntry.SpecialCategory.Parameters.Count >= 3)
                    SpecialParam3Label = bgmEntry.SpecialCategory.Parameters[2];
                if (bgmEntry.SpecialCategory.Parameters.Count >= 4)
                    SpecialParam4Label = bgmEntry.SpecialCategory.Parameters[3];
            }
        }

        public BgmEntryListViewModel() { }

        public void LoadLocalized(string locale)
        {
            if (_refBgmEntry.GameTitle.Title != null && _refBgmEntry.GameTitle.Title.ContainsKey(locale))
                GameTitle = _refBgmEntry.GameTitle.Title[locale];
            else
                GameTitle = GameId;

            if (_refBgmEntry.Title != null && _refBgmEntry.Title.ContainsKey(locale))
                Title = _refBgmEntry.Title[locale];
            else
                Title = ToneId;

            if (_refBgmEntry.Copyright != null && _refBgmEntry.Copyright.ContainsKey(locale))
                Copyright = _refBgmEntry.Copyright[locale];
            else
                Copyright = string.Empty;

            if (_refBgmEntry.Author != null && _refBgmEntry.Author.ContainsKey(locale))
                Author = _refBgmEntry.Author[locale];
            else
                Author = string.Empty;
        }
    }
}
