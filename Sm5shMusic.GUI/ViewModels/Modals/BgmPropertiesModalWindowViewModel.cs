using Avalonia.Controls;
using DynamicData;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using Sm5sh.Mods.Music.Helpers;
using Sm5shMusic.GUI.Helpers;
using Sm5shMusic.GUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sm5shMusic.GUI.ViewModels
{
    public class BgmPropertiesModalWindowViewModel : ModalBaseViewModel<BgmEntryViewModel>
    {
        private readonly ILogger _logger;
        private readonly List<ComboItem> _recordTypes;
        private readonly List<ComboItem> _specialCategories;
        private readonly ReadOnlyObservableCollection<LocaleViewModel> _locales;
        private readonly ReadOnlyObservableCollection<SeriesEntryViewModel> _series;
        private readonly ReadOnlyObservableCollection<GameTitleEntryViewModel> _games;
        private readonly ReadOnlyObservableCollection<string> _streamSetIds;
        private readonly Subject<Window> _whenNewRequestToAddGameEntry;
        private bool _isUpdatingSpecialRule = false;

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

        public ReadOnlyObservableCollection<SeriesEntryViewModel> Series { get { return _series; } }
        public ReadOnlyObservableCollection<GameTitleEntryViewModel> Games { get { return _games; } }
        public ReadOnlyObservableCollection<string> StreamSetIds { get { return _streamSetIds; } }
        public ReadOnlyObservableCollection<LocaleViewModel> Locales { get { return _locales; } }

        public ReactiveCommand<Window, Unit> ActionNewGame { get; }

        public BgmPropertiesModalWindowViewModel(ILogger<BgmPropertiesModalWindowViewModel> logger, IObservable<IChangeSet<LocaleViewModel, string>> observableLocales,
            IObservable<IChangeSet<SeriesEntryViewModel, string>> observableSeries, IObservable<IChangeSet<GameTitleEntryViewModel, string>> observableGames,
            IObservable<IChangeSet<BgmStreamSetEntryViewModel, string>> observableBgmStreamSetEntries)
        {
            _logger = logger;
            _recordTypes = GetRecordTypes();
            _specialCategories = GetSpecialCategories();
            _whenNewRequestToAddGameEntry = new Subject<Window>();

            //Bind observables
            observableLocales
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _locales)
                .DisposeMany()
                .Subscribe();
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
            observableBgmStreamSetEntries
               .Transform(p => p.StreamSetId)
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _streamSetIds)
               .DisposeMany()
               .Subscribe();

            //Set up MSBT Fields
            var defaultLocale = new LocaleViewModel(Constants.DEFAULT_LOCALE, Constants.GetLocaleDisplayName(Constants.DEFAULT_LOCALE));
            MSBTTitleEditor = new MSBTFieldViewModel()
            {
                Locales = Locales,
                SelectedLocale = defaultLocale
            };
            MSBTAuthorEditor = new MSBTFieldViewModel()
            {
                Locales = Locales,
                SelectedLocale = defaultLocale
            };
            MSBTCopyrightEditor = new MSBTFieldViewModel()
            {
                Locales = Locales,
                SelectedLocale = defaultLocale,
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
        }

        private void AddNewGame(Window window)
        {
            _logger.LogDebug("Clicked Add New Game");
            _whenNewRequestToAddGameEntry.OnNext(window);
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

        protected override Task SaveChanges()
        {
            _logger.LogDebug("Save Changes");
            DbRootViewModel.TestDispOrder = (short)(IsInSoundTest ? DbRootViewModel.TestDispOrder > -1 ? DbRootViewModel.TestDispOrder : short.MaxValue : -1);
            if (SelectedRecordType != null)
                DbRootViewModel.RecordType = SelectedRecordType.Id;
            DbRootViewModel.MSBTTitle = MSBTTitleEditor.MSBTValues;
            DbRootViewModel.MSBTAuthor = MSBTAuthorEditor.MSBTValues;
            DbRootViewModel.MSBTCopyright = MSBTCopyrightEditor.MSBTValues;
            return Task.CompletedTask;
        }

        protected override void LoadItem(BgmEntryViewModel item)
        {
            _logger.LogDebug("Load Item");
            DbRootViewModel = item?.DbRootViewModel;
            StreamSetViewModel = item?.StreamSetViewModel;
            AssignedInfoViewModel = item?.AssignedInfoViewModel;
            StreamPropertyViewModel = item?.StreamPropertyViewModel;
            BgmPropertyViewModel = item?.BgmPropertyViewModel;

            MSBTTitleEditor.MSBTValues = DbRootViewModel.MSBTTitle;
            MSBTAuthorEditor.MSBTValues = DbRootViewModel.MSBTAuthor;
            MSBTCopyrightEditor.MSBTValues = DbRootViewModel.MSBTCopyright;
            IsInSoundTest = DbRootViewModel.TestDispOrder > -1;
            SelectedRecordType = _recordTypes.FirstOrDefault(p => p.Id == DbRootViewModel.RecordType);
            SelectedGameTitleViewModel = _games.FirstOrDefault(p => p.UiGameTitleId == DbRootViewModel.UiGameTitleId);
            SetSpecialCategoryRules(StreamSetViewModel.SpecialCategory);
        }
    }
}
