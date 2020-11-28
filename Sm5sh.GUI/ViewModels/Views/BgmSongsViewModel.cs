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
using System.Linq;
using Avalonia;
using System.Reactive.Subjects;
using System.Collections.Generic;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmSongsViewModel : ViewModelBase, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IChangeSet<ModEntryViewModel, string> _newModSet;
        private readonly ReadOnlyObservableCollection<ModEntryViewModel> _mods;
        private readonly ReadOnlyObservableCollection<LocaleViewModel> _locales;
        private readonly Subject<ModEntryViewModel> _whenNewRequestToAddBgmEntry;
        private readonly IObservable<string> _whenLocaleChanged;

        public BgmListViewModel VMBgmList { get; }
        public BgmFiltersViewModel VMBgmFilters { get; }
        public BgmPropertiesViewModel VMBgmProperties { get; }
        public ReadOnlyObservableCollection<ModEntryViewModel> Mods { get { return _mods; } }
        public ReadOnlyObservableCollection<LocaleViewModel> Locales { get { return _locales; } }
        public IObservable<ModEntryViewModel> WhenNewRequestToAddBgmEntry { get { return _whenNewRequestToAddBgmEntry; } }
        public IObservable<string> WhenLocaleChanged { get { return _whenLocaleChanged; } }

        [Reactive]
        public string SelectedLocale { get; set; }

        public BgmSongsViewModel(IServiceProvider serviceProvider, ILogger<BgmSongsViewModel> logger, 
            IObservable<IChangeSet<BgmEntryViewModel, string>> observableBgmEntriesList,
            IObservable<IChangeSet<ModEntryViewModel, string>> observableMusicModsList,
            IObservable<IChangeSet<LocaleViewModel, string>> observableLocalesList)
        {
            _logger = logger;
            _whenNewRequestToAddBgmEntry = new Subject<ModEntryViewModel>();
            _newModSet = GetCreateNewMod();
            SelectedLocale = Constants.DEFAULT_LOCALE;

            _whenLocaleChanged = this.WhenAnyValue(p => p.SelectedLocale);
            var observableLocalizedBgmEntriesList = observableBgmEntriesList
                .AutoRefreshOnObservable(p => _whenLocaleChanged)
                .ForEachChange(o => o.Current.LoadLocalized(SelectedLocale));

            //List of mods
            observableMusicModsList
                .Prepend(_newModSet)
                .Sort(SortExpressionComparer<ModEntryViewModel>.Descending(p => p.CreateFlag).ThenByAscending(p => p.ModName), SortOptimisations.IgnoreEvaluates)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _mods)
                .DisposeMany()
                .Subscribe();
            //locales
            observableLocalesList
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _locales)
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

        public void ChangeLocale(LocaleViewModel vmLocale)
        {
            SelectedLocale = vmLocale.Id;
        }

        public void AddNewBgmEntry(ModEntryViewModel vmMusicMod)
        {
            _whenNewRequestToAddBgmEntry.OnNext(vmMusicMod);
        }

        public void Dispose()
        {
            if(_whenNewRequestToAddBgmEntry != null)
            {
                _whenNewRequestToAddBgmEntry?.OnCompleted();
                _whenNewRequestToAddBgmEntry?.Dispose();
            }
        }

        private IChangeSet<ModEntryViewModel, string> GetCreateNewMod()
        {
            return new ChangeSet<ModEntryViewModel, string>(new List<Change<ModEntryViewModel, string>>()
            {
                new Change<ModEntryViewModel, string>(ChangeReason.Add, "-1", new ModEntryViewModel(null, null){ ModName = "Create New Mod", CreateFlag = true })
            });
        }
    }
}
