using DynamicData;
using Microsoft.Extensions.Logging;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using Sm5sh.GUI.Helpers;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using System;
using Microsoft.Extensions.DependencyInjection;
using DynamicData.Binding;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Linq;
using Sm5sh.GUI.Models;
using Avalonia;
using System.Reactive.Subjects;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmSongsViewModel : ViewModelBase, IDisposable
    {
        private readonly ILogger _logger;
        private readonly ReadOnlyObservableCollection<ComboItem> _mods;
        private readonly Subject<string> _whenNewRequestToAddSong;

        public BgmListViewModel VMBgmList { get; }
        public BgmFiltersViewModel VMBgmFilters { get; }
        public BgmPropertiesViewModel VMBgmProperties { get; }
        public ReadOnlyObservableCollection<ComboItem> Mods { get { return _mods; } }
        public IObservable<string> WhenNewRequestToAddSong { get { return _whenNewRequestToAddSong; } }

        [Reactive]
        public string SelectedLocale { get; set; }

        public BgmSongsViewModel(IServiceProvider serviceProvider, ILogger<BgmSongsViewModel> logger, IObservable<IChangeSet<BgmEntryListViewModel, string>> observableBgmEntriesList)
        {
            _logger = logger;
            _whenNewRequestToAddSong = new Subject<string>();
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
                    var newMenuItem = new ComboItem(firstItem.ModPath, $"To {firstItem.ModName}...");
                    return newMenuItem;
                    })
                //.Prepend(_allChangeSet)
                .Sort(SortExpressionComparer<ComboItem>.Ascending(p => p.Label), SortOptimisations.IgnoreEvaluates)
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

        public void AddNewBgmEntry(string modPath)
        {
            _whenNewRequestToAddSong.OnNext(modPath);
        }

        public void Dispose()
        {
            if(_whenNewRequestToAddSong != null)
            {
                _whenNewRequestToAddSong?.OnCompleted();
                _whenNewRequestToAddSong?.Dispose();
            }
        }
    }
}
