﻿using AutoMapper;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sm5shMusic.GUI.Helpers;
using Sm5shMusic.GUI.Interfaces;
using Sm5shMusic.GUI.Views;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using VGMMusic;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Sm5sh;
using Sm5shMusic.GUI.Dialogs;
using System.IO;

namespace Sm5shMusic.GUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IAudioStateService _audioState;
        private readonly IMusicModManagerService _musicModManagerService;
        private readonly IGUIStateManager _guiStateManager;
        private readonly IViewModelManager _viewModelManager;
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
        private string _currentLocale;

        private ModalDialog<BgmPropertiesModalWindow, BgmPropertiesModalWindowViewModel, BgmEntryViewModel> _dialogSimpleBgmEditor;
        private ModalDialog<BgmAdvancedPropertiesModalWindow, BgmPropertiesModalWindowViewModel, BgmEntryViewModel> _dialogAdvancedBgmEditor;
        private ModalDialog<GamePropertiesModalWindow, GamePropertiesModalWindowViewModel, GameTitleEntryViewModel> _dialogGameEditor;

        [Reactive]
        public bool IsAdvanced { get; set; }

        [Reactive]
        public bool IsLoading { get; set; }

        [Reactive]
        public bool IsShowingDebug { get; set; }

        public GamePickerModalWindowViewModel VMGamePicker { get; }
        public ModPropertiesModalWindowViewModel VMModEditor { get; }
        public ModPickerModalWindowViewModel VMModPicker { get; }
        public PlaylistPropertiesModalWindowViewModel VMPlaylistEditor { get; }
        public PlaylistPickerModalWindowViewModel VMPlaylistPicker { get; }
        public PlaylistStageAssignementModalWindowViewModel VMStageAssignement { get; }
        public ToneIdCreationModalWindowModel VMToneIdCreation { get; }
        public BgmSongsViewModel VMBgmSongs { get; }
        public PlaylistViewModel VMPlaylists { get; }
        public BgmFiltersViewModel VMBgmFilters { get; }
        public ContextMenuViewModel VMContextMenu { get; }

        public ReactiveCommand<Unit, Unit> ActionExit { get; }
        public ReactiveCommand<Unit, Unit> ActionBuild { get; }
        public ReactiveCommand<Unit, Unit> ActionBuildNoCache { get; }
        public ReactiveCommand<Unit, Unit> ActionRefreshData { get; }

        public MainWindowViewModel(IServiceProvider serviceProvider, IOptions<Sm5shOptions> config, IViewModelManager viewModelManager, IGUIStateManager guiStateManager, IMapper mapper, IAudioStateService audioState, INus3AudioService nus3AudioService, IVGMMusicPlayer musicPlayer,
            IMusicModManagerService musicModManagerService, ISm5shMusicOverride sm5shMusicOverride, IDialogWindow rootDialog, IMessageDialog messageDialog, IFileDialog fileDialog, IBuildDialog buildDialog, ILogger<MainWindowViewModel> logger)
        {
            _viewModelManager = viewModelManager;
            _musicModManagerService = musicModManagerService;
            _guiStateManager = guiStateManager;
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

            //Initialize Contextual Menu view
            VMContextMenu = ActivatorUtilities.CreateInstance<ContextMenuViewModel>(serviceProvider, viewModelManager.ObservableModsEntries, viewModelManager.ObservableLocales);
            VMContextMenu.WhenLocaleChanged.Subscribe((locale) => _currentLocale = locale);
            var observableBgmDbRootEntriesList = viewModelManager.ObservableDbRootEntries
                .DeferUntilLoaded()
                .AutoRefreshOnObservable(p => VMContextMenu.WhenLocaleChanged)
                .ForEachChange(o => o.Current.LoadLocalized(_currentLocale));
            var observableGameEntriesList = viewModelManager.ObservableGameTitles
                .DeferUntilLoaded()
                .AutoRefreshOnObservable(p => VMContextMenu.WhenLocaleChanged)
                .ForEachChange(o => o.Current.LoadLocalized(_currentLocale));

            //Initialize filters
            VMBgmFilters = ActivatorUtilities.CreateInstance<BgmFiltersViewModel>(serviceProvider, observableBgmDbRootEntriesList);

            //Initialize main views
            VMBgmSongs = ActivatorUtilities.CreateInstance<BgmSongsViewModel>(serviceProvider, VMBgmFilters.WhenFiltersAreApplied, VMContextMenu);
            VMPlaylists = ActivatorUtilities.CreateInstance<PlaylistViewModel>(serviceProvider, VMBgmFilters.WhenFiltersAreApplied, viewModelManager.ObservablePlaylistsEntries, VMContextMenu);

            //Initialize Editors

            VMGamePicker = ActivatorUtilities.CreateInstance<GamePickerModalWindowViewModel>(serviceProvider,
                observableGameEntriesList);
            VMModEditor = ActivatorUtilities.CreateInstance<ModPropertiesModalWindowViewModel>(serviceProvider);
            VMModPicker = ActivatorUtilities.CreateInstance<ModPickerModalWindowViewModel>(serviceProvider,
                viewModelManager.ObservableModsEntries);
            
            VMPlaylistEditor = ActivatorUtilities.CreateInstance<PlaylistPropertiesModalWindowViewModel>(serviceProvider,
               viewModelManager.ObservablePlaylistsEntries);
            VMPlaylistPicker = ActivatorUtilities.CreateInstance<PlaylistPickerModalWindowViewModel>(serviceProvider,
                viewModelManager.ObservablePlaylistsEntries);
            VMStageAssignement = ActivatorUtilities.CreateInstance<PlaylistStageAssignementModalWindowViewModel>(serviceProvider,
                viewModelManager.ObservablePlaylistsEntries, viewModelManager.ObservableStagesEntries);
            VMToneIdCreation = ActivatorUtilities.CreateInstance<ToneIdCreationModalWindowModel>(serviceProvider,
                observableBgmDbRootEntriesList);

            //Setup GameEditor
            var vmGameEditor = ActivatorUtilities.CreateInstance<GamePropertiesModalWindowViewModel>(serviceProvider,
                viewModelManager.ObservableLocales, viewModelManager.ObservableSeries, observableGameEntriesList);
            _dialogGameEditor = new ModalDialog<GamePropertiesModalWindow, GamePropertiesModalWindowViewModel, GameTitleEntryViewModel>(vmGameEditor);

            //Setup BgmEditor
            var vmBgmEditor = ActivatorUtilities.CreateInstance<BgmPropertiesModalWindowViewModel>(serviceProvider,
                viewModelManager.ObservableLocales, viewModelManager.ObservableSeries, observableGameEntriesList, viewModelManager.ObservableStreamSetEntries);
            vmBgmEditor.VMGamePropertiesModal = vmGameEditor;
            vmBgmEditor.WhenNewRequestToAddGameEntry.Subscribe(async (o) => await AddNewOrEditGame(o));
            _dialogSimpleBgmEditor = new ModalDialog<BgmPropertiesModalWindow, BgmPropertiesModalWindowViewModel, BgmEntryViewModel>(vmBgmEditor);
            _dialogAdvancedBgmEditor = new ModalDialog<BgmAdvancedPropertiesModalWindow, BgmPropertiesModalWindowViewModel, BgmEntryViewModel>(vmBgmEditor);

            //Listen to requests from children
            this.VMContextMenu.WhenNewRequestToAddBgmEntry.Subscribe(async (o) => await AddNewBgmEntry(o));
            this.VMContextMenu.WhenNewRequestToAddModEntry.Subscribe(async (o) => await AddNewOrEditMod());
            this.VMContextMenu.WhenNewRequestToAddGameEntry.Subscribe(async (o) => await AddNewOrEditGame());
            this.VMContextMenu.WhenNewRequestToEditGameEntry.Subscribe(async (o) => await EditGame());
            this.VMContextMenu.WhenNewRequestToEditModEntry.Subscribe(async (o) => await EditMod());
            
            this.VMBgmSongs.WhenNewRequestToEditBgmEntry.Subscribe(async (o) => await EditBgmEntry(o));
            this.VMBgmSongs.WhenNewRequestToDeleteBgmEntry.Subscribe(async (o) => await DeleteBgmEntry(o));
            this.VMBgmSongs.WhenNewRequestToReorderBgmEntries.Subscribe((o) => _viewModelManager.ReorderSongs());
            this.VMPlaylists.WhenNewRequestToUpdatePlaylists.Subscribe((o) => UpdatePlaylists());
            this.VMPlaylists.WhenNewRequestToCreatePlaylist.Subscribe(async (o) => await AddNewOrEditPlaylist());
            this.VMPlaylists.WhenNewRequestToEditPlaylist.Subscribe(async (o) => await EditPlaylist());
            this.VMPlaylists.WhenNewRequestToDeletePlaylist.Subscribe(async (o) => await DeletePlaylist());
            this.VMPlaylists.WhenNewRequestToAssignPlaylistToStage.Subscribe(async (o) => await AssignPlaylistToStage());

            ActionExit = ReactiveCommand.Create(OnExit);
            ActionBuild = ReactiveCommand.CreateFromTask(OnBuild);
            ActionBuildNoCache = ReactiveCommand.CreateFromTask(OnBuildNoCache);
            ActionRefreshData = ReactiveCommand.CreateFromTask(OnInitData);
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

        public async Task OnInitData()
        {
            IsLoading = true;
            await _buildDialog.Init((o) =>
            {
                _viewModelManager.Init();
                IsLoading = false;
            }, (o) =>
            {
                OnExit();
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
        #region BGM Operations
        public async Task AddNewBgmEntry(ModEntryViewModel managerMod)
        {
            /*if (managerMod?.MusicMod == null)
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
                }
                //Open ToneIdWindow
                VMToneIdCreation.Filename = inputFile;
                VMToneIdCreation.ToneId = toneId;
                var modalToneIdCreation = new ToneIdCreationModalWindow() { DataContext = VMToneIdCreation };
                var result = await modalToneIdCreation.ShowDialog<ToneIdCreationModalWindow>(_rootDialog.Window);
                if (result != null)
                {
                    toneId = VMToneIdCreation.ToneId;

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
            }*/
        }

        public async Task EditBgmEntry(BgmDbRootEntryViewModel vmBgmEntry)
        {
            if (vmBgmEntry != null)
            {
                BgmEntryViewModel result;
                if (IsAdvanced)
                    result = await _dialogAdvancedBgmEditor.ShowDialog(_rootDialog.Window, new BgmEntryViewModel(vmBgmEntry));
                else
                    result = await _dialogSimpleBgmEditor.ShowDialog(_rootDialog.Window, new BgmEntryViewModel(vmBgmEntry));

                if (result != null)
                    await _guiStateManager.UpdateMusicModEntries(result.GetMusicModEntries(), vmBgmEntry.MusicMod);
            }
        }

        public async Task DeleteBgmEntry(BgmDbRootEntryViewModel vmBgmEntry)
        {
            if (vmBgmEntry != null)
            {
                var result = await _messageDialog.ShowWarningConfirm($"Delete '{vmBgmEntry.Title}'?", "Do you really want to remove this song?\r\nIf it's a Core song, this could cause unknown issues. Prefer hiding the song instead.");

                if (result)
                {
                    if (vmBgmEntry != null)
                    {
                        vmBgmEntry.StopPlay();
                        await Task.Delay(500);

                        _logger.LogInformation("Deleting {ToneId}...", vmBgmEntry.ToneId);

                        //TODO - When supported more complex mods this needs to be updated 
                        //Right now, it is tied to v2 mods
                        var deleteMusicModEntries = new BgmEntryViewModel(vmBgmEntry).GetMusicModDeleteEntries();
                        await _guiStateManager.RemoveMusicModEntries(deleteMusicModEntries);
                        
                        _logger.LogInformation("{ToneId} deleted.", vmBgmEntry.ToneId);
                    }
                }
            }
        }
        #endregion

        #region Game Operations
        public async Task AddNewOrEditGame(Window parent = null, GameTitleEntryViewModel vmGameTitleEntry = null)
        {
            var result = await _dialogGameEditor.ShowDialog(_rootDialog.Window, vmGameTitleEntry);
            if (result != null)
                await _guiStateManager.UpdateGameTitleEntry(result.GetReferenceEntity());
        }

        public async Task EditGame(Window parent = null)
        {
            var modalPickerGame = new GamePickerModalWindow() { DataContext = VMGamePicker };
            var results = await modalPickerGame.ShowDialog<GamePickerModalWindow>(parent ?? _rootDialog.Window);
            if (results != null)
            {
                await AddNewOrEditGame(parent, VMGamePicker.SelectedGameTitleEntry);
            }
        }
        #endregion

        #region Mod Operations
        public async Task AddNewOrEditMod(Window parent = null, ModEntryViewModel vmModEntry = null)
        {
            /*VMModEditor.LoadMusicMod(vmModEntry?.MusicMod);
            var modalCreateMod = new ModPropertiesModalWindow() { DataContext = VMModEditor };
            var results = await modalCreateMod.ShowDialog<ModPropertiesModalWindow>(parent ?? _rootDialog.Window);

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
            }*/
        }

        public async Task EditMod(Window parent = null)
        {
            var modalPickerMod = new ModPickerModalWindow() { DataContext = VMModPicker };
            var results = await modalPickerMod.ShowDialog<ModPickerModalWindow>(parent ?? _rootDialog.Window);
            if (results != null)
            {
                await AddNewOrEditMod(parent, VMModPicker.SelectedModEntry);
            }
        }
        #endregion

        #region Playlists Operations
        public void UpdatePlaylists()
        {
            /*try
            {
                //TODO - The data should be first persisted in the Playlists - currently not mapped back to the original PlaylistEntry
                //TODO - Handle anything saving in a specific service
                var playlists = _playlistsEntries.ToDictionary(p => p.Id, p => p.ToPlaylistEntry());
                _sm5shMusicOverride.UpdatePlaylistConfig(playlists);
            }
            catch (Exception e)
            {
                _logger.LogError("Error while saying playlists. Most likely a concurrency issue.", e.Message);
            }*/
        }

        public async Task AddNewOrEditPlaylist(Window parent = null, PlaylistEntryViewModel vmPlaylist = null)
        {
            /*VMPlaylistEditor.LoadPlaylist(vmPlaylist);
            var modalPickerGame = new PlaylistPropertiesModalWindow() { DataContext = VMPlaylistEditor };
            var results = await modalPickerGame.ShowDialog<PlaylistPropertiesModalWindow>(parent ?? _rootDialog.Window);

            if (results != null)
            {
                if (!VMPlaylistEditor.IsEdit)
                {
                    //TODO - Handle anything saving in a specific service
                    _playlistsEntries.Add(VMPlaylistEditor.SelectedPlaylistEntry);
                }
            }*/
        }

        public async Task EditPlaylist(Window parent = null)
        {
            var modalPickerPlaylist = new PlaylistPickerModalWindow() { DataContext = VMPlaylistPicker };
            var results = await modalPickerPlaylist.ShowDialog<PlaylistPickerModalWindow>(parent ?? _rootDialog.Window);
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
            /*var result = await _messageDialog.ShowWarningConfirm($"Delete Playlist '{vmPlaylistEntry.Title}'?", "Do you really want to remove this playlist?\r\nIf it's a Core playlist, this could cause unknown issues.");
            if (result)
            {
                _playlistsEntries.Remove(vmPlaylistEntry);
                UpdatePlaylists();
            }*/
        }

        public async Task AssignPlaylistToStage()
        {
            /*VMStageAssignement.LoadControl();
            var modalPickerAssignStagePlaylist = new PlaylistStageAssignementModalWindow() { DataContext = VMStageAssignement };
            var results = await modalPickerAssignStagePlaylist.ShowDialog<PlaylistStageAssignementModalWindow>(_rootDialog.Window);
            if (results != null)
            {
                foreach (var vmStage in VMStageAssignement.Stages)
                    _mapper.Map(vmStage, vmStage.GetStageEntryReference());

                //TODO - Handle anything saving in a specific service
                _sm5shMusicOverride.UpdateMusicStageOverride(_stagesEntries.Select(p => p.GetStageEntryReference()).ToList());
            }*/
        }
        #endregion
        #endregion

        #region Modal Helpers
        #endregion
    }
}
