using ReactiveUI.Fody.Helpers;
using Sm5sh.Mods.Music.Models;
using VGMMusic;
using System.IO;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models.BgmEntryModels;
using System.Collections.Generic;
using Sm5sh.GUI.Models;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmEntryViewModel : BgmEntryEditableViewModel
    {
        //Helper Getters
        public IMusicMod MusicMod { get { return _refBgmEntry.MusicMod; } }
        public EntrySource Source { get { return _refBgmEntry != null ? _refBgmEntry.Source : EntrySource.Unknown; } }
        public SeriesEntryViewModel SeriesViewModel { get { return GameTitleViewModel?.SeriesViewModel; } }
        public string GameId { get { return GameTitleViewModel.GameId; } }
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
        //public bool HiddenInSoundTest { get { return DbRoot.TestDispOrder == -1; } }
        //public short SoundTestIndex { get { return DbRoot.TestDispOrder; } }
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
        //Necessary so the drag & drop / refresh can work better
        [Reactive]
        public short SoundTestIndex { get; set; }
        [Reactive]
        public bool HiddenInSoundTest { get; set; }

        public BgmEntryViewModel(IVGMMusicPlayer musicPlayer, BgmEntry bgmEntry)
            : base(bgmEntry)
        {
            //Music Player
            DoesFileExist = File.Exists(bgmEntry.Filename);
            if (DoesFileExist)
                MusicPlayer = new MusicPlayerViewModel(musicPlayer, bgmEntry.Filename);

            SoundTestIndex = DbRoot.TestDispOrder;
            HiddenInSoundTest = DbRoot.TestDispOrder == -1;
        }

        public void LoadLocalized(string locale)
        {
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

            GameTitleViewModel?.LoadLocalized(locale);
        }
    }
}
