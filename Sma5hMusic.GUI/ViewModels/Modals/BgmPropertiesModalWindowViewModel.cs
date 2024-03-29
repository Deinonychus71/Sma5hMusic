﻿using AutoMapper;
using Avalonia.Controls;
using Avalonia.Threading;
using DynamicData;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using Sma5h.Mods.Music;
using Sma5h.Mods.Music.Helpers;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sma5hMusic.GUI.ViewModels
{
    public class BgmPropertiesModalWindowViewModel : ModalBaseViewModel<BgmEntryViewModel>
    {
        private readonly IOptionsMonitor<ApplicationSettings> _config;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IFileDialog _fileDialog;
        private readonly IGUIStateManager _guiStateManager;
        private readonly List<GameTitleEntryViewModel> _recentGameTitles;
        private readonly List<ComboItem> _recordTypes;
        private readonly List<ComboItem> _specialCategories;
        private readonly ReadOnlyObservableCollection<SeriesEntryViewModel> _series;
        private readonly ReadOnlyObservableCollection<GameTitleEntryViewModel> _games;
        private readonly ReadOnlyObservableCollection<string> _assignedInfoIds;
        private readonly Subject<Window> _whenNewRequestToAddGameEntry;
        private bool _isUpdatingSpecialRule = false;
        private string _originalGameTitleId;
        private string _originalFilename;

        public IEnumerable<GameTitleEntryViewModel> RecentGameTitles { get { return _recentGameTitles; } }
        [Reactive]
        public bool DisplayRecents { get; set; }
        [Reactive]
        public GameTitleEntryViewModel SelectedRecentAction { get; set; }

        public IObservable<Window> WhenNewRequestToAddGameEntry { get { return _whenNewRequestToAddGameEntry; } }
        public GamePropertiesModalWindowViewModel VMGamePropertiesModal { get; set; }

        public BgmDbRootEntryViewModel DbRootViewModel { get; private set; }
        public BgmStreamSetEntryViewModel StreamSetViewModel { get; private set; }
        public BgmAssignedInfoEntryViewModel AssignedInfoViewModel { get; private set; }
        [Reactive]
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
        public ReactiveCommand<Window, Unit> ActionClosing { get; }

        public BgmPropertiesModalWindowViewModel(IOptionsMonitor<ApplicationSettings> config, ILogger<BgmPropertiesModalWindowViewModel> logger, IFileDialog fileDialog,
            IMapper mapper, IGUIStateManager guiStateManager, IViewModelManager viewModelManager)
        {
            _config = config;
            _logger = logger;
            _mapper = mapper;
            _guiStateManager = guiStateManager;
            _fileDialog = fileDialog;
            _recordTypes = GetRecordTypes();
            _specialCategories = GetSpecialCategories();
            _whenNewRequestToAddGameEntry = new Subject<Window>();
            _recentGameTitles = new List<GameTitleEntryViewModel>();

            //Bind observables
            viewModelManager.ObservableSeries.Connect()
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _series)
               .DisposeMany()
               .Subscribe();
            viewModelManager.ObservableGameTitles.Connect()
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _games)
               .DisposeMany()
               .Subscribe();
            viewModelManager.ObservableAssignedInfoEntries.Connect()
               .Transform(p => p.InfoId)
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _assignedInfoIds)
               .DisposeMany()
               .Subscribe();

            //Set up MSBT Fields
            var defaultLocale = _config.CurrentValue.Sma5hMusicGUI.DefaultGUILocale;
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
            this.WhenAnyValue(p => p.SelectedRecentAction).Subscribe(o => HandleRecentAction(o));

            //Validation
            this.ValidationRule(p => p.SelectedGameTitleViewModel,
                p => p != null && !string.IsNullOrWhiteSpace(p.UiGameTitleId),
                "Please select a game.");
            this.ValidationRule(p => p.StreamPropertyViewModel.StartPoint0,
                p => ValidateStreamPropertyTime(p), "This value must be of the format '00:00:00.000'");
            this.ValidationRule(p => p.StreamPropertyViewModel.StartPoint1,
                p => ValidateStreamPropertyTime(p), "This value must be of the format '00:00:00.000'");
            this.ValidationRule(p => p.StreamPropertyViewModel.StartPoint2,
                p => ValidateStreamPropertyTime(p), "This value must be of the format '00:00:00.000'");
            this.ValidationRule(p => p.StreamPropertyViewModel.StartPoint3,
                p => ValidateStreamPropertyTime(p), "This value must be of the format '00:00:00.000'");
            this.ValidationRule(p => p.StreamPropertyViewModel.StartPoint4,
                p => ValidateStreamPropertyTime(p), "This value must be of the format '00:00:00.000'");
            this.ValidationRule(p => p.StreamPropertyViewModel.EndPoint,
                p => ValidateStreamPropertyTime(p), "This value must be of the format '00:00:00.000'");
            this.ValidationRule(p => p.StreamPropertyViewModel.StartPointSuddenDeath,
                p => ValidateStreamPropertyTime(p), "This value must be of the format '00:00:00.000'");
            this.ValidationRule(p => p.StreamPropertyViewModel.StartPointTransition,
                p => ValidateStreamPropertyTime(p), "This value must be of the format '00:00:00.000'");

            ActionNewGame = ReactiveCommand.Create<Window>(AddNewGame);
            ActionChangeFile = ReactiveCommand.CreateFromTask<BgmPropertyEntryViewModel>(ChangeFile);
            ActionCalculateLoopCues = ReactiveCommand.CreateFromTask<BgmPropertyEntryViewModel>(CalculateAudioCues);
            ActionClosing = ReactiveCommand.Create<Window>(ClosingWindow);
        }

        private bool ValidateStreamPropertyTime(string value)
        {
            return string.IsNullOrEmpty(value) || Regex.IsMatch(value, @"^\d{2}:\d{2}:\d{2}.\d{3}$", RegexOptions.Compiled);
        }

        private void HandleRecentAction(GameTitleEntryViewModel o)
        {
            if (o == null)
                return;

            SelectedGameTitleViewModel = _games.FirstOrDefault(p => p == o);

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                SelectedRecentAction = null;
            }, DispatcherPriority.Background);
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
                {
                    DbRootViewModel.UiGameTitleId = gameTitle.UiGameTitleId;
                }
                else
                    DbRootViewModel.UiGameTitleId = MusicConstants.InternalIds.GAME_TITLE_ID_DEFAULT;
            }
        }

        private void ClosingWindow(Window w)
        {
            BgmPropertyViewModel?.MusicPlayer?.ChangeFilename(_originalFilename);
        }

        protected override Task<bool> SaveChanges()
        {
            _logger.LogDebug("Save Changes");
            if (BgmPropertyViewModel.AudioVolume < Constants.MinimumGameVolume)
                BgmPropertyViewModel.AudioVolume = Constants.MinimumGameVolume;
            if (BgmPropertyViewModel.AudioVolume > Constants.MaximumGameVolume)
                BgmPropertyViewModel.AudioVolume = Constants.MaximumGameVolume;

            _originalFilename = BgmPropertyViewModel.Filename;

            DbRootViewModel.TestDispOrder = (short)(IsInSoundTest ? DbRootViewModel.TestDispOrder > -1 ? DbRootViewModel.TestDispOrder : _guiStateManager.GetNewHighestSoundTestOrderValue() : -1);
            if (SelectedRecordType != null)
                DbRootViewModel.RecordType = SelectedRecordType.Id;
            MSBTTitleEditor.SaveValueToRecent();
            MSBTAuthorEditor.SaveValueToRecent();
            MSBTCopyrightEditor.SaveValueToRecent();
            DbRootViewModel.MSBTTitle = SaveMSBTValues(MSBTTitleEditor.MSBTValues);
            DbRootViewModel.MSBTAuthor = SaveMSBTValues(MSBTAuthorEditor.MSBTValues);
            DbRootViewModel.MSBTCopyright = SaveMSBTValues(MSBTCopyrightEditor.MSBTValues);

            if (!string.IsNullOrEmpty(SelectedGameTitleViewModel?.UiGameTitleId) && _originalGameTitleId != SelectedGameTitleViewModel.UiGameTitleId)
            {
                if (SelectedGameTitleViewModel != null && !_recentGameTitles.Contains(SelectedGameTitleViewModel))
                {
                    if (_recentGameTitles.Count > 9)
                        _recentGameTitles.RemoveAt(_recentGameTitles.Count - 1);
                    _recentGameTitles.Insert(0, SelectedGameTitleViewModel);
                }
                DisplayRecents = _recentGameTitles.Count() > 0;
            }

            return Task.FromResult(true);
        }

        private Dictionary<string, string> SaveMSBTValues(Dictionary<string, string> msbtValues)
        {
            var output = new Dictionary<string, string>();
            var copyToEmptyLocales = _config.CurrentValue.Sma5hMusicGUI.CopyToEmptyLocales;
            var defaultMSBTLocale = _config.CurrentValue.Sma5hMusicGUI.DefaultMSBTLocale;
            if (msbtValues != null)
            {
                foreach (var msbtValue in msbtValues)
                {
                    if (!string.IsNullOrEmpty(msbtValue.Value))
                        output.Add(msbtValue.Key, msbtValue.Value);
                    else if (copyToEmptyLocales && msbtValues.ContainsKey(defaultMSBTLocale))
                        output.Add(msbtValue.Key, msbtValues[defaultMSBTLocale]);
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
            _originalGameTitleId = SelectedGameTitleViewModel?.UiGameTitleId;
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
