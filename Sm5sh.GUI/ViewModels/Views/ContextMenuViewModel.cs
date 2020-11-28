using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Sm5sh.GUI.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Sm5sh.GUI.ViewModels
{
    public class ContextMenuViewModel : ViewModelBase, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IChangeSet<ModEntryViewModel, string> _newModSet;
        private readonly ReadOnlyObservableCollection<ModEntryViewModel> _mods;
        private readonly ReadOnlyObservableCollection<LocaleViewModel> _locales;
        private readonly BehaviorSubject<string> _whenLocaleChanged;
        private readonly Subject<ModEntryViewModel> _whenNewRequestToAddBgmEntry;

        public ReadOnlyObservableCollection<ModEntryViewModel> Mods { get { return _mods; } }
        public ReadOnlyObservableCollection<LocaleViewModel> Locales { get { return _locales; } }
        public IObservable<ModEntryViewModel> WhenNewRequestToAddBgmEntry { get { return _whenNewRequestToAddBgmEntry; } }
        public IObservable<string> WhenLocaleChanged { get { return _whenLocaleChanged; } }

        public ContextMenuViewModel(ILogger<BgmSongsViewModel> logger,
            IObservable<IChangeSet<ModEntryViewModel, string>> observableMusicModsList,
            IObservable<IChangeSet<LocaleViewModel, string>> observableLocalesList)
        {
            _logger = logger;
            _whenNewRequestToAddBgmEntry = new Subject<ModEntryViewModel>();
            _newModSet = GetCreateNewMod();
            _whenLocaleChanged = new BehaviorSubject<string>(Constants.DEFAULT_LOCALE);

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

        }

        public void ChangeLocale(LocaleViewModel vmLocale)
        {
            _whenLocaleChanged.OnNext(vmLocale.Id);
        }

        public void AddNewBgmEntry(ModEntryViewModel vmMusicMod)
        {
            _whenNewRequestToAddBgmEntry.OnNext(vmMusicMod);
        }

        public void Dispose()
        {
            if (_whenLocaleChanged != null)
            {
                _whenLocaleChanged?.OnCompleted();
                _whenLocaleChanged?.Dispose();
            }
            if (_whenNewRequestToAddBgmEntry != null)
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
