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
using Sm5sh.GUI.Models;
using Avalonia;
using System.Reactive.Subjects;
using System.Collections.Generic;
using Sm5sh.Mods.Music.Interfaces;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmSongsViewModel : ViewModelBase, IDisposable
    {
        private readonly ILogger _logger;
        private IChangeSet<ModItem, string> _newModSet;
        private readonly ReadOnlyObservableCollection<ModItem> _mods;
        private readonly Subject<IMusicMod> _whenNewRequestToAddBgmEntry;

        public BgmListViewModel VMBgmList { get; }
        public BgmFiltersViewModel VMBgmFilters { get; }
        public BgmPropertiesViewModel VMBgmProperties { get; }
        public ReadOnlyObservableCollection<ModItem> Mods { get { return _mods; } }
        public IObservable<IMusicMod> WhenNewRequestToAddBgmEntry { get { return _whenNewRequestToAddBgmEntry; } }

        [Reactive]
        public string SelectedLocale { get; set; }

        public BgmSongsViewModel(IServiceProvider serviceProvider, ILogger<BgmSongsViewModel> logger, 
            IObservable<IChangeSet<BgmEntryViewModel, string>> observableBgmEntriesList,
            IObservable<IChangeSet<IMusicMod, string>> observableMusicModsList)
        {
            _logger = logger;
            _whenNewRequestToAddBgmEntry = new Subject<IMusicMod>();
            _newModSet = GetCreateNewMod();
            SelectedLocale = Constants.DEFAULT_LOCALE;

            var whenLocaleChanged = this.WhenAnyValue(p => p.SelectedLocale);
            var observableLocalizedBgmEntriesList = observableBgmEntriesList
                .AutoRefreshOnObservable(p => whenLocaleChanged)
                .ForEachChange(o => o.Current.LoadLocalized(SelectedLocale));

            //List of mods
            observableMusicModsList
                .Transform(p => new ModItem(p, $"To {p.Mod.Name}..."))
                .Prepend(_newModSet)
                .Sort(SortExpressionComparer<ModItem>.Descending(p => p.CreateFlag).ThenByAscending(p => p.Label), SortOptimisations.IgnoreEvaluates)
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

        public void AddNewBgmEntry(IMusicMod musicMod)
        {
            _whenNewRequestToAddBgmEntry.OnNext(musicMod);
        }

        public void Dispose()
        {
            if(_whenNewRequestToAddBgmEntry != null)
            {
                _whenNewRequestToAddBgmEntry?.OnCompleted();
                _whenNewRequestToAddBgmEntry?.Dispose();
            }
        }

        private IChangeSet<ModItem, string> GetCreateNewMod()
        {
            return new ChangeSet<ModItem, string>(new List<Change<ModItem, string>>()
            {
                new Change<ModItem, string>(ChangeReason.Add, "-1", new ModItem(null,"Create New...", true ))
            });
        }
    }
}
