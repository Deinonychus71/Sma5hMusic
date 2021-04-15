using AutoMapper;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sma5h.Mods.Music.Models;
using Sma5hMusic.GUI.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sma5hMusic.GUI.ViewModels
{
    public class BgmDbRootEntryViewModel : BgmBaseViewModel<BgmDbRootEntry>
    {
        private string _currentLocale;
        private string _uiGameTitleId;

        //Getters/Setters
        public string UiBgmId { get; }
        public string StreamSetId { get; set; }
        public string Rarity { get; set; }
        [Reactive]
        public string RecordType { get; set; }
        public string UiGameTitleId
        {
            get => _uiGameTitleId;
            set
            {
                this.RaiseAndSetIfChanged(ref _uiGameTitleId, value);
                this.RaisePropertyChanged(nameof(GameTitleViewModel));
                this.RaisePropertyChanged(nameof(SeriesViewModel));
                this.RaisePropertyChanged(nameof(SeriesId));
            }
        }
        public string UiGameTitleId1 { get; set; }
        public string UiGameTitleId2 { get; set; }
        public string UiGameTitleId3 { get; set; }
        public string UiGameTitleId4 { get; set; }
        public string NameId { get; set; }
        public short SaveNo { get; set; }
        [Reactive]
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
        public bool IsSelectableMovieEdit { get; set; }
        public bool IsSelectableOriginal { get; set; }
        public bool IsDlc { get; set; }
        public bool IsPatch { get; set; }
        public string DlcUiCharaId { get; set; }
        public string DlcMiiHatMotifId { get; set; }
        public string DlcMiiBodyMotifId { get; set; }
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
        public BgmStreamSetEntryViewModel StreamSetViewModel { get { return _viewModelManager.GetBgmStreamSetViewModel(StreamSetId); } }
        public GameTitleEntryViewModel GameTitleViewModel { get { return _viewModelManager.GetGameTitleViewModel(UiGameTitleId); } }
        public GameTitleEntryViewModel GameTitle1ViewModel { get { return _viewModelManager.GetGameTitleViewModel(UiGameTitleId1); } }
        public GameTitleEntryViewModel GameTitle2ViewModel { get { return _viewModelManager.GetGameTitleViewModel(UiGameTitleId2); } }
        public GameTitleEntryViewModel GameTitle3ViewModel { get { return _viewModelManager.GetGameTitleViewModel(UiGameTitleId3); } }
        public GameTitleEntryViewModel GameTitle4ViewModel { get { return _viewModelManager.GetGameTitleViewModel(UiGameTitleId4); } }

        public bool HiddenInSoundTest { get { return TestDispOrder == -1; } }
        public SeriesEntryViewModel SeriesViewModel { get { return GameTitleViewModel?.SeriesViewModel; } }
        public BgmAssignedInfoEntryViewModel AssignedInfoViewModel { get { return StreamSetViewModel?.Info0ViewModel; } }
        public BgmStreamPropertyEntryViewModel StreamPropertyViewModel { get { return AssignedInfoViewModel?.StreamPropertyViewModel; } }
        public BgmPropertyEntryViewModel BgmPropertyViewModel { get { return StreamPropertyViewModel?.DataName0ViewModel; } }
        public string SeriesId { get { return GameTitleViewModel?.UiSeriesId; } }
        public string ToneId { get { return StreamPropertyViewModel?.DataName0 != null ? StreamPropertyViewModel.DataName0 : null; } }
        public string Filename { get { return BgmPropertyViewModel?.Filename; } }
        public bool DoesFileExist { get { return BgmPropertyViewModel != null && BgmPropertyViewModel.DoesFileExist; } }
        public MusicPlayerViewModel MusicPlayer { get { return BgmPropertyViewModel?.MusicPlayer; } }

        public BgmDbRootEntryViewModel(IViewModelManager viewModelManager, IMapper mapper, BgmDbRootEntry bgmDbRootEntry)
            : base(viewModelManager, mapper, bgmDbRootEntry)
        {
            UiBgmId = bgmDbRootEntry?.UiBgmId;
        }

        public async Task StopPlay()
        {
            if (MusicPlayer != null)
            {
                await MusicPlayer.StopSong();
            }
        }

        public void LoadLocalized(string locale = null)
        {
            if (!string.IsNullOrEmpty(locale))
                _currentLocale = locale;

            if (string.IsNullOrEmpty(_currentLocale))
                return;

            if (MSBTTitle != null && MSBTTitle.ContainsKey(_currentLocale))
                Title = MSBTTitle[_currentLocale];
            else
                Title = UiBgmId;

            if (MSBTCopyright != null && MSBTCopyright.ContainsKey(_currentLocale))
                Copyright = MSBTCopyright[_currentLocale];
            else
                Copyright = string.Empty;

            if (MSBTAuthor != null && MSBTAuthor.ContainsKey(_currentLocale))
                Author = MSBTAuthor[_currentLocale];
            else
                Author = string.Empty;
        }

        public override ReactiveObjectBaseViewModel GetCopy()
        {
            return _mapper.Map(this, new BgmDbRootEntryViewModel(_viewModelManager, _mapper, GetReferenceEntity()));
        }

        public override ReactiveObjectBaseViewModel SaveChanges()
        {
            var original = _viewModelManager.GetBgmDbRootViewModel(UiBgmId);
            _mapper.Map(this, original.GetReferenceEntity());
            _mapper.Map(this, original);
            original.LoadLocalized(_currentLocale);
            return original;
        }
    }
}
