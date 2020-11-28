using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using System;
using Sm5sh.Mods.Music.Interfaces;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using System.Reactive;
using DynamicData;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using Sm5sh.GUI.Helpers;
using System.Collections.Generic;
using DynamicData.Binding;
using System.Reactive.Subjects;
using AutoMapper;

namespace Sm5sh.GUI.ViewModels
{
    public class GamePropertiesModalWindowViewModel : ViewModelBase, IDisposable
    {
        private readonly ReadOnlyObservableCollection<LocaleViewModel> _locales;
        private readonly ReadOnlyObservableCollection<SeriesEntryViewModel> _series;
        private readonly ReadOnlyObservableCollection<GameTitleEntryViewModel> _games;
        private readonly Subject<GameTitleEntryViewModel> _whenNewRequestToAddGameEntry;
        private const string REGEX_REPLACE = @"[^a-zA-Z0-9_]";
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public IMusicMod ModManager { get; }

        public MSBTFieldViewModel MSBTTitleEditor { get; set; }

        [Reactive]
        public string UiGameTitleId { get; set; }

        [Reactive]
        public string NameId { get; set; }

        [Reactive]
        public SeriesEntryViewModel SelectedSeries { get; set; }

        [Reactive]
        public bool Unk1 { get; set; }

        [Reactive]
        public int Release { get; set; }

        public GameTitleEntryViewModel SelectedGameTitleEntry { get; private set; }

        [Reactive]
        public bool IsEdit { get; set; }

        public ReadOnlyObservableCollection<SeriesEntryViewModel> Series { get { return _series; } }
        public ReadOnlyObservableCollection<GameTitleEntryViewModel> Games { get { return _games; } }
        public ReadOnlyObservableCollection<LocaleViewModel> Locales { get { return _locales; } }
        public IObservable<GameTitleEntryViewModel> WhenNewRequestToAddGameEntry { get { return _whenNewRequestToAddGameEntry; } }

        public ReactiveCommand<Window, Unit> ActionOK { get; }
        public ReactiveCommand<Window, Unit> ActionCancel { get; }

        public GamePropertiesModalWindowViewModel(ILogger<GamePropertiesModalWindowViewModel> logger, IMapper mapper, IObservable<IChangeSet<LocaleViewModel, string>> observableLocales, 
            IObservable<IChangeSet<SeriesEntryViewModel, string>> observableSeries, IObservable<IChangeSet<GameTitleEntryViewModel, string>> observableGames)
        {
            _logger = logger;
            _mapper = mapper;
            _whenNewRequestToAddGameEntry = new Subject<GameTitleEntryViewModel>();

            //Bind observables
            observableLocales
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _locales)
                .DisposeMany()
                .Subscribe();
            observableSeries
               .Sort(SortExpressionComparer<SeriesEntryViewModel>.Ascending(p => p.Title), SortOptimisations.IgnoreEvaluates)
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _series)
               .DisposeMany()
               .Subscribe();
            observableGames
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _games)
               .DisposeMany()
               .Subscribe();

            var canExecute = this.WhenAnyValue(x => x.UiGameTitleId, x => x.SelectedSeries.SeriesId, x => x.MSBTTitleEditor.CurrentLocalizedValue, (g, s, c) =>
            !string.IsNullOrEmpty(g) && !string.IsNullOrEmpty(s) && !string.IsNullOrEmpty(c));

            //Set up MSBT Fields
            var defaultLocale = new LocaleViewModel(Constants.DEFAULT_LOCALE, Constants.GetLocaleDisplayName(Constants.DEFAULT_LOCALE));
            MSBTTitleEditor = new MSBTFieldViewModel()
            {
                Locales = Locales,
                SelectedLocale = defaultLocale
            };

            this.WhenAnyValue(p => p.MSBTTitleEditor.CurrentLocalizedValue).Subscribe((o) => { FormatGameId(o); });

            ActionOK = ReactiveCommand.Create<Window>(SubmitDialogOK, canExecute);
            ActionCancel = ReactiveCommand.Create<Window>(SubmitDialogCancel);
        }

        public void LoadGame(GameTitleEntryViewModel gameEntryViewModel)
        {
            if(gameEntryViewModel == null)
            {
                UiGameTitleId = string.Empty;
                NameId = string.Empty;
                SelectedSeries = null;
                Unk1 = false;
                Release = 0;
                IsEdit = false;
                MSBTTitleEditor.MSBTValues = new Dictionary<string, string>();
            }
            else
            {
                UiGameTitleId = gameEntryViewModel.UiGameTitleId;
                NameId = gameEntryViewModel.NameId;
                SelectedSeries = gameEntryViewModel.SeriesViewModel;
                Unk1 = gameEntryViewModel.Unk1;
                Release = gameEntryViewModel.Release;
                MSBTTitleEditor.MSBTValues = gameEntryViewModel.MSBTTitle;
                IsEdit = true;
                SelectedGameTitleEntry = gameEntryViewModel;
            }
        }

        private void FormatGameId(string gameId)
        {
            if (!IsEdit)
            {
                if (string.IsNullOrEmpty(gameId))
                {
                    UiGameTitleId = "ui_gametitle_";
                    NameId = string.Empty;
                }
                else
                {
                    NameId = Regex.Replace(gameId.Replace(" ", "_"), REGEX_REPLACE, string.Empty).ToLower();
                    UiGameTitleId = $"ui_gametitle_{NameId}";
                }
            }
        }

        public void SubmitDialogOK(Window window)
        {
            if (!IsEdit)
            {
                SelectedGameTitleEntry = new GameTitleEntryViewModel(new Mods.Music.Models.GameTitleEntry(UiGameTitleId));
            }

            var refGame = SelectedGameTitleEntry.GetGameEntryReference();
            refGame.MSBTTitle = MSBTTitleEditor.MSBTValues;
            refGame.NameId = NameId;
            refGame.Release = Release;
            refGame.Unk1 = Unk1;
            refGame.UiSeriesId = SelectedSeries.SeriesId;
            _mapper.Map(refGame, SelectedGameTitleEntry);
            SelectedGameTitleEntry.SeriesViewModel = SelectedSeries;

            if (!IsEdit)
            {
                _whenNewRequestToAddGameEntry.OnNext(SelectedGameTitleEntry);
            }

            window.Close(window);
        }

        public void SubmitDialogCancel(Window window)
        {
            window.Close();
        }

        public void Dispose()
        {
            if (_whenNewRequestToAddGameEntry != null)
            {
                _whenNewRequestToAddGameEntry?.OnCompleted();
                _whenNewRequestToAddGameEntry?.Dispose();
            }
        }
    }
}
