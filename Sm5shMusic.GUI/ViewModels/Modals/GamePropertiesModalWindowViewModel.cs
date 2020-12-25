using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using Sm5sh.Mods.Music;
using Sm5sh.Mods.Music.Helpers;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using Sm5shMusic.GUI.Helpers;
using Sm5shMusic.GUI.Interfaces;
using Sm5shMusic.GUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sm5shMusic.GUI.ViewModels
{
    public class GamePropertiesModalWindowViewModel : ModalBaseViewModel<GameTitleEntryViewModel>
    {
        private readonly IOptions<ApplicationSettings> _config;
        private readonly ReadOnlyObservableCollection<SeriesEntryViewModel> _series;
        private readonly ReadOnlyObservableCollection<GameTitleEntryViewModel> _games;
        private const string REGEX_REPLACE = @"[^a-zA-Z0-9_]";
        private readonly string REGEX_VALIDATION = $"^{MusicConstants.InternalIds.GAME_TITLE_ID_PREFIX}[a-z0-9_]+$";
        private readonly ILogger _logger;
        private readonly IGUIStateManager _guiStateManager;
        private readonly IViewModelManager _viewModelManager;

        public IMusicMod ModManager { get; }

        public MSBTFieldViewModel MSBTTitleEditor { get; set; }

        [Reactive]
        public string UiGameTitleId { get; set; }

        [Reactive]
        public SeriesEntryViewModel SelectedSeries { get; set; }

        [Reactive]
        public string NameId { get; set; }

        [Reactive]
        public bool Unk1 { get; set; }

        [Reactive]
        public int Release { get; set; }

        [Reactive]
        public bool IsEdit { get; set; }

        public ReadOnlyObservableCollection<SeriesEntryViewModel> Series { get { return _series; } }
        public ReadOnlyObservableCollection<GameTitleEntryViewModel> Games { get { return _games; } }

        public GamePropertiesModalWindowViewModel(IOptions<ApplicationSettings> config, ILogger<GamePropertiesModalWindowViewModel> logger, IViewModelManager viewModelManager, IGUIStateManager guiStateManager,
            IObservable<IChangeSet<SeriesEntryViewModel, string>> observableSeries, IObservable<IChangeSet<GameTitleEntryViewModel, string>> observableGames)
        {
            _config = config;
            _logger = logger;
            _guiStateManager = guiStateManager;
            _viewModelManager = viewModelManager;

            //Bind observables
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

            //Set up MSBT Fields
            var defaultLocale = _config.Value.Sm5shMusicGUI.DefaultGUILocale;
            var defaultLocaleItem = new ComboItem(defaultLocale, Constants.GetLocaleDisplayName(defaultLocale));
            MSBTTitleEditor = new MSBTFieldViewModel()
            {
                //Locales = Locales,
                SelectedLocale = defaultLocaleItem,
                CurrentLocalizedValue = string.Empty
            };

            //Validation
            this.ValidationRule(p => p.UiGameTitleId,
                p => !string.IsNullOrEmpty(p) && Regex.IsMatch(p, REGEX_VALIDATION),
                $"The Game ID must start by '{MusicConstants.InternalIds.GAME_TITLE_ID_PREFIX}' and only contain lowercase letters, digits and underscore.");

            this.ValidationRule(p => p.UiGameTitleId,
              p => p != null && p.Length <= MusicConstants.GameResources.GameTitleMaximumSize,
              $"The Game ID is too long. Maximum is {MusicConstants.GameResources.GameTitleMaximumSize}");

            this.ValidationRule(p => p.UiGameTitleId,
             p => p != null && p.Length >= MusicConstants.GameResources.GameTitleMinimumSize,
             $"The Game ID is too short. Minimum is {MusicConstants.GameResources.GameTitleMinimumSize}");

            this.ValidationRule(p => p.UiGameTitleId,
                p => (IsEdit || !_games.Select(p => p.UiGameTitleId).Contains(p)),
                $"The Game ID already exists.");

            this.ValidationRule(p => p.SelectedSeries,
                p => p != null,
                $"Please select a series.");

            this.ValidationRule(p => p.MSBTTitleEditor.CurrentLocalizedValue,
                p => !string.IsNullOrEmpty(p),
                $"Please give a title to your game (in at least one language).");

            this.WhenAnyValue(p => p.MSBTTitleEditor.CurrentLocalizedValue).Subscribe((o) => { FormatGameId(o); });
        }

        private void FormatGameId(string gameId)
        {
            if (!IsEdit)
            {
                if (string.IsNullOrEmpty(gameId))
                {
                    UiGameTitleId = MusicConstants.InternalIds.GAME_TITLE_ID_PREFIX;
                    NameId = string.Empty;
                }
                else
                {
                    NameId = Regex.Replace(gameId.Replace(" ", "_"), REGEX_REPLACE, string.Empty).ToLower();
                    UiGameTitleId = $"{MusicConstants.InternalIds.GAME_TITLE_ID_PREFIX}{NameId}";
                }
            }
        }

        protected override async Task<bool> SaveChanges()
        {
            _logger.LogDebug("Save Changes");

            if (!IsEdit)
            {
                var newGameEntry = new GameTitleEntry(UiGameTitleId, EntrySource.Mod);
                await _guiStateManager.CreateNewGameTitleEntry(newGameEntry);
                _refSelectedItem = _viewModelManager.GetGameTitleViewModel(UiGameTitleId);
            }

            _refSelectedItem.MSBTTitle = SaveMSBTValues(MSBTTitleEditor.MSBTValues);
            _refSelectedItem.NameId = NameId;
            _refSelectedItem.Release = Release;
            _refSelectedItem.Unk1 = Unk1;
            _refSelectedItem.UiSeriesId = SelectedSeries.SeriesId;

            return true;
        }

        private Dictionary<string, string> SaveMSBTValues(Dictionary<string, string> msbtValues)
        {
            var output = new Dictionary<string, string>();
            if (msbtValues != null)
            {
                foreach (var msbtValue in msbtValues)
                {
                    if (!string.IsNullOrEmpty(msbtValue.Value))
                        output.Add(msbtValue.Key, msbtValue.Value);
                    else if (_config.Value.Sm5shMusicGUI.CopyToEmptyLocales && msbtValues.ContainsKey(_config.Value.Sm5shMusicGUI.DefaultMSBTLocale))
                        output.Add(msbtValue.Key, msbtValues[_config.Value.Sm5shMusicGUI.DefaultMSBTLocale]);
                }
            }
            return output;
        }

        protected override void LoadItem(GameTitleEntryViewModel item)
        {
            _logger.LogDebug("Load Item");

            if (item == null)
            {
                IsEdit = false;
                UiGameTitleId = string.Empty;
                NameId = string.Empty;
                SelectedSeries = null;
                Unk1 = false;
                Release = 0;
                MSBTTitleEditor.MSBTValues = new Dictionary<string, string>();
            }
            else
            {
                IsEdit = true;
                UiGameTitleId = item.UiGameTitleId;
                NameId = item.NameId;
                SelectedSeries = item.SeriesViewModel;
                Unk1 = item.Unk1;
                Release = item.Release;
                MSBTTitleEditor.MSBTValues = item.MSBTTitle;
            }
        }
    }
}
