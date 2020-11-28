using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using Sm5sh.GUI.Models;
using System.Collections.Generic;
using Sm5sh.GUI.Helpers;
using System.Linq;
using System;
using DynamicData;
using System.Reactive.Linq;
using ReactiveUI;
using AutoMapper;
using VGMMusic;
using System.Reactive;
using Avalonia.Controls;
using Sm5sh.Mods.Music.Models;
using System.Threading.Tasks;
using Sm5sh.GUI.Views;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmPropertiesModalWindowViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IVGMMusicPlayer _musicPlayer;
        private readonly List<ComboItem> _recordTypes;
        private readonly List<ComboItem> _specialCategories;
        private readonly List<ComboItem> _personaStages;
        private readonly ReadOnlyObservableCollection<LocaleViewModel> _locales;
        private readonly ReadOnlyObservableCollection<SeriesEntryViewModel> _series;
        private readonly ReadOnlyObservableCollection<GameTitleEntryViewModel> _games;
        private readonly ReadOnlyObservableCollection<string> _songs;
        private BgmEntryViewModel _fakeBgm = new BgmEntryViewModel(null, new Mods.Music.Models.BgmEntry("fake"));
        private BgmEntryViewModel _refSavedBgmEntryView;

        public GamePropertiesModalWindowViewModel VMGamePropertiesModal { get; set; }

        [Reactive]
        public BgmEntryViewModel SelectedBgmEntry { get; private set; }

        public MSBTFieldViewModel MSBTTitleEditor { get; set; }
        public MSBTFieldViewModel MSBTAuthorEditor { get; set; }
        public MSBTFieldViewModel MSBTCopyrightEditor { get; set; }

        public IEnumerable<ComboItem> RecordTypes { get { return _recordTypes; } }
        public IEnumerable<ComboItem> SpecialCategories { get { return _specialCategories; } }
        public IEnumerable<ComboItem> SpecialCategoriesPersonaStages { get { return _personaStages; } }
        [Reactive]
        public ComboItem SelectedRecordType { get; set; }

        [Reactive]
        public ComboItem SelectedSpecialCategory { get; set; }
        [Reactive]
        public bool IsSpecialCategoryPinch { get; set; }
        [Reactive]
        public bool IsSpecialCategoryPersona { get; set; }
        [Reactive]
        public bool IsInSoundTest { get; set; }

        public ReadOnlyObservableCollection<SeriesEntryViewModel> Series { get { return _series; } }
        public ReadOnlyObservableCollection<GameTitleEntryViewModel> Games { get { return _games; } }
        public ReadOnlyObservableCollection<string> Bgms { get { return _songs; } }
        public ReadOnlyObservableCollection<LocaleViewModel> Locales { get { return _locales; } }

        public ReactiveCommand<Window, Unit> ActionCancel { get; }
        public ReactiveCommand<Window, Unit> ActionSave { get; }
        public ReactiveCommand<Window, Unit> ActionNewGame { get; }
        public ReactiveCommand<Window, Unit> ActionEditGame { get; }

        public BgmPropertiesModalWindowViewModel(ILogger<BgmPropertiesModalWindowViewModel> logger, IVGMMusicPlayer musicPlayer, IMapper mapper, IObservable<IChangeSet<LocaleViewModel, string>> observableLocales,
            IObservable<IChangeSet<SeriesEntryViewModel, string>> observableSeries, IObservable<IChangeSet<GameTitleEntryViewModel, string>> observableGames,
            IObservable<IChangeSet<BgmEntryViewModel, string>> observableBgms)
        {
            _logger = logger;
            _mapper = mapper;
            _musicPlayer = musicPlayer;
            _recordTypes = GetRecordTypes();
            _specialCategories = GetSpecialCategories();
            _personaStages = GetSpecialCategoriesPersonaStages();

            ActionCancel = ReactiveCommand.Create<Window>(CancelChanges);
            ActionSave = ReactiveCommand.Create<Window>(SaveChanges);
            ActionNewGame = ReactiveCommand.CreateFromTask<Window>(AddNewGame);
            ActionEditGame = ReactiveCommand.Create<Window>(SaveChanges);

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
            observableBgms
               .Transform(p => $"info_{p.ToneId}")
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _songs)
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

            SelectedBgmEntry = _fakeBgm;
        }

        private void CancelChanges(Window w)
        {
            SelectedBgmEntry = _fakeBgm;
            w.Close();
        }

        private void SaveChanges(Window window)
        {
            var previousTestOrder = _refSavedBgmEntryView.DbRoot.TestDispOrder;
            _refSavedBgmEntryView = _mapper.Map(SelectedBgmEntry, _refSavedBgmEntryView);
            _refSavedBgmEntryView.DbRoot.TestDispOrder = (short)(IsInSoundTest ? previousTestOrder > -1 ? previousTestOrder : 0 : -1);
            _refSavedBgmEntryView.DbRoot.RecordType = SelectedRecordType.Id;
            _refSavedBgmEntryView.MSBTLabels.Title = MSBTTitleEditor.MSBTValues;
            _refSavedBgmEntryView.MSBTLabels.Author = MSBTAuthorEditor.MSBTValues;
            _refSavedBgmEntryView.MSBTLabels.Copyright = MSBTCopyrightEditor.MSBTValues;

            window.Close(window);
        }
        
        private async Task AddNewGame(Window window)
        {
            VMGamePropertiesModal.LoadGame(null);
            var modalCreateMod = new GamePropertiesModalWindow() { DataContext = VMGamePropertiesModal };
            var results = await modalCreateMod.ShowDialog<GamePropertiesModalWindow>(window);

            if(results != null)
            {

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
            var recordTypes = new List<ComboItem>() { new ComboItem(string.Empty, "None") };
            recordTypes.AddRange(Constants.SpecialCategories.UI_SPECIAL_CATEGORY.Select(p => new ComboItem(p.Key, p.Value)));
            return recordTypes;
        }

        private List<ComboItem> GetSpecialCategoriesPersonaStages()
        {
            var recordTypes = new List<ComboItem>() { new ComboItem(string.Empty, "None") };
            recordTypes.AddRange(Constants.SpecialCategories.CONVERTER_SPECIAL_CATEGORY_PERSONA.Select(p => new ComboItem(p.Key, p.Value)));
            return recordTypes;
        }

        public void LoadBgmEntry(BgmEntryViewModel vmBgmEntry)
        {
            _refSavedBgmEntryView = vmBgmEntry;

            //TODO: Cleanup
            //Manually setting fields to breaks references
            SelectedBgmEntry = _mapper.Map(vmBgmEntry, new BgmEntryViewModel(_musicPlayer, new BgmEntry(vmBgmEntry.ToneId, vmBgmEntry.MusicMod, vmBgmEntry.Filename)) { GameTitleViewModel = vmBgmEntry.GameTitleViewModel});
            MSBTTitleEditor.MSBTValues = SelectedBgmEntry.MSBTLabels.Title;
            MSBTAuthorEditor.MSBTValues = SelectedBgmEntry.MSBTLabels.Author;
            MSBTCopyrightEditor.MSBTValues = SelectedBgmEntry.MSBTLabels.Copyright;
            IsInSoundTest = SelectedBgmEntry.DbRoot.TestDispOrder > -1;
            SelectedRecordType = _recordTypes.FirstOrDefault(p => p.Id == vmBgmEntry.RecordType);
            SetSpecialCategory();
        }

        private void SetSpecialCategory()
        {
            var rule = string.Empty;

            if (SelectedBgmEntry.StreamSet.SpecialCategory == Constants.SpecialCategories.SPECIAL_CATEGORY_PINCH_VALUE)
                rule = Constants.SpecialCategories.SPECIAL_CATEGORY_PINCH;
            else if (SelectedBgmEntry.StreamSet.Info1 != null && Constants.SpecialCategories.CONVERTER_SPECIAL_CATEGORY_PERSONA.ContainsKey(SelectedBgmEntry.StreamSet.Info1))
                rule = Constants.SpecialCategories.SPECIAL_CATEGORY_PERSONA;

            SelectedSpecialCategory = _specialCategories.FirstOrDefault(p => p.Id == rule);
            SetSpecialCategoryRules(rule);
        }

        private void SetSpecialCategoryRules(string specialRule)
        {
            IsSpecialCategoryPinch = false;
            IsSpecialCategoryPersona = false;

            switch (specialRule)
            {
                case Constants.SpecialCategories.SPECIAL_CATEGORY_PINCH:
                    IsSpecialCategoryPinch = true;
                    if(SelectedBgmEntry != null)
                        SelectedBgmEntry.StreamSet.SpecialCategory = Constants.SpecialCategories.SPECIAL_CATEGORY_PINCH_VALUE;
                    break;
                case Constants.SpecialCategories.SPECIAL_CATEGORY_PERSONA:
                    IsSpecialCategoryPersona = true;
                    break;
            }
        }
    }
}
