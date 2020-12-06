using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Sm5shMusic.GUI.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Sm5shMusic.GUI.ViewModels
{
    public class ContextMenuViewModel : ViewModelBase, IDisposable
    {
        private readonly ILogger _logger;
        private readonly ReadOnlyObservableCollection<ModEntryViewModel> _mods;
        private readonly ReadOnlyObservableCollection<LocaleViewModel> _locales;
        private readonly BehaviorSubject<string> _whenLocaleChanged;
        private readonly Subject<ModEntryViewModel> _whenNewRequestToAddBgmEntry;
        private readonly Subject<Unit> _whenNewRequestToAddModEntry;
        private readonly Subject<Unit> _whenNewRequestToAddGameEntry;
        private readonly Subject<Unit> _whenNewRequestToEditModEntry;
        private readonly Subject<Unit> _whenNewRequestToEditGameEntry;

        public ReadOnlyObservableCollection<ModEntryViewModel> Mods { get { return _mods; } }
        public ReadOnlyObservableCollection<LocaleViewModel> Locales { get { return _locales; } }
        public IObservable<ModEntryViewModel> WhenNewRequestToAddBgmEntry { get { return _whenNewRequestToAddBgmEntry; } }
        public IObservable<Unit> WhenNewRequestToAddModEntry { get { return _whenNewRequestToAddModEntry; } }
        public IObservable<Unit> WhenNewRequestToAddGameEntry { get { return _whenNewRequestToAddGameEntry; } }
        public IObservable<Unit> WhenNewRequestToEditModEntry { get { return _whenNewRequestToEditModEntry; } }
        public IObservable<Unit> WhenNewRequestToEditGameEntry { get { return _whenNewRequestToEditGameEntry; } }
        public IObservable<string> WhenLocaleChanged { get { return _whenLocaleChanged; } }
        public ReactiveCommand<Unit, Unit> ActionNewMod { get; }
        public ReactiveCommand<Unit, Unit> ActionNewGame { get; }
        public ReactiveCommand<Unit, Unit> ActionEditMod { get; }
        public ReactiveCommand<Unit, Unit> ActionEditGame { get; }
        public ReactiveCommand<ModEntryViewModel, Unit> ActionAddNewBgm { get; }

        public ContextMenuViewModel(ILogger<BgmSongsViewModel> logger,
            IObservable<IChangeSet<ModEntryViewModel, string>> observableMusicModsList,
            IObservable<IChangeSet<LocaleViewModel, string>> observableLocalesList)
        {
            _logger = logger;
            _whenNewRequestToAddBgmEntry = new Subject<ModEntryViewModel>();
            _whenLocaleChanged = new BehaviorSubject<string>(Constants.DEFAULT_LOCALE);
            _whenNewRequestToAddModEntry = new Subject<Unit>();
            _whenNewRequestToAddGameEntry = new Subject<Unit>();
            _whenNewRequestToEditModEntry = new Subject<Unit>();
            _whenNewRequestToEditGameEntry = new Subject<Unit>();

            //List of mods
            observableMusicModsList
                .Sort(SortExpressionComparer<ModEntryViewModel>.Ascending(p => p.Name), SortOptimisations.IgnoreEvaluates)
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

            ActionNewMod = ReactiveCommand.Create(AddNewMod);
            ActionNewGame = ReactiveCommand.Create(AddNewGame);
            ActionEditMod = ReactiveCommand.Create(EditMod);
            ActionEditGame = ReactiveCommand.Create(EditGame);
            ActionAddNewBgm = ReactiveCommand.Create<ModEntryViewModel>(AddNewBgmEntry);
        }

        public void ChangeLocale(LocaleViewModel vmLocale)
        {
            _logger.LogDebug("Clicked Change Locale");
            _whenLocaleChanged.OnNext(vmLocale.Id);
        }

        public void AddNewBgmEntry(ModEntryViewModel vmMusicMod)
        {
            _logger.LogDebug("Clicked New Bgm");
            _whenNewRequestToAddBgmEntry.OnNext(vmMusicMod);
        }

        public void AddNewGame()
        {
            _logger.LogDebug("Clicked New Game");
            _whenNewRequestToAddGameEntry.OnNext(Unit.Default);
        }

        public void EditGame()
        {
            _logger.LogDebug("Clicked Edit Game");
            _whenNewRequestToEditGameEntry.OnNext(Unit.Default);
        }

        public void AddNewMod()
        {
            _logger.LogDebug("Clicked New Mod");
            _whenNewRequestToAddModEntry.OnNext(Unit.Default);
        }

        public void EditMod()
        {
            _logger.LogDebug("Clicked Edit Mod");
            _whenNewRequestToEditModEntry.OnNext(Unit.Default);
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
            if (_whenNewRequestToAddModEntry != null)
            {
                _whenNewRequestToAddModEntry?.OnCompleted();
                _whenNewRequestToAddModEntry?.Dispose();
            }
            if (_whenNewRequestToAddGameEntry != null)
            {
                _whenNewRequestToAddGameEntry?.OnCompleted();
                _whenNewRequestToAddGameEntry?.Dispose();
            }
            if (_whenNewRequestToEditModEntry != null)
            {
                _whenNewRequestToEditModEntry?.OnCompleted();
                _whenNewRequestToEditModEntry?.Dispose();
            }
            if (_whenNewRequestToEditGameEntry != null)
            {
                _whenNewRequestToEditGameEntry?.OnCompleted();
                _whenNewRequestToEditGameEntry?.Dispose();
            }
        }
    }
}
