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
using Sm5sh.Mods.Music.Models;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmPropertiesModalWindowViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly List<ComboItem> _recordTypes;
        private readonly ReadOnlyObservableCollection<LocaleViewModel> _locales;
        private readonly ReadOnlyObservableCollection<SeriesEntryViewModel> _series;
        private readonly ReadOnlyObservableCollection<GameTitleEntryViewModel> _games;

        [Reactive]
        public BgmEntryEditableViewModel SelectedBgmEntry { get; set; }

        public MSBTFieldViewModel MSBTTitleEditor { get; set; }
        public MSBTFieldViewModel MSBTAuthorEditor { get; set; }
        public MSBTFieldViewModel MSBTCopyrightEditor { get; set; }

        public IEnumerable<ComboItem> RecordTypes { get { return _recordTypes; } }

        public ReadOnlyObservableCollection<SeriesEntryViewModel> Series { get { return _series; } }
        public ReadOnlyObservableCollection<GameTitleEntryViewModel> Games { get { return _games; } }
        public ReadOnlyObservableCollection<LocaleViewModel> Locales { get { return _locales; } }

        public BgmPropertiesModalWindowViewModel(ILogger<BgmPropertiesModalWindowViewModel> logger, IMapper mapper, IObservable<IChangeSet<LocaleViewModel, string>> observableLocales,
            IObservable<IChangeSet<SeriesEntryViewModel, string>> observableSeries, IObservable<IChangeSet<GameTitleEntryViewModel, string>> observableGames)
        {
            _logger = logger;
            _mapper = mapper;
            _recordTypes = GetRecordTypes();

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

            SelectedBgmEntry = new BgmEntryEditableViewModel();
        }

        private List<ComboItem> GetRecordTypes()
        {
            var recordTypes = new List<ComboItem>() { new ComboItem("All", "All", true) };
            recordTypes.AddRange(Constants.CONVERTER_RECORD_TYPE.Select(p => new ComboItem(p.Key, p.Value)));
            return recordTypes;
        }

        public void LoadBgmEntry(BgmEntry bgmEntry)
        {
            SelectedBgmEntry = _mapper.Map(bgmEntry, new BgmEntryEditableViewModel(bgmEntry));
        }
    }
}
