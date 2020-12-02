﻿using AutoMapper;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sm5sh.GUI.Helpers;
using Sm5sh.GUI.Interfaces;
using Sm5sh.GUI.Views;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using VGMMusic;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sm5sh.GUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IAudioStateService _audioState;
        private readonly IMusicModManagerService _musicModManagerService;
        private readonly INus3AudioService _nus3AudioService;
        private readonly IVGMMusicPlayer _musicPlayer;
        private readonly ISm5shMusicOverride _sm5shMusicOverride;
        private readonly IMessageDialog _messageDialog;
        private readonly IFileDialog _fileDialog;
        private readonly IDialogWindow _rootDialog;
        private readonly IBuildDialog _buildDialog;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IOptions<Sm5shOptions> _config;
        private readonly ObservableCollection<LocaleViewModel> _locales;
        private readonly ObservableCollection<SeriesEntryViewModel> _seriesEntries;
        private readonly ObservableCollection<GameTitleEntryViewModel> _gameTitleEntries;
        private readonly ObservableCollection<BgmEntryViewModel> _bgmEntries;
        private readonly ObservableCollection<PlaylistEntryViewModel> _playlistsEntries;
        private readonly ObservableCollection<ModEntryViewModel> _musicMods;
        private readonly List<StageEntryViewModel> _stagesEntries; //Don't need obs yet
        private string _currentLocale;

        [Reactive]
        public bool IsAdvanced { get; set; }

        [Reactive]
        public bool IsLoading { get; set; }

        [Reactive]
        public bool IsShowingDebug { get; set; }

        public BgmPropertiesModalWindowViewModel VMBgmEditor { get; }
        public GamePropertiesModalWindowViewModel VMGameEditor { get; }
        public GamePickerModalWindowViewModel VMGamePicker { get; }
        public ModPropertiesModalWindowViewModel VMModEditor { get; }
        public ModPickerModalWindowViewModel VMModPicker { get; }
        public PlaylistPropertiesModalWindowViewModel VMPlaylistEditor { get; }
        public PlaylistPickerModalWindowViewModel VMPlaylistPicker { get; }
        public PlaylistStageAssignementModalWindowViewModel VMStageAssignemnt { get; }
        public BgmSongsViewModel VMBgmSongs { get; }
        public PlaylistViewModel VMPlaylists { get; }
        public BgmFiltersViewModel VMBgmFilters { get; }
        public ContextMenuViewModel VMContextMenu { get; }

        public ReactiveCommand<Unit, Unit> ActionExit { get; }
        public ReactiveCommand<Unit, Unit> ActionBuild { get; }
        public ReactiveCommand<Unit, Unit> ActionBuildNoCache { get; }
        public ReactiveCommand<Unit, Unit> ActionRefreshData { get; }

        public MainWindowViewModel(IServiceProvider serviceProvider, IOptions<Sm5shOptions> config, IMapper mapper, IAudioStateService audioState, INus3AudioService nus3AudioService, IVGMMusicPlayer musicPlayer,
            IMusicModManagerService musicModManagerService, ISm5shMusicOverride sm5shMusicOverride, IDialogWindow rootDialog, IMessageDialog messageDialog, IFileDialog fileDialog, IBuildDialog buildDialog, ILogger<MainWindowViewModel> logger)
        {
            _musicModManagerService = musicModManagerService;
            _nus3AudioService = nus3AudioService;
            _musicPlayer = musicPlayer;
            _sm5shMusicOverride = sm5shMusicOverride;
            _fileDialog = fileDialog;
            _buildDialog = buildDialog;
            _messageDialog = messageDialog;
            _rootDialog = rootDialog;
            _logger = logger;
            _mapper = mapper;
            _config = config;
            IsLoading = true;

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
            _stagesEntries = new List<StageEntryViewModel>();

            //Initialize filters
            VMBgmFilters = ActivatorUtilities.CreateInstance<BgmFiltersViewModel>(serviceProvider, observableBgmEntriesList);

            //Initialize main views
            VMBgmSongs = ActivatorUtilities.CreateInstance<BgmSongsViewModel>(serviceProvider, VMBgmFilters.WhenFiltersAreApplied, VMContextMenu);
            VMPlaylists = ActivatorUtilities.CreateInstance<PlaylistViewModel>(serviceProvider, VMBgmFilters.WhenFiltersAreApplied, observablePlaylistEntriesList, VMContextMenu);

            //Initialize Editors
            VMGameEditor = ActivatorUtilities.CreateInstance<GamePropertiesModalWindowViewModel>(serviceProvider,
                observableLocaleList, observableSeriesEntriesList, observableGameEntriesList);
            VMGamePicker = ActivatorUtilities.CreateInstance<GamePickerModalWindowViewModel>(serviceProvider,
                observableGameEntriesList);
            VMModEditor = ActivatorUtilities.CreateInstance<ModPropertiesModalWindowViewModel>(serviceProvider);
            VMModPicker = ActivatorUtilities.CreateInstance<ModPickerModalWindowViewModel>(serviceProvider,
                observableMusicModsList);
            VMBgmEditor = ActivatorUtilities.CreateInstance<BgmPropertiesModalWindowViewModel>(serviceProvider,
                observableLocaleList, observableSeriesEntriesList, observableGameEntriesList, observableBgmEntriesList);
            VMBgmEditor.VMGamePropertiesModal = VMGameEditor;
            VMPlaylistEditor = ActivatorUtilities.CreateInstance<PlaylistPropertiesModalWindowViewModel>(serviceProvider,
               observablePlaylistEntriesList);
            VMPlaylistPicker = ActivatorUtilities.CreateInstance<PlaylistPickerModalWindowViewModel>(serviceProvider,
                observablePlaylistEntriesList);
            VMStageAssignemnt = ActivatorUtilities.CreateInstance<PlaylistStageAssignementModalWindowViewModel>(serviceProvider,
                observablePlaylistEntriesList, _stagesEntries);

            //Listen to requests from children
            this.VMContextMenu.WhenNewRequestToAddBgmEntry.Subscribe(async (o) => await AddNewBgmEntry(o));
            this.VMContextMenu.WhenNewRequestToAddModEntry.Subscribe(async (o) => await AddNewOrEditMod());
            this.VMContextMenu.WhenNewRequestToAddGameEntry.Subscribe(async (o) => await AddNewOrEditGame());
            this.VMContextMenu.WhenNewRequestToEditGameEntry.Subscribe(async (o) => await EditGame());
            this.VMContextMenu.WhenNewRequestToEditModEntry.Subscribe(async (o) => await EditMod());
            this.VMBgmEditor.WhenNewRequestToAddGameEntry.Subscribe(async (o) => await AddNewOrEditGame(o));
            this.VMBgmSongs.WhenNewRequestToEditBgmEntry.Subscribe(async (o) => await EditBgmEntry(o));
            this.VMBgmSongs.WhenNewRequestToDeleteBgmEntry.Subscribe(async (o) => await DeleteBgmEntry(o));
            this.VMBgmSongs.WhenNewRequestToReorderBgmEntries.Subscribe((o) => ReorderSongs());
            this.VMPlaylists.WhenNewRequestToUpdatePlaylists.Subscribe((o) => UpdatePlaylists());
            this.VMPlaylists.WhenNewRequestToCreatePlaylist.Subscribe(async (o) => await AddNewOrEditPlaylist());
            this.VMPlaylists.WhenNewRequestToEditPlaylist.Subscribe(async (o) => await EditPlaylist());
            this.VMPlaylists.WhenNewRequestToDeletePlaylist.Subscribe(async (o) => await DeletePlaylist());
            this.VMPlaylists.WhenNewRequestToAssignPlaylistToStage.Subscribe(async (o) => await AssignPlaylistToStage());

            ActionExit = ReactiveCommand.Create(OnExit);
            ActionBuild = ReactiveCommand.CreateFromTask(OnBuild);
            ActionBuildNoCache = ReactiveCommand.CreateFromTask(OnBuildNoCache);
            ActionRefreshData = ReactiveCommand.Create(OnInitData);
        }


        #region Actions
        public void OnExit()
        {
            _rootDialog.Window.Close();
        }

        public async Task OnBuild()
        {
            IsLoading = true;
            IsShowingDebug = true;
            await _musicPlayer.Stop();
            _logger.LogInformation("Building with cache option ON. Don't touch anything :)");
            await _buildDialog.Build(true, (o) =>
            {
                IsLoading = false;
                IsShowingDebug = false;
            }, (o) =>
            {
                IsLoading = false;
                IsShowingDebug = false;
            });
        }

        public async Task OnBuildNoCache()
        {
            IsLoading = true;
            IsShowingDebug = true;
            await _musicPlayer.Stop();
            _logger.LogInformation("Building with cache option OFF. Don't touch anything :)");
            await _buildDialog.Build(false, (o) =>
            {
                IsLoading = false;
                IsShowingDebug = false;
            }, (o) =>
            {
                IsLoading = false;
                IsShowingDebug = false;
            });
        }

        public void OnInitData()
        {
            IsLoading = true;
            _buildDialog.Init((o) =>
            {
                InitAllDataAsync();
                IsLoading = false;
            });
        }

        public async Task OnAboutOpen()
        {
            await _messageDialog.ShowInformation("About",
                "Sm5sh Music - v0.03 by deinonychus71\r\n\r\n\r\n" +
                "Research: soneek\r\n\r\n" +
                "Testing: Demonslayerx8\r\n\r\n" +
                "prcEditor: https://github.com/BenHall-7/paracobNET \r\nBenHall-7\r\n\r\n" +
                "paramLabels: https://github.com/ultimate-research/param-labels \r\nBenHall-7, jam1garner, Dr-HyperCake, Birdwards, ThatNintendoNerd, ScanMountGoat, Meshima, Blazingflare, TheSmartKid, jugeeya, Demonslayerx8\r\n\r\n" +
                "msbtEditor: https://github.com/IcySon55/3DLandMSBTeditor \r\nIcySon55, exelix11 \r\n\r\n" +
                "nus3audio: https://github.com/jam1garner/nus3audio-rs \r\njam1garner \r\n\r\n" +
                "bgm-property: https://github.com/jam1garner/smash-bgm-property \r\njam1garner \r\n\r\n" +
                "VGAudio: https://github.com/Thealexbarney/VGAudio \r\nThealexbarney, soneek, jam1garner, devlead, Raytwo, nnn1590\r\n\r\n" +
                "vgmstream: https://github.com/vgmstream/vgmstream \r\nbnnm, kode54, NicknineTheEagle, bxaimc, Thealexbarney\r\nAll contributors: https://github.com/vgmstream/vgmstream/graphs/contributors \r\n\r\n" +
                "CrossArc: https://github.com/Ploaj/ArcCross \r\nPloaj, ScanMountGoat, BenHall-7, shadowninja108, jam1garner, M-1-RLG\r\n\r\n");
        }
        #endregion

        #region Data Operations
        public Task InitAllDataAsync()
        {
            _logger.LogInformation("Initialize data");

            //Reset everything
            _seriesEntries.Clear();
            _bgmEntries.Clear();
            _gameTitleEntries.Clear();
            _locales.Clear();
            _musicMods.Clear();
            _playlistsEntries.Clear();
            _stagesEntries.Clear();

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

            var stages = _audioState.GetStagesEntries().Select(p => new StageEntryViewModel(p));
            _stagesEntries.AddRange(stages);
            _logger.LogInformation("Stages Loaded.");

            return Task.CompletedTask;
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
            //TODO - Handle anything saving in a specific service
            _sm5shMusicOverride.UpdateSoundTestOrderConfig(_bgmEntries.ToDictionary(p => p.ToneId, p => p.DbRoot.TestDispOrder));
        }

        public void UpdatePlaylists()
        {
            try
            {
                //TODO - The data should be first persisted in the Playlists - currently not mapped back to the original PlaylistEntry
                //TODO - Handle anything saving in a specific service
                var playlists = _playlistsEntries.ToDictionary(p => p.Id, p => p.ToPlaylistEntry());
                _sm5shMusicOverride.UpdatePlaylistConfig(playlists);
            }
            catch (Exception e)
            {
                _logger.LogError("Error while saying playlists. Most likely a concurrency issue.", e.Message);
            }
        }

        public async Task AddNewOrEditMod(Window parent = null, ModEntryViewModel vmModEntry = null)
        {
            VMModEditor.LoadMusicMod(vmModEntry?.MusicMod);
            var modalCreateMod = new ModPropertiesModalWindow() { DataContext = VMModEditor };
            var results = await modalCreateMod.ShowDialog<ModPropertiesModalWindow>(parent != null ? parent : _rootDialog.Window);

            if (results != null)
            {
                if (!VMModEditor.IsEdit)
                {
                    var newManagerMod = _musicModManagerService.AddMusicMod(new MusicModInformation()
                    {
                        Name = VMModEditor.ModName,
                        Author = VMModEditor.ModAuthor,
                        Website = VMModEditor.ModWebsite,
                        Description = VMModEditor.ModDescription
                    }, VMModEditor.ModPath);

                    //TODO - Handle anything saving in a specific service
                    var newVmMusicMod = new ModEntryViewModel(newManagerMod.Id, newManagerMod);
                    _musicMods.Add(newVmMusicMod);
                    await AddNewBgmEntry(newVmMusicMod);
                }
            }
        }

        public async Task AddNewBgmEntry(ModEntryViewModel managerMod)
        {
            if (managerMod?.MusicMod == null)
            {
                await _messageDialog.ShowError("Error", "The mod could not be found.");
                return;
            }

            var results = await _fileDialog.OpenFileDialogAudio();
            if (results.Length == 0)
                return;

            //TODO - Handle anything saving in a specific service
            _logger.LogInformation("Adding {NbrFiles} files to Mod {ModPath}", results.Length, managerMod.ModPath);
            foreach (var inputFile in results)
            {
                string toneId = null;

                //Nus3audio - Retrieve Tone ID from file
                if (Path.GetExtension(inputFile).ToLower() == ".nus3audio")
                {
                    toneId = _nus3AudioService.GetToneIdFromNus3Audio(inputFile);
                    if (!await _messageDialog.ShowWarningConfirm("ToneId", $"The ToneId found in the Nus3Audio was '{toneId}'. Do you wish to use that?"))
                        toneId = null;
                }

                //Other file - Attempt to retrieve Tone Id from filename, otherwise ask user to type it
                if (string.IsNullOrEmpty(toneId))
                {
                    toneId = Path.GetFileNameWithoutExtension(inputFile);
                    if (!System.Text.RegularExpressions.Regex.IsMatch(Path.GetFileNameWithoutExtension(inputFile), @"^[a-z0-9_]+$"))
                    {
                        toneId = await _messageDialog.PromptInput("Tone ID?", "Could not extract a Tone ID from the filename.\r\nPlease enter a valid Tone ID.");
                        if (!System.Text.RegularExpressions.Regex.IsMatch(toneId, @"^[a-z0-9_]+$"))
                        {
                            //TODO: Fix later, and let user change id
                            await _messageDialog.ShowError("Error", $"The song {inputFile} could not be added to the mod.\r\nThe Tone ID should only contain lowercase characters, digits or underscore.");
                            continue;
                        }
                    }
                }

                //Check if exists
                if (_audioState.DoesToneIdExist(toneId))
                {
                    await _messageDialog.ShowError("Error", $"The Tone Id {toneId} already exists in the database. Make sure to pick a unique ID.");
                    continue;
                }

                //Create
                var newBgm = managerMod.MusicMod.AddBgm(toneId, inputFile);
                if (newBgm == null)
                {
                    await _messageDialog.ShowError("Error", $"The song {inputFile} could not be added to the mod.\r\nPlease check the logs.");
                    continue;
                }
                if (!_audioState.AddBgmEntry(newBgm))
                {
                    newBgm.MusicMod.RemoveBgm(newBgm.ToneId);
                    await _messageDialog.ShowError("Error", $"The song {inputFile} could not be added to the DB.\r\nPlease check the logs.");
                    continue;
                }
                var vmBgmEntry = _mapper.Map(newBgm, new BgmEntryViewModel(_musicPlayer, newBgm));
                //Edit metadata
                await EditBgmEntry(vmBgmEntry);
                //Add to UI
                _bgmEntries.Add(vmBgmEntry);
            }
        }

        public async Task EditBgmEntry(BgmEntryViewModel vmBgmEntry)
        {
            await VMBgmEditor.LoadBgmEntry(vmBgmEntry);
            Window modalEditBgmProps;
            Window results;
            if (IsAdvanced)
            {
                modalEditBgmProps = new BgmAdvancedPropertiesModalWindow() { DataContext = VMBgmEditor };
                results = await modalEditBgmProps.ShowDialog<BgmAdvancedPropertiesModalWindow>(_rootDialog.Window);
            }
            else
            {
                modalEditBgmProps = new BgmPropertiesModalWindow() { DataContext = VMBgmEditor };
                results = await modalEditBgmProps.ShowDialog<BgmPropertiesModalWindow>(_rootDialog.Window);
            }

            if (results != null)
            {
                vmBgmEntry.LoadLocalized(_currentLocale);
                var bgmNew = _mapper.Map(vmBgmEntry, vmBgmEntry.GetBgmEntryReference());
                bgmNew.GameTitle = _mapper.Map(vmBgmEntry.GameTitleViewModel, new GameTitleEntry(vmBgmEntry.UiGameTitleId));

                //TODO - Handle anything saving in a specific service
                //TODO - The data should be first persisted in the BgmEntries
                if (bgmNew.Source == EntrySource.Mod)
                    vmBgmEntry.MusicMod.UpdateBgm(bgmNew);
                else
                    _sm5shMusicOverride.UpdateCoreBgmEntry(bgmNew);
                //Reorder
                ReorderSongs();
            }
        }

        public async Task DeleteBgmEntry(BgmEntryViewModel vmBgmEntry)
        {
            if (vmBgmEntry != null)
            {
                var result = await _messageDialog.ShowWarningConfirm($"Delete '{vmBgmEntry.Title}'?", "Do you really want to remove this song?\r\nIf it's a Core song, this could cause unknown issues. Prefer hiding the song instead.");

                if (result)
                {
                    if (vmBgmEntry != null)
                    {
                        //TODO - Handle anything saving in a specific service
                        vmBgmEntry.StopPlay();
                        await Task.Delay(500);

                        _logger.LogInformation("Deleting {ToneId}...", vmBgmEntry.ToneId);
                        var bgmDelete = vmBgmEntry.GetBgmEntryReference();
                        if (bgmDelete.Source == EntrySource.Mod)
                            vmBgmEntry.MusicMod.RemoveBgm(bgmDelete.ToneId);
                        else
                            _sm5shMusicOverride.RemoveCoreBgmEntry(bgmDelete.ToneId);

                        _audioState.RemoveBgmEntry(bgmDelete.ToneId);
                        _bgmEntries.Remove(vmBgmEntry);
                        _logger.LogInformation("{ToneId} deleted.", vmBgmEntry.ToneId);

                        //Delete from playlist
                        foreach (var playlist in _playlistsEntries)
                            playlist.RemoveSong(vmBgmEntry.DbRoot.UiBgmId);
                    }
                }
            }
        }

        public async Task AddNewOrEditGame(Window parent = null, GameTitleEntryViewModel vmGameTitleEntry = null)
        {
            var isEdit = vmGameTitleEntry != null;
            VMGameEditor.LoadGame(vmGameTitleEntry);
            var modalCreateGame = new GamePropertiesModalWindow() { DataContext = VMGameEditor };
            var results = await modalCreateGame.ShowDialog<GamePropertiesModalWindow>(parent != null ? parent : _rootDialog.Window);

            if (results != null)
            {
                if (VMGameEditor.SelectedGameTitleEntry != null)
                    VMGameEditor.SelectedGameTitleEntry.LoadLocalized(_currentLocale);

                //TODO - Handle anything saving in a specific service
                if (!isEdit)
                {
                    _gameTitleEntries.Add(VMGameEditor.SelectedGameTitleEntry);
                }
                //No need to handle mod, it is saved with the song
                else if (VMGameEditor.SelectedGameTitleEntry.Source == EntrySource.Core)
                {
                    _sm5shMusicOverride.UpdateCoreGameTitleEntry(VMGameEditor.SelectedGameTitleEntry.GetGameEntryReference());
                }
            }
        }

        public async Task EditGame(Window parent = null)
        {
            var modalPickerGame = new GamePickerModalWindow() { DataContext = VMGamePicker };
            var results = await modalPickerGame.ShowDialog<GamePickerModalWindow>(parent != null ? parent : _rootDialog.Window);
            if (results != null)
            {
                await AddNewOrEditGame(parent, VMGamePicker.SelectedGameTitleEntry);
            }
        }

        public async Task EditMod(Window parent = null)
        {
            var modalPickerMod = new ModPickerModalWindow() { DataContext = VMModPicker };
            var results = await modalPickerMod.ShowDialog<ModPickerModalWindow>(parent != null ? parent : _rootDialog.Window);
            if (results != null)
            {
                await AddNewOrEditMod(parent, VMModPicker.SelectedModEntry);
            }
        }

        public async Task AddNewOrEditPlaylist(Window parent = null, PlaylistEntryViewModel vmPlaylist = null)
        {
            VMPlaylistEditor.LoadPlaylist(vmPlaylist);
            var modalPickerGame = new PlaylistPropertiesModalWindow() { DataContext = VMPlaylistEditor };
            var results = await modalPickerGame.ShowDialog<PlaylistPropertiesModalWindow>(parent != null ? parent : _rootDialog.Window);

            if (results != null)
            {
                if (!VMPlaylistEditor.IsEdit)
                {
                    //TODO - Handle anything saving in a specific service
                    _playlistsEntries.Add(VMPlaylistEditor.SelectedPlaylistEntry);
                }
            }
        }

        public async Task EditPlaylist(Window parent = null)
        {
            var modalPickerPlaylist = new PlaylistPickerModalWindow() { DataContext = VMPlaylistPicker };
            var results = await modalPickerPlaylist.ShowDialog<PlaylistPickerModalWindow>(parent != null ? parent : _rootDialog.Window);
            if (results != null)
            {
                await AddNewOrEditPlaylist(parent, VMPlaylistPicker.SelectedPlaylistEntry);
                UpdatePlaylists();
            }
        }

        public async Task DeletePlaylist(Window parent = null)
        {
            var modalPickerGame = new PlaylistPickerModalWindow() { DataContext = VMPlaylistPicker };
            var results = await modalPickerGame.ShowDialog<PlaylistPickerModalWindow>(_rootDialog.Window);
            if (results != null)
            {
                await DeletePlaylist(VMPlaylistPicker.SelectedPlaylistEntry);
            }
        }

        public async Task DeletePlaylist(PlaylistEntryViewModel vmPlaylistEntry)
        {
            var result = await _messageDialog.ShowWarningConfirm($"Delete Playlist '{vmPlaylistEntry.Title}'?", "Do you really want to remove this playlist?\r\nIf it's a Core playlist, this could cause unknown issues.");
            if (result)
            {
                _playlistsEntries.Remove(vmPlaylistEntry);
                UpdatePlaylists();
            }
        }

        public async Task AssignPlaylistToStage()
        {
            VMStageAssignemnt.LoadControl();
            var modalPickerAssignStagePlaylist = new PlaylistStageAssignementModalWindow() { DataContext = VMStageAssignemnt };
            var results = await modalPickerAssignStagePlaylist.ShowDialog<PlaylistStageAssignementModalWindow>(_rootDialog.Window);
            if (results != null)
            {
                foreach (var vmStage in VMStageAssignemnt.Stages)
                    _mapper.Map(vmStage, vmStage.GetStageEntryReference());

                //TODO - Handle anything saving in a specific service
                _sm5shMusicOverride.UpdateMusicStageOverride(_stagesEntries.Select(p => p.GetStageEntryReference()).ToList());
            }
        }
        #endregion
    }
}