using AutoMapper;
using ReactiveUI.Fody.Helpers;
using Sm5sh.Mods.Music.Models;
using Sm5shMusic.GUI.Interfaces;
using System.Collections.Generic;

namespace Sm5shMusic.GUI.ViewModels
{
    public class BgmDbRootEntryViewModel : BgmBaseViewModel<BgmDbRootEntry>
    {
        private string _currentLocale;

        //Getters/Setters
        public string UiBgmId { get; }
        public string StreamSetId { get; set; }
        public string Rarity { get; set; }
        public string RecordType { get; set; }
        public string UiGameTitleId { get; set; }
        public string UiGameTitleId1 { get; set; }
        public string UiGameTitleId2 { get; set; }
        public string UiGameTitleId3 { get; set; }
        public string UiGameTitleId4 { get; set; }
        public string NameId { get; set; }
        public short SaveNo { get; set; }
        public short TestDispOrder { get; set; }
        public int MenuValue { get; set; }
        public bool JpRegion { get; set; }
        public bool OtherRegion { get; set; }
        public bool Possessed { get; set; }
        public bool PrizeLottery { get; set; }
        public uint ShopPrice { get; set; }
        public bool CountTarget { get; set; }
        public byte MenuLoop { get; set; }
        public bool IsSelectableStageMake { get; set; }
        public bool Unk1 { get; set; }
        public bool Unk2 { get; set; }
        public bool IsDlc { get; set; }
        public bool IsPatch { get; set; }
        public string Unk3 { get; set; }
        public string Unk4 { get; set; }
        public string Unk5 { get; set; }
        public Dictionary<string, string> MSBTTitle { get; set; }
        public Dictionary<string, string> MSBTAuthor { get; set; }
        public Dictionary<string, string> MSBTCopyright { get; set; }

        //To obtain reactive change for locale
        [Reactive]
        public string Title { get; set; }
        [Reactive]
        public string Copyright { get; set; }
        [Reactive]
        public string Author { get; set; }


        //Helper Getters
        public BgmStreamSetEntryViewModel StreamSetViewModel { get { return _audioStateManager.GetBgmStreamSetViewModel(StreamSetId); } }
        public GameTitleEntryViewModel GameTitleViewModel { get { return _audioStateManager.GetGameTitleViewModel(UiGameTitleId); ; } }
        public GameTitleEntryViewModel GameTitle1ViewModel { get { return _audioStateManager.GetGameTitleViewModel(UiGameTitleId1); ; } }
        public GameTitleEntryViewModel GameTitle2ViewModel { get { return _audioStateManager.GetGameTitleViewModel(UiGameTitleId2); ; } }
        public GameTitleEntryViewModel GameTitle3ViewModel { get { return _audioStateManager.GetGameTitleViewModel(UiGameTitleId3); ; } }
        public GameTitleEntryViewModel GameTitle4ViewModel { get { return _audioStateManager.GetGameTitleViewModel(UiGameTitleId4); ; } }

        public bool HiddenInSoundTest { get { return TestDispOrder == -1; } }
        public SeriesEntryViewModel SeriesViewModel { get { return GameTitleViewModel?.SeriesViewModel; } }
        public BgmAssignedInfoEntryViewModel AssignedInfoViewModel { get { return StreamSetViewModel?.Info0ViewModel; } }
        public BgmStreamPropertyEntryViewModel StreamPropertyViewModel { get { return AssignedInfoViewModel?.StreamPropertyViewModel; } }
        public BgmPropertyEntryViewModel BgmPropertyViewModel { get { return StreamPropertyViewModel?.DataName0ViewModel; } }
        public string SeriesId { get { return GameTitleViewModel?.UiSeriesId; } }
        public string SpecialCategoryLabel { get { return StreamSetViewModel.SpecialCategory; } }
        public string SpecialParam1Label { get { return StreamSetViewModel.Info1; } }
        public string SpecialParam2Label { get { return StreamSetViewModel.Info2; } }
        public string SpecialParam3Label { get { return StreamSetViewModel.Info3; } }
        public string SpecialParam4Label { get { return StreamSetViewModel.Info4; } }
        public string ToneId { get { return StreamPropertyViewModel?.DataName0 != null ? StreamPropertyViewModel.DataName0 : null; } }
        public string Filename { get { return BgmPropertyViewModel != null ? BgmPropertyViewModel.Filename : null; } }
        public bool DoesFileExist { get {  return BgmPropertyViewModel != null && BgmPropertyViewModel.DoesFileExist; } }
        public MusicPlayerViewModel MusicPlayer { get { return BgmPropertyViewModel?.MusicPlayer; } }

        public BgmDbRootEntryViewModel(IAudioStateViewModelManager audioStateManager, IMapper mapper, BgmDbRootEntry bgmDbRootEntry)
            : base(audioStateManager, mapper, bgmDbRootEntry)
        {
            UiBgmId = bgmDbRootEntry.UiBgmId;
        }

        public void StopPlay()
        {
            if (MusicPlayer != null)
            {
                MusicPlayer.StopSong();
            }
        }

        public void LoadLocalized(string locale)
        {
            _currentLocale = locale;

            if (string.IsNullOrEmpty(locale))
                return;

            if (MSBTTitle != null && MSBTTitle.ContainsKey(locale))
                Title = MSBTTitle[locale];
            else
                Title = UiBgmId;

            if (MSBTCopyright != null && MSBTCopyright.ContainsKey(locale))
                Copyright = MSBTCopyright[locale];
            else
                Copyright = string.Empty;

            if (MSBTAuthor != null && MSBTAuthor.ContainsKey(locale))
                Author = MSBTAuthor[locale];
            else
                Author = string.Empty;
        }

        public override BgmBaseViewModel<BgmDbRootEntry> GetCopy()
        {
            return _mapper.Map(this, new BgmDbRootEntryViewModel(_audioStateManager, _mapper, new BgmDbRootEntry(UiBgmId, MusicMod)));
        }

        public override BgmBaseViewModel<BgmDbRootEntry> SaveChanges()
        {
            var original = _audioStateManager.GetBgmDbRootViewModel(UiBgmId);
            _mapper.Map(this, original.GetReferenceEntity());
            _mapper.Map(this, original);
            LoadLocalized(_currentLocale);
            return original;
        }
    }
}
