using AutoMapper;
using Avalonia.Controls;
using DynamicData;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using Sma5h.Mods.Music;
using Sma5h.Mods.Music.Helpers;
using Sma5h.Mods.Music.Interfaces;
using Sma5hMusic.GUI.Helpers;
using Sma5hMusic.GUI.Interfaces;
using Sma5hMusic.GUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sma5hMusic.GUI.ViewModels
{
    public class BgmPropertiesModalWindowViewModel : ModalBaseViewModel<BgmEntryViewModel>
    {
        private readonly IOptions<ApplicationSettings> _config;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IFileDialog _fileDialog;
        private readonly IGUIStateManager _guiStateManager;
        private readonly List<ComboItem> _recordTypes;
        private readonly List<ComboItem> _specialCategories;
        private readonly ReadOnlyObservableCollection<SeriesEntryViewModel> _series;
        private readonly ReadOnlyObservableCollection<GameTitleEntryViewModel> _games;
        private readonly ReadOnlyObservableCollection<string> _assignedInfoIds;
        private readonly Subject<Window> _whenNewRequestToAddGameEntry;
        private bool _isUpdatingSpecialRule = false;
        private const float _minimumVolume = -20.0f;
        private const float _maximumVolume = 20.0f;
        private string _originalFilename;

        public IObservable<Window> WhenNewRequestToAddGameEntry { get { return _whenNewRequestToAddGameEntry; } }
        public GamePropertiesModalWindowViewModel VMGamePropertiesModal { get; set; }

        public BgmDbRootEntryViewModel DbRootViewModel { get; private set; }
        public BgmStreamSetEntryViewModel StreamSetViewModel { get; private set; }
        public BgmAssignedInfoEntryViewModel AssignedInfoViewModel { get; private set; }
        public BgmStreamPropertyEntryViewModel StreamPropertyViewModel { get; private set; }
        public BgmPropertyEntryViewModel BgmPropertyViewModel { get; private set; }

        public MSBTFieldViewModel MSBTTitleEditor { get; set; }
        public MSBTFieldViewModel MSBTAuthorEditor { get; set; }
        public MSBTFieldViewModel MSBTCopyrightEditor { get; set; }
        public IEnumerable<ComboItem> RecordTypes { get { return _recordTypes; } }
        public IEnumerable<ComboItem> SpecialCategories { get { return _specialCategories; } }
        [Reactive]
        public ComboItem SelectedRecordType { get; set; }
        [Reactive]
        public GameTitleEntryViewModel SelectedGameTitleViewModel { get; set; }
        [Reactive]
        public ComboItem SelectedSpecialCategory { get; set; }
        [Reactive]
        public bool IsSpecialCategoryPinch { get; set; }
        [Reactive]
        public bool IsInSoundTest { get; set; }

        public bool IsModSong { get; set; }

        public ReadOnlyObservableCollection<SeriesEntryViewModel> Series { get { return _series; } }
        public ReadOnlyObservableCollection<GameTitleEntryViewModel> Games { get { return _games; } }
        public ReadOnlyObservableCollection<string> AssignedInfoIds { get { return _assignedInfoIds; } }

        public ReactiveCommand<Window, Unit> ActionNewGame { get; }
        public ReactiveCommand<BgmPropertyEntryViewModel, Unit> ActionChangeFile { get; }
        public ReactiveCommand<BgmPropertyEntryViewModel, Unit> ActionCalculateLoopCues { get; }

        public BgmPropertiesModalWindowViewModel(IOptions<ApplicationSettings> config, ILogger<BgmPropertiesModalWindowViewModel> logger, IFileDialog fileDialog,
            IMapper mapper, IGUIStateManager guiStateManager, IObservable<IChangeSet<SeriesEntryViewModel, string>> observableSeries, 
            IObservable<IChangeSet<GameTitleEntryViewModel, string>> observableGames, IObservable<IChangeSet<BgmAssignedInfoEntryViewModel, string>> observableBgmAssignedInfoEntries)
        {
            _config = config;
            _logger = logger;
            _mapper = mapper;
            _guiStateManager = guiStateManager;
            _fileDialog = fileDialog;
            _recordTypes = GetRecordTypes();
            _specialCategories = GetSpecialCategories();
            _whenNewRequestToAddGameEntry = new Subject<Window>();

            //Bind observables
            observableSeries
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _series)
               .DisposeMany()
               .Subscribe();
            observableGames
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _games)
               .DisposeMany()
               .Subscribe();
            observableBgmAssignedInfoEntries
               .Transform(p => p.InfoId)
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _assignedInfoIds)
               .DisposeMany()
               .Subscribe();

            //Set up MSBT Fields
            var defaultLocale = _config.Value.Sma5hMusicGUI.DefaultGUILocale;
            var defaultLocaleItem = new ComboItem(defaultLocale, Constants.GetLocaleDisplayName(defaultLocale));
            MSBTTitleEditor = new MSBTFieldViewModel()
            {
                SelectedLocale = defaultLocaleItem
            };
            MSBTAuthorEditor = new MSBTFieldViewModel()
            {
                SelectedLocale = defaultLocaleItem
            };
            MSBTCopyrightEditor = new MSBTFieldViewModel()
            {
                SelectedLocale = defaultLocaleItem,
                AcceptsReturn = true
            };

            //Set up subscriber on special category
            this.WhenAnyValue(p => p.SelectedSpecialCategory).Subscribe(o => SetSpecialCategoryRules(o?.Id));
            this.WhenAnyValue(p => p.SelectedItem.StreamSetViewModel.SpecialCategory).Subscribe(o => SetSpecialCategoryRules(o));
            this.WhenAnyValue(p => p.SelectedGameTitleViewModel).Subscribe((o) => SetGameTitleId(o));

            //Validation
            this.ValidationRule(p => p.SelectedGameTitleViewModel,
                p => p != null && !string.IsNullOrWhiteSpace(p.UiGameTitleId),
                "Please select a game.");

            ActionNewGame = ReactiveCommand.Create<Window>(AddNewGame);
            ActionChangeFile = ReactiveCommand.CreateFromTask<BgmPropertyEntryViewModel>(ChangeFile);
            ActionCalculateLoopCues = ReactiveCommand.CreateFromTask<BgmPropertyEntryViewModel>(CalculateAudioCues);
        }

        private void AddNewGame(Window window)
        {
            _logger.LogDebug("Clicked Add New Game");
            _whenNewRequestToAddGameEntry.OnNext(window);
        }

        private async Task ChangeFile(BgmPropertyEntryViewModel bgmPropertyEntryViewModel)
        {
            _logger.LogDebug("Clicked Change File");
            var filename = await _fileDialog.OpenFileDialogAudioSingle();
            if (!string.IsNullOrEmpty(filename))
            {
                var oldFile = BgmPropertyViewModel.Filename;
                BgmPropertyViewModel.Filename = filename;
                if (await CalculateAudioCues(bgmPropertyEntryViewModel))
                {
                    await BgmPropertyViewModel.MusicPlayer?.ChangeFilename(filename);
                }
                else
                {
                    BgmPropertyViewModel.Filename = oldFile;
                }
            }
        }

        private List<ComboItem> GetRecordTypes()
        {
            var recordTypes = new List<ComboItem>();
            recordTypes.AddRange(Constants.CONVERTER_RECORD_TYPE.Select(p => new ComboItem(p.Key, p.Value)));
            return recordTypes;
        }

        private List<ComboItem> GetSpecialCategories()
        {
            var recordTypes = new List<ComboItem>() { new ComboItem(string.Empty, "None/Other") };
            recordTypes.AddRange(Constants.SpecialCategories.UI_SPECIAL_CATEGORY.Select(p => new ComboItem(p.Key, p.Value)));
            return recordTypes;
        }

        private void SetSpecialCategoryRules(string specialRule)
        {
            if (!_isUpdatingSpecialRule)
            {
                _isUpdatingSpecialRule = true;
                IsSpecialCategoryPinch = false;

                if (_refSelectedItem != null)
                {
                    SelectedSpecialCategory = _specialCategories.FirstOrDefault(p => p.Id == specialRule);
                    if (SelectedSpecialCategory == null)
                        SelectedSpecialCategory = _specialCategories[0];
                    _refSelectedItem.StreamSetViewModel.SpecialCategory = specialRule;

                    switch (specialRule)
                    {
                        case Constants.SpecialCategories.SPECIAL_CATEGORY_PINCH_VALUE:
                            IsSpecialCategoryPinch = true;
                            break;
                    }
                }
                _isUpdatingSpecialRule = false;
            }
        }

        private void SetGameTitleId(GameTitleEntryViewModel gameTitle)
        {
            if (DbRootViewModel != null)
            {
                if (gameTitle != null)
                    DbRootViewModel.UiGameTitleId = gameTitle.UiGameTitleId;
                else
                    DbRootViewModel.UiGameTitleId = MusicConstants.InternalIds.GAME_TITLE_ID_DEFAULT;
            }
        }

        protected override Task CancelChanges()
        {
            BgmPropertyViewModel?.MusicPlayer?.ChangeFilename(_originalFilename);
            return base.CancelChanges();
        }

        protected override Task<bool> SaveChanges()
        {
            _logger.LogDebug("Save Changes");
            if (BgmPropertyViewModel.AudioVolume < _minimumVolume)
                BgmPropertyViewModel.AudioVolume = _minimumVolume;
            if (BgmPropertyViewModel.AudioVolume > _maximumVolume)
                BgmPropertyViewModel.AudioVolume = _maximumVolume;

            DbRootViewModel.TestDispOrder = (short)(IsInSoundTest ? DbRootViewModel.TestDispOrder > -1 ? DbRootViewModel.TestDispOrder : short.MaxValue : -1);
            if (SelectedRecordType != null)
                DbRootViewModel.RecordType = SelectedRecordType.Id;
            MSBTTitleEditor.SaveValueToRecent();
            MSBTAuthorEditor.SaveValueToRecent();
            MSBTCopyrightEditor.SaveValueToRecent();
            DbRootViewModel.MSBTTitle = SaveMSBTValues(MSBTTitleEditor.MSBTValues);
            DbRootViewModel.MSBTAuthor = SaveMSBTValues(MSBTAuthorEditor.MSBTValues);
            DbRootViewModel.MSBTCopyright = SaveMSBTValues(MSBTCopyrightEditor.MSBTValues);
            return Task.FromResult(true);
        }

        private Dictionary<string, string> SaveMSBTValues(Dictionary<string, string> msbtValues)
        {
            var output = new Dictionary<string, string>();
            if (msbtValues != null)
            {
                foreach (var msbtValue in msbtValues)
                {
                    if (!string.IsNullOrEmpty(msbtValue.Value))
                        output.Add(msbtValue.Key, msbtValue.Value);
                    else if (_config.Value.Sma5hMusicGUI.CopyToEmptyLocales && msbtValues.ContainsKey(_config.Value.Sma5hMusicGUI.DefaultMSBTLocale))
                        output.Add(msbtValue.Key, msbtValues[_config.Value.Sma5hMusicGUI.DefaultMSBTLocale]);
                }
            }
            return output;
        }

        protected override void LoadItem(BgmEntryViewModel item)
        {
            _logger.LogDebug("Load Item");
            DbRootViewModel = item?.DbRootViewModel;
            StreamSetViewModel = item?.StreamSetViewModel;
            AssignedInfoViewModel = item?.AssignedInfoViewModel;
            StreamPropertyViewModel = item?.StreamPropertyViewModel;
            BgmPropertyViewModel = item?.BgmPropertyViewModel;
            _originalFilename = BgmPropertyViewModel?.Filename;

            IsModSong = item.MusicMod != null;

            MSBTTitleEditor.MSBTValues = DbRootViewModel.MSBTTitle;
            MSBTAuthorEditor.MSBTValues = DbRootViewModel.MSBTAuthor;
            MSBTCopyrightEditor.MSBTValues = DbRootViewModel.MSBTCopyright;
            IsInSoundTest = DbRootViewModel.TestDispOrder > -1;
            SelectedRecordType = _recordTypes.FirstOrDefault(p => p.Id == DbRootViewModel.RecordType);
            SelectedGameTitleViewModel = _games.FirstOrDefault(p => p.UiGameTitleId == DbRootViewModel.UiGameTitleId);
            SetSpecialCategoryRules(StreamSetViewModel.SpecialCategory);
        }

        private async Task<bool> CalculateAudioCues(BgmPropertyEntryViewModel bgmPropertyEntryViewModel)
        {
            var audioCuePoints = await _guiStateManager.UpdateAudioCuePoints(bgmPropertyEntryViewModel.Filename);
            if (audioCuePoints != null)
            {
                _mapper.Map(audioCuePoints, bgmPropertyEntryViewModel);
                return true;
            }
            return false;
        }
    }
}
