using DynamicData;
using Microsoft.Extensions.Logging;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using Sm5sh.GUI.Helpers;
using System.Collections.Generic;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using System;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Controls;
using Sm5sh.GUI.Models;
using DynamicData.Binding;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Linq;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmSongsViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private readonly RangeObservableCollection<string> _locales = new RangeObservableCollection<string>(new List<string>() { "us_en", "jp_ja" });
        private readonly ReadOnlyObservableCollection<MenuImportSongViewModel> _mods;

        public BgmListViewModel VMBgmList { get; }
        public BgmFiltersViewModel VMBgmFilters { get; }
        public BgmPropertiesViewModel VMBgmProperties { get; }
        public ReadOnlyObservableCollection<MenuImportSongViewModel> Mods { get { return _mods; } }
        [Reactive]
        public string SelectedLocale { get; set; }

        public BgmSongsViewModel(IServiceProvider serviceProvider, ILogger<BgmSongsViewModel> logger, IObservable<IChangeSet<BgmEntryListViewModel, string>> observableBgmEntriesList)
        {
            _logger = logger;
            SelectedLocale = Constants.DEFAULT_LOCALE;

            var whenLocaleChanged = this.WhenAnyValue(p => p.SelectedLocale);
            var observableLocalizedBgmEntriesList = observableBgmEntriesList
                .AutoRefreshOnObservable(p => whenLocaleChanged)
                .ForEachChange(o => o.Current.LoadLocalized(SelectedLocale));

            //List of mods
            var modsChanged = observableBgmEntriesList.WhenValueChanged(species => species.ModPath);
            observableBgmEntriesList
                .Filter(p => !string.IsNullOrEmpty(p.ModPath))
                .Group(p => p.ModPath, modsChanged.Select(_ => Unit.Default))
                .Transform(p => {
                    var firstItem = p.Cache.Items.First();
                    var newMenuItem = new MenuImportSongViewModel(firstItem.ModPath, $"To {firstItem.ModName}...");
                    return newMenuItem;
                    })
                //.Prepend(_allChangeSet)
                .Sort(SortExpressionComparer<MenuImportSongViewModel>.Ascending(p => p.ModName), SortOptimisations.IgnoreEvaluates)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _mods)
                .DisposeMany()
                .Subscribe();

            //Initialize filters
            VMBgmFilters = ActivatorUtilities.CreateInstance<BgmFiltersViewModel>(serviceProvider, observableLocalizedBgmEntriesList);

            //Initialize list
            VMBgmList = ActivatorUtilities.CreateInstance<BgmListViewModel>(serviceProvider, VMBgmFilters.WhenFiltersAreApplied);

            //Initialize properties
            var whenSelectedBgmEntryChanged = this.WhenAnyValue(p => p.VMBgmList.SelectedBgmEntry);
            VMBgmProperties = ActivatorUtilities.CreateInstance<BgmPropertiesViewModel>(serviceProvider, whenSelectedBgmEntryChanged);
        }

        public void ChangeLocale(string locale)
        {
            SelectedLocale = locale;
        }

        public void AddNewBgmEntry(Window parent)
        {
        }
    }
}
