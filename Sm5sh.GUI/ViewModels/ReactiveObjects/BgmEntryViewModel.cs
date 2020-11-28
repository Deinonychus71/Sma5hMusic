using ReactiveUI.Fody.Helpers;
using Sm5sh.Mods.Music.Models;
using VGMMusic;
using System.IO;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models.BgmEntryModels;
using System.Collections.Generic;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmEntryViewModel : BgmEntryEditableViewModel
    {
        //Helper Getters
        public IMusicMod MusicMod { get { return _refBgmEntry.MusicMod; } }
        public EntrySource Source { get { return _refBgmEntry != null ? _refBgmEntry.Source : EntrySource.Unknown; } }
        public SeriesEntryViewModel SeriesViewModel { get { return GameTitleViewModel?.SeriesViewModel; } }
        public string UiGameTitleId { get { return GameTitleViewModel.UiGameTitleId; } }
        public string SeriesId { get { return GameTitleViewModel.SeriesId; } }
        public string ModName { get { return MusicMod?.Name; } }
        public string ModAuthor { get { return MusicMod?.Mod.Author; } }
        public string ModWebsite { get { return MusicMod?.Mod.Website; } }
        public string ModId { get { return MusicMod?.Id; } }
        public string ModPath { get { return MusicMod?.ModPath; } }
        public bool IsMod { get { return Source == EntrySource.Mod; } }
        public Dictionary<string, string> MSBTTitle { get { return MSBTLabels.Title; } }
        public Dictionary<string, string> MSBTAuthor { get { return MSBTLabels.Author; } }
        public Dictionary<string, string> MSBTCopyright { get { return MSBTLabels.Copyright; } }
        public bool HiddenInSoundTest { get { return DbRoot.TestDispOrder == -1; } }
        public string RecordType { get { return DbRoot.RecordType; } }
        public string SpecialCategoryLabel { get { return StreamSet.SpecialCategory; } }
        public string SpecialParam1Label { get { return StreamSet.Info1; } }
        public string SpecialParam2Label { get { return StreamSet.Info2; } }
        public string SpecialParam3Label { get { return StreamSet.Info3; } }
        public string SpecialParam4Label { get { return StreamSet.Info4; } }


        //Getters/Private Setters - For This View Only
        public MusicPlayerViewModel MusicPlayer { get; private set; }
        public bool DoesFileExist { get; private set; }
        //To obtain reactive change for locale
        [Reactive]
        public string Title { get; set; }
        [Reactive]
        public string Copyright { get; set; }
        [Reactive]
        public string Author { get; set; }

        public BgmEntryViewModel(IVGMMusicPlayer musicPlayer, BgmEntry bgmEntry)
            : base(bgmEntry)
        {
            if (bgmEntry != null)
            {
                if (musicPlayer != null)
                {
                    //Music Player
                    DoesFileExist = File.Exists(bgmEntry.Filename);
                    if (DoesFileExist)
                        MusicPlayer = new MusicPlayerViewModel(musicPlayer, bgmEntry.Filename);
                }
            }
        }

        public void LoadLocalized(string locale)
        {
            if (string.IsNullOrEmpty(locale))
                return;

            if (MSBTTitle != null && MSBTTitle.ContainsKey(locale))
                Title = MSBTTitle[locale];
            else
                Title = ToneId;

            if (MSBTCopyright != null && MSBTCopyright.ContainsKey(locale))
                Copyright = MSBTCopyright[locale];
            else
                Copyright = string.Empty;

            if (MSBTAuthor != null && MSBTAuthor.ContainsKey(locale))
                Author = MSBTAuthor[locale];
            else
                Author = string.Empty;
        }
    }
}
