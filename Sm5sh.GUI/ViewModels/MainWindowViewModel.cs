using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using VGMMusic;
using Sm5sh.Interfaces;
using Splat;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Microsoft.Extensions.DependencyInjection;
using System;
using Sm5sh.GUI.Interfaces;
using Microsoft.Extensions.Options;
using Sm5sh.GUI.Views;
using System.Collections.ObjectModel;
using System.Linq;
using Sm5sh.GUI.Helpers;
using AutoMapper;
using System.Reactive.Linq;

namespace Sm5sh.GUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IAudioStateService _audioState;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMusicModManagerService _musicModManagerService;
        private readonly IVGMMusicPlayer _musicPlayer;
        private readonly ISm5shMusicOverride _sm5shMusicOverride;
        private readonly IMessageDialog _messageDialog;
        private readonly IFileDialog _fileDialog;
        private readonly IDialogWindow _rootDialog;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IOptions<Sm5shOptions> _config;
        private readonly ObservableCollection<LocaleViewModel> _locales;
        private readonly ObservableCollection<SeriesEntryViewModel> _seriesEntries;
        private readonly ObservableCollection<GameTitleEntryViewModel> _gameTitleEntries;
        private readonly ObservableCollection<BgmEntryViewModel> _bgmEntries;
        private readonly ObservableCollection<PlaylistEntryViewModel> _playlistsEntries;
        private readonly ObservableCollection<ModEntryViewModel> _musicMods;
        private string _currentLocale;

        public BgmPropertiesModalWindowViewModel VMBgmEditor { get; }
        public GamePropertiesModalWindowViewModel VMGameEditor { get; }
        public ModPropertiesModalWindowViewModel VMModEditor { get; }
        public BgmSongsViewModel VMBgmSongs { get; }
        public PlaylistViewModel VMPlaylists { get; }
        public BgmFiltersViewModel VMBgmFilters { get; }
        public ContextMenuViewModel VMContextMenu { get; }

        public MainWindowViewModel(IServiceProvider serviceProvider, IOptions<Sm5shOptions> config, IMapper mapper, IAudioStateService audioState, IVGMMusicPlayer musicPlayer,
            IMusicModManagerService musicModManagerService, ISm5shMusicOverride sm5shMusicOverride, IDialogWindow rootDialog, IMessageDialog messageDialog, IFileDialog fileDialog, ILogger<MainWindowViewModel> logger)
        {
            _serviceProvider = serviceProvider;
            _musicModManagerService = musicModManagerService;
            _musicPlayer = musicPlayer;
            _sm5shMusicOverride = sm5shMusicOverride;
            _fileDialog = fileDialog;
            _messageDialog = messageDialog;
            _rootDialog = rootDialog;
            _logger = logger;
            _mapper = mapper;
            _config = config;

            //DI
            _audioState = audioState;

            //Initialize observables mods & locale
            _locales = new ObservableCollection<LocaleViewModel>();
            var observableLocaleList = _locales.ToObservableChangeSet(p => p.Id);
            _musicMods = new ObservableCollection<ModEntryViewModel>();
            var observableMusicModsList = _musicMods.ToObservableChangeSet(p => p.ModId);

            //Initialize Contextual Menu view
            VMContextMenu = ActivatorUtilities.CreateInstance<ContextMenuViewModel>(serviceProvider, observableMusicModsList, observableLocaleList);

            //Initialize Content Observables - Games & BGM are automatically localized
            VMContextMenu.WhenLocaleChanged.Subscribe((locale) => _currentLocale = locale);
            _seriesEntries = new ObservableCollection<SeriesEntryViewModel>();
            var observableSeriesEntriesList = _seriesEntries.ToObservableChangeSet(p => p.SeriesId);
            _bgmEntries = new ObservableCollection<BgmEntryViewModel>();
            var observableBgmEntriesList = _bgmEntries.ToObservableChangeSet(p => p.ToneId)
                .DeferUntilLoaded()
                .AutoRefreshOnObservable(p => VMContextMenu.WhenLocaleChanged)
                .ForEachChange(o => o.Current.LoadLocalized(_currentLocale));
            _gameTitleEntries = new ObservableCollection<GameTitleEntryViewModel>();
            var observableGameEntriesList = _gameTitleEntries.ToObservableChangeSet(p => p.UiGameTitleId)
                .AutoRefreshOnObservable(p => VMContextMenu.WhenLocaleChanged)
                .ForEachChange(o => o.Current.LoadLocalized(_currentLocale));
            _playlistsEntries = new ObservableCollection<PlaylistEntryViewModel>();
            var observablePlaylistEntriesList = _playlistsEntries.ToObservableChangeSet(p => p.Id);

            //Initialize filters
            VMBgmFilters = ActivatorUtilities.CreateInstance<BgmFiltersViewModel>(serviceProvider, observableBgmEntriesList);

            //Initialize main views
            VMBgmSongs = ActivatorUtilities.CreateInstance<BgmSongsViewModel>(serviceProvider, VMBgmFilters.WhenFiltersAreApplied, VMContextMenu);
            VMPlaylists = ActivatorUtilities.CreateInstance<PlaylistViewModel>(serviceProvider, VMBgmFilters.WhenFiltersAreApplied, observablePlaylistEntriesList, VMContextMenu);

            //Initialize Editors
            VMGameEditor = ActivatorUtilities.CreateInstance<GamePropertiesModalWindowViewModel>(serviceProvider,
                observableLocaleList, observableSeriesEntriesList, observableGameEntriesList);
            VMModEditor = ActivatorUtilities.CreateInstance<ModPropertiesModalWindowViewModel>(serviceProvider,
                observableMusicModsList);
            VMBgmEditor = ActivatorUtilities.CreateInstance<BgmPropertiesModalWindowViewModel>(serviceProvider,
                observableLocaleList, observableSeriesEntriesList, observableGameEntriesList, observableBgmEntriesList);
            VMBgmEditor.VMGamePropertiesModal = VMGameEditor;

            //Listen to requests from children
            this.VMContextMenu.WhenNewRequestToAddBgmEntry.Subscribe(async (o) => await AddNewBgmEntry(o));
            this.VMBgmSongs.WhenNewRequestToEditBgmEntry.Subscribe(async (o) => await EditBgmEntry(o));
            this.VMBgmSongs.WhenNewRequestToDeleteBgmEntry.Subscribe(async (o) => await DeleteBgmEntry(o));
            this.VMBgmSongs.WhenNewRequestToReorderBgmEntries.Subscribe((o) => ReorderSongs());
            this.VMGameEditor.WhenNewRequestToAddGameEntry.Subscribe((o) => AddNewGame(o));
            this.VMPlaylists.WhenNewRequestToUpdatePlaylists.Subscribe((o) => UpdatePlaylists());
        }

        public void ResetBgmList()
        {
            Task.Run(() =>
            {
                //Reset everything
                _seriesEntries.Clear();
                _bgmEntries.Clear();
                _gameTitleEntries.Clear();
                _locales.Clear();
                _musicMods.Clear();
                _playlistsEntries.Clear();

                //Load
                var stateManager = _serviceProvider.GetService<IStateManager>();
                var mods = _serviceProvider.GetServices<ISm5shMod>();
                stateManager.Init();
                foreach (var mod in mods)
                {
                    mod.Init();
                }

                //Init view models
                //Bind
                var localeList = _audioState.GetLocales().Select(p => new LocaleViewModel(p, Constants.GetLocaleDisplayName(p)));
                _locales.AddRange(localeList);
                _logger.LogInformation("Locales Loaded.");

                var modsList = _musicModManagerService.MusicMods.Select(p => new ModEntryViewModel(p.Id, p));
                _musicMods.AddRange(modsList);
                _logger.LogInformation("Music Mods List Loaded.");

                var seriesList = _audioState.GetSeriesEntries().Select(p => new SeriesEntryViewModel(p)).ToDictionary(p => p.SeriesId, p => p);
                _seriesEntries.AddRange(seriesList.Values);
                _logger.LogInformation("Series Loaded.");

                var gameList = _audioState.GetGameTitleEntries().Select(p => _mapper.Map(p, new GameTitleEntryViewModel(p) { SeriesViewModel = seriesList[p.UiSeriesId] })).ToDictionary(p => p.UiGameTitleId, p => p);
                _gameTitleEntries.AddRange(gameList.Values);
                _logger.LogInformation("Game Titles Loaded.");

                var bgmList = _audioState.GetBgmEntries().Select(p => _mapper.Map(p, new BgmEntryViewModel(_musicPlayer, p) { GameTitleViewModel = gameList[p.GameTitleId] }));
                _bgmEntries.AddRange(bgmList);
                ReorderSongs();
                _logger.LogInformation("BGM List Loaded.");

                var playlists = _audioState.GetPlaylists().Select(p => new PlaylistEntryViewModel(p, _bgmEntries.ToDictionary(p => p.DbRoot.UiBgmId, p => p)));
                _playlistsEntries.AddRange(playlists);
                _logger.LogInformation("Playlists Loaded.");
            });
        }

        public void ReorderSongs()
        {
            short i = 0;
            var listBgms = _bgmEntries.Where(p => !p.HiddenInSoundTest).OrderBy(p => p.DbRoot.TestDispOrder);
            foreach (var bgmEntry in listBgms)
            {
                bgmEntry.DbRoot.TestDispOrder = i;
                i += 2;
            }
            //TODO - The data should be first persisted in the BgmEntries
            _sm5shMusicOverride.UpdateSoundTestOrderConfig(_bgmEntries.ToDictionary(p => p.ToneId, p => p.DbRoot.TestDispOrder));
        }

        public void UpdatePlaylists()
        {
            //TODO - The data should be first persisted in the Playlists - currently not mapped back to the original PlaylistEntry
            var playlists = _playlistsEntries.ToDictionary(p => p.Id, p => p.ToPlaylistEntry().Tracks);
            _sm5shMusicOverride.UpdatePlaylistConfig(playlists);
        }

        public async Task AddNewBgmEntry(ModEntryViewModel managerMod)
        {
            if (managerMod.MusicMod == null)
                managerMod = await CreateNewMod();

            if (managerMod == null)
                return;

            var results = await _fileDialog.OpenFileDialogAudio();
            if (results.Length == 0)
                return;

            _logger.LogInformation("Adding {NbrFiles} files to Mod {ModPath}", results.Length, managerMod.ModPath);
            foreach (var inputFile in results)
            {
                var newBgm = managerMod.MusicMod.AddBgm(inputFile);
                //TODO: Handle error while adding file into mod
                if (newBgm == null)
                {

                    continue;
                }
                //TODO: Handle error while adding file into DB
                if (!_audioState.AddBgmEntry(newBgm))
                {
                    continue;
                }
                var vmBgmEntry = new BgmEntryViewModel(_musicPlayer, newBgm);
                //Edit metadata
                await EditBgmEntry(vmBgmEntry);
                //Add to UI
                _bgmEntries.Add(vmBgmEntry);
            }
        }

        public async Task EditBgmEntry(BgmEntryViewModel vmBgmEntry)
        {
            VMBgmEditor.LoadBgmEntry(vmBgmEntry);
            var modalEditBgmProps = new BgmPropertiesModalWindow() { DataContext = VMBgmEditor };
            var results = await modalEditBgmProps.ShowDialog<BgmPropertiesModalWindow>(_rootDialog.Window);

            if (results != null)
            {
                vmBgmEntry.LoadLocalized(_currentLocale);
                var bgmNew = _mapper.Map(vmBgmEntry, vmBgmEntry.GetBgmEntryReference());
                bgmNew.GameTitle = _mapper.Map(vmBgmEntry.GameTitleViewModel, new GameTitleEntry(vmBgmEntry.UiGameTitleId));
                //TODO - The data should be first persisted in the BgmEntries
                if (bgmNew.Source == Mods.Music.Models.BgmEntryModels.EntrySource.Mod)
                    vmBgmEntry.MusicMod.UpdateBgm(bgmNew);
                else
                    _sm5shMusicOverride.UpdateCoreBgmEntry(bgmNew);
            }
        }

        public async Task DeleteBgmEntry(BgmEntryViewModel vmBgmEntry)
        {
            if (vmBgmEntry != null)
            {
                var result = await _messageDialog.ShowWarningConfirm($"Delete '{vmBgmEntry.Title}'?", "Do you really want to remove this song? If it's a Core song, this could cause unknown issues. Prefer hiding the song instead.");

                if (result)
                {
                    if (vmBgmEntry != null)
                    {
                        _logger.LogInformation("Deleting {ToneId}...", vmBgmEntry.ToneId);
                        var bgmDelete = vmBgmEntry.GetBgmEntryReference();
                        if (bgmDelete.Source == Mods.Music.Models.BgmEntryModels.EntrySource.Mod)
                            vmBgmEntry.MusicMod.RemoveBgm(bgmDelete.ToneId);
                        else
                            _sm5shMusicOverride.RemoveCoreBgmEntry(bgmDelete.ToneId);

                        _bgmEntries.Remove(vmBgmEntry);
                        _logger.LogInformation("{ToneId} deleted.", vmBgmEntry.ToneId);
                    }
                }
            }
        }

        public void AddNewGame(GameTitleEntryViewModel vmGameEntry)
        {
            _gameTitleEntries.Add(vmGameEntry);
        }

        public async Task<ModEntryViewModel> CreateNewMod()
        {
            VMModEditor.LoadMusicMod(null);
            var modalCreateMod = new ModPropertiesModalWindow() { DataContext = VMModEditor };
            var results = await modalCreateMod.ShowDialog<ModPropertiesModalWindow>(_rootDialog.Window);

            if (results != null)
            {
                var newManagerMod = _musicModManagerService.AddMusicMod(new MusicModInformation()
                {
                    Name = VMModEditor.ModName,
                    Author = VMModEditor.ModAuthor,
                    Website = VMModEditor.ModWebsite,
                    Description = VMModEditor.ModDescription
                }, VMModEditor.ModPath);

                var newVmMusicMod = new ModEntryViewModel(newManagerMod.Id, newManagerMod);
                _musicMods.Add(newVmMusicMod);
                return newVmMusicMod;
            }
            return null;
        }
    }
}
