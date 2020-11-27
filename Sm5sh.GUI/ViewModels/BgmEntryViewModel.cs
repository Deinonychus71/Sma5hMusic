using ReactiveUI.Fody.Helpers;
using Sm5sh.GUI.Helpers;
using Sm5sh.Mods.Music.Models;
using VGMMusic;
using System.Linq;
using System;
using System.IO;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models.BgmEntryModels;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmEntryViewModel : ViewModelBase
    {
        private readonly BgmEntry _refBgmEntry;

        //Getters/Setters
        [Reactive]
        public GameTitleEntryViewModel GameTitleViewModel { get; set; }
        [Reactive]
        public string Title { get; set; }
        [Reactive]
        public string Copyright { get; set; }
        [Reactive]
        public string Author { get; set; }
        [Reactive]
        public string RecordTypeId { get; set; }
        [Reactive]
        public string RecordTypeLabel { get; set; }
        [Reactive]
        public bool HiddenInSoundTest { get; set; }
        [Reactive]
        public short SoundTestIndex { get; set; }
        [Reactive]
        public string SpecialCategoryLabel { get; set; }
        [Reactive]
        public string SpecialParam1Label { get; set; }
        [Reactive]
        public string SpecialParam2Label { get; set; }
        [Reactive]
        public string SpecialParam3Label { get; set; }
        [Reactive]
        public string SpecialParam4Label { get; set; }

        //Getters/Private Setters
        public MusicPlayerViewModel MusicPlayer { get; private set; }
        public bool DoesFileExist { get; private set; }

        //Getters
        public string ToneId { get; }
        public string Filename { get; }
        public SeriesEntryViewModel SeriesTitleViewModel { get { return GameTitleViewModel?.SeriesViewModel; } }
        public string GameId { get { return GameTitleViewModel.GameId; } }
        public string SeriesId { get { return GameTitleViewModel.SeriesId; } }
        public IMusicMod MusicMod { get; }
        public string ModName { get { return MusicMod?.Name; } }
        public string ModId { get { return MusicMod?.Id; } }
        public string ModPath { get { return MusicMod?.ModPath; } }
        public EntrySource Source { get { return _refBgmEntry != null ? _refBgmEntry.Source : EntrySource.Unknown; } }
        public bool IsMod { get { return Source == EntrySource.Mod; } }
       

        public BgmEntryViewModel(IVGMMusicPlayer musicPlayer, BgmEntry bgmEntry)
        {
            _refBgmEntry = bgmEntry;

            //1:1 Mapping with BgmEntry
            ToneId = bgmEntry.ToneId;
            RecordTypeId = bgmEntry.RecordType;
            SoundTestIndex = bgmEntry.SoundTestIndex;
            HiddenInSoundTest = bgmEntry.HiddenInSoundTest;
            Filename = bgmEntry.Filename;
            MusicMod = bgmEntry.MusicMod;

            //Music Player
            DoesFileExist = File.Exists(bgmEntry.Filename);
            if (DoesFileExist)
                MusicPlayer = new MusicPlayerViewModel(musicPlayer, bgmEntry.Filename);

            //Calculated Fields
            RecordTypeLabel = Constants.GetRecordTypeDisplayName(bgmEntry.RecordType);
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

        public BgmEntryViewModel() { }

        public void LoadLocalized(string locale)
        {
            if (_refBgmEntry.MSBTLabels.Title != null && _refBgmEntry.MSBTLabels.Title.ContainsKey(locale))
                Title = _refBgmEntry.MSBTLabels.Title[locale];
            else
                Title = ToneId;

            if (_refBgmEntry.MSBTLabels.Copyright != null && _refBgmEntry.MSBTLabels.Copyright.ContainsKey(locale))
                Copyright = _refBgmEntry.MSBTLabels.Copyright[locale];
            else
                Copyright = string.Empty;

            if (_refBgmEntry.MSBTLabels.Author != null && _refBgmEntry.MSBTLabels.Author.ContainsKey(locale))
                Author = _refBgmEntry.MSBTLabels.Author[locale];
            else
                Author = string.Empty;

            GameTitleViewModel?.LoadLocalized(locale);
        }
    }
}
