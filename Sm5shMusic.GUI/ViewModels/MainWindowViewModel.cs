using AutoMapper;
using Avalonia.Controls;
using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sm5sh.Mods.Music.Interfaces;
using Sm5shMusic.GUI.Dialogs;
using Sm5shMusic.GUI.Helpers;
using Sm5shMusic.GUI.Interfaces;
using Sm5shMusic.GUI.Views;
using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using VGMMusic;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Microsoft.Extensions.Options;
using Sm5sh.Mods.Music;

namespace Sm5shMusic.GUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IGUIStateManager _guiStateManager;
        private readonly IViewModelManager _viewModelManager;
        private readonly IVGMMusicPlayer _musicPlayer;
        private readonly IMessageDialog _messageDialog;
        private readonly IFileDialog _fileDialog;
        private readonly IDialogWindow _rootDialog;
        private readonly IBuildDialog _buildDialog;
        private readonly IOptions<ApplicationSettings> _appSettings;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private string _currentLocale;

        private readonly ModalDialog<BgmPropertiesModalWindow, BgmPropertiesModalWindowViewModel, BgmEntryViewModel> _dialogSimpleBgmEditor;
        private readonly ModalDialog<BgmAdvancedPropertiesModalWindow, BgmPropertiesModalWindowViewModel, BgmEntryViewModel> _dialogAdvancedBgmEditor;
        private readonly ModalDialog<GamePropertiesModalWindow, GamePropertiesModalWindowViewModel, GameTitleEntryViewModel> _dialogGameEditor;
        private readonly ModalDialog<GamePickerModalWindow, GamePickerModalWindowViewModel, GameTitleEntryViewModel> _dialogGamePicker;
        private readonly ModalDialog<ModPropertiesModalWindow, ModPropertiesModalWindowViewModel, ModEntryViewModel> _dialogModEditor;
        private readonly ModalDialog<ModPickerModalWindow, ModPickerModalWindowViewModel, ModEntryViewModel> _dialogModPicker;
        private readonly ModalDialog<PlaylistPropertiesModalWindow, PlaylistPropertiesModalWindowViewModel, PlaylistEntryViewModel> _dialogPlaylistEditor;
        private readonly ModalDialog<PlaylistPickerModalWindow, PlaylistPickerModalWindowViewModel, PlaylistEntryViewModel> _dialogPlaylistPicker;
        private readonly ModalDialog<PlaylistDeletePickerModalWindow, PlaylistDeletePickerModalWindowViewModel, PlaylistEntryViewModel> _dialogPlaylistDeletePicker;
        private readonly ModalDialog<GlobalSettingsModalWindow, GlobalSettingsModalWindowViewModel, GlobalConfigurationViewModel> _dialogGlobalSettingsEditor;
        private readonly PlaylistStageAssignementModalWindowViewModel _vmStageAssignement;
        private readonly ToneIdCreationModalWindowModel _vmToneIdCreation;

        [Reactive]
        public bool IsAdvanced { get; set; }
        [Reactive]
        public bool IsLoading { get; set; }
        [Reactive]
        public bool IsShowingDebug { get; set; }

        public BgmSongsViewModel VMBgmSongs { get; }
        public PlaylistViewModel VMPlaylists { get; }
        public BgmFiltersViewModel VMBgmFilters { get; }
        public ContextMenuViewModel VMContextMenu { get; }

        public ReactiveCommand<Unit, Unit> ActionExit { get; }
        public ReactiveCommand<Unit, Unit> ActionBuild { get; }
        public ReactiveCommand<Unit, Unit> ActionBuildNoCache { get; }
        public ReactiveCommand<Unit, Unit> ActionRefreshData { get; }
        public ReactiveCommand<Unit, Unit> ActionToggleAdvanced { get; }
        public ReactiveCommand<Unit, Unit> ActionToggleConsole { get; }
        public ReactiveCommand<Unit, Unit> ActionOpenAbout { get; }
        public ReactiveCommand<Unit, Unit> ActionOpenGlobalSettings { get; }

        public MainWindowViewModel(IServiceProvider serviceProvider, IViewModelManager viewModelManager, IGUIStateManager guiStateManager, IMapper mapper, IVGMMusicPlayer musicPlayer, 
            IDialogWindow rootDialog, IMessageDialog messageDialog, IFileDialog fileDialog, IBuildDialog buildDialog, IOptions<ApplicationSettings> appSettings, ILogger<MainWindowViewModel> logger)
        {
            _viewModelManager = viewModelManager;
            _guiStateManager = guiStateManager;
            _musicPlayer = musicPlayer;
            _fileDialog = fileDialog;
            _buildDialog = buildDialog;
            _messageDialog = messageDialog;
            _rootDialog = rootDialog;
            _logger = logger;
            _mapper = mapper;
            _appSettings = appSettings;
            IsLoading = true;

            _logger.LogInformation($"GUI Version: {Constants.GUIVersion}");

            //Set values
            IsAdvanced = appSettings.Value.Sm5shMusicGUI.Advanced;

            //Set global settings
            var vmGlobalSettings = new GlobalSettingsModalWindowViewModel(_guiStateManager, fileDialog);
            _dialogGlobalSettingsEditor = new ModalDialog<GlobalSettingsModalWindow, GlobalSettingsModalWindowViewModel, GlobalConfigurationViewModel>(vmGlobalSettings);

            //Initialize Contextual Menu view
            VMContextMenu = ActivatorUtilities.CreateInstance<ContextMenuViewModel>(serviceProvider, viewModelManager.ObservableModsEntries, viewModelManager.ObservableLocales);
            VMContextMenu.WhenLocaleChanged.Subscribe((locale) => _currentLocale = locale);
            var observableBgmDbRootEntriesList = viewModelManager.ObservableDbRootEntries
                .DeferUntilLoaded()
                .Filter(p => p.UiBgmId != "ui_bgm_random")
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

            //Setup ModEditor
            var vmModEditor = ActivatorUtilities.CreateInstance<ModPropertiesModalWindowViewModel>(serviceProvider);
            var vmModPicker = ActivatorUtilities.CreateInstance<ModPickerModalWindowViewModel>(serviceProvider,
               viewModelManager.ObservableModsEntries);
            _dialogModEditor = new ModalDialog<ModPropertiesModalWindow, ModPropertiesModalWindowViewModel, ModEntryViewModel>(vmModEditor);
            _dialogModPicker = new ModalDialog<ModPickerModalWindow, ModPickerModalWindowViewModel, ModEntryViewModel>(vmModPicker);

            //Setup GameEditor
            var vmGameEditor = ActivatorUtilities.CreateInstance<GamePropertiesModalWindowViewModel>(serviceProvider,
                viewModelManager.ObservableLocales, viewModelManager.ObservableSeries, observableGameEntriesList);
            var vmGamePicker = ActivatorUtilities.CreateInstance<GamePickerModalWindowViewModel>(serviceProvider,
                observableGameEntriesList);
            _dialogGameEditor = new ModalDialog<GamePropertiesModalWindow, GamePropertiesModalWindowViewModel, GameTitleEntryViewModel>(vmGameEditor);
            _dialogGamePicker = new ModalDialog<GamePickerModalWindow, GamePickerModalWindowViewModel, GameTitleEntryViewModel>(vmGamePicker);
            _vmToneIdCreation = ActivatorUtilities.CreateInstance<ToneIdCreationModalWindowModel>(serviceProvider,
                viewModelManager.ObservableBgmPropertyEntries);

            //Setup BgmEditor
            var vmBgmEditor = ActivatorUtilities.CreateInstance<BgmPropertiesModalWindowViewModel>(serviceProvider,
                viewModelManager.ObservableLocales, viewModelManager.ObservableSeries, observableGameEntriesList, viewModelManager.ObservableAssignedInfoEntries);
            vmBgmEditor.VMGamePropertiesModal = vmGameEditor;
            vmBgmEditor.WhenNewRequestToAddGameEntry.Subscribe(async (o) => await AddNewOrEditGame(o));
            _dialogSimpleBgmEditor = new ModalDialog<BgmPropertiesModalWindow, BgmPropertiesModalWindowViewModel, BgmEntryViewModel>(vmBgmEditor);
            _dialogAdvancedBgmEditor = new ModalDialog<BgmAdvancedPropertiesModalWindow, BgmPropertiesModalWindowViewModel, BgmEntryViewModel>(vmBgmEditor);

            //Setup PlaylistControls
            var vmPlaylistPicker = ActivatorUtilities.CreateInstance<PlaylistPickerModalWindowViewModel>(serviceProvider,
                viewModelManager.ObservablePlaylistsEntries);
            var vmPlaylistDeletePicker = ActivatorUtilities.CreateInstance<PlaylistDeletePickerModalWindowViewModel>(serviceProvider,
                viewModelManager.ObservablePlaylistsEntries);
            _dialogPlaylistPicker = new ModalDialog<PlaylistPickerModalWindow, PlaylistPickerModalWindowViewModel, PlaylistEntryViewModel>(vmPlaylistPicker);
            _dialogPlaylistDeletePicker = new ModalDialog<PlaylistDeletePickerModalWindow, PlaylistDeletePickerModalWindowViewModel, PlaylistEntryViewModel>(vmPlaylistDeletePicker);
            var vmPlaylistEditor = ActivatorUtilities.CreateInstance<PlaylistPropertiesModalWindowViewModel>(serviceProvider,
               viewModelManager.ObservablePlaylistsEntries);
            _dialogPlaylistEditor = new ModalDialog<PlaylistPropertiesModalWindow, PlaylistPropertiesModalWindowViewModel, PlaylistEntryViewModel>(vmPlaylistEditor);
            _vmStageAssignement = ActivatorUtilities.CreateInstance<PlaylistStageAssignementModalWindowViewModel>(serviceProvider,
                viewModelManager.ObservablePlaylistsEntries, viewModelManager.ObservableStagesEntries);

            //Listen to requests from children
            VMContextMenu.WhenNewRequestToAddBgmEntry.Subscribe(async (o) => await AddNewBgmEntry(o));
            VMContextMenu.WhenNewRequestToAddModEntry.Subscribe(async (o) => await AddNewOrEditMod());
            VMContextMenu.WhenNewRequestToAddGameEntry.Subscribe(async (o) => await AddNewOrEditGame());
            VMContextMenu.WhenNewRequestToEditGameEntry.Subscribe(async (o) => await EditGame());
            VMContextMenu.WhenNewRequestToEditModEntry.Subscribe(async (o) => await EditMod());
            VMBgmSongs.WhenNewRequestToEditBgmEntry.Subscribe(async (o) => await EditBgmEntry(o));
            VMBgmSongs.WhenNewRequestToDeleteBgmEntry.Subscribe(async (o) => await DeleteBgmEntry(o));
            VMBgmSongs.WhenNewRequestToReorderBgmEntries.Subscribe(async (o) => await _guiStateManager.ReorderSongs());
            VMPlaylists.WhenNewRequestToUpdatePlaylists.Subscribe(async (o) => await _guiStateManager.PersistPlaylistChanges());
            VMPlaylists.WhenNewRequestToCreatePlaylist.Subscribe(async (o) => await AddNewOrEditPlaylist());
            VMPlaylists.WhenNewRequestToEditPlaylist.Subscribe(async (o) => await EditPlaylist());
            VMPlaylists.WhenNewRequestToDeletePlaylist.Subscribe(async (o) => await DeletePlaylist());
            VMPlaylists.WhenNewRequestToAssignPlaylistToStage.Subscribe(async (o) => await AssignPlaylistToStage());

            ActionExit = ReactiveCommand.Create(OnExit);
            ActionBuild = ReactiveCommand.CreateFromTask(OnBuild);
            ActionBuildNoCache = ReactiveCommand.CreateFromTask(OnBuildNoCache);
            ActionRefreshData = ReactiveCommand.CreateFromTask(OnInitData);
            ActionToggleAdvanced = ReactiveCommand.Create(OnAdvancedToggle);
            ActionToggleConsole = ReactiveCommand.Create(OnConsoleToggle);
            ActionOpenAbout = ReactiveCommand.CreateFromTask(OnAboutOpen);
            ActionOpenGlobalSettings = ReactiveCommand.CreateFromTask(OnGlobalSettingsOpen);
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
                $"Sm5sh Music - v{Constants.GUIVersion} by deinonychus71\r\n\r\n\r\n" +
                "Research: soneek\r\n\r\n" +
                "Testing: Demonslayerx8, Segtendo\r\n\r\n" +
                "prcEditor: https://github.com/BenHall-7/paracobNET \r\nBenHall-7\r\n\r\n" +
                "paramLabels: https://github.com/ultimate-research/param-labels \r\nBenHall-7, jam1garner, Dr-HyperCake, Birdwards, ThatNintendoNerd, ScanMountGoat, Meshima, Blazingflare, TheSmartKid, jugeeya, Demonslayerx8\r\n\r\n" +
                "msbtEditor: https://github.com/IcySon55/3DLandMSBTeditor \r\nIcySon55, exelix11 \r\n\r\n" +
                "nus3audio: https://github.com/jam1garner/nus3audio-rs \r\njam1garner \r\n\r\n" +
                "bgm-property: https://github.com/jam1garner/smash-bgm-property \r\njam1garner \r\n\r\n" +
                "VGAudio: https://github.com/Thealexbarney/VGAudio \r\nThealexbarney, soneek, jam1garner, devlead, Raytwo, nnn1590\r\n\r\n" +
                "vgmstream: https://github.com/vgmstream/vgmstream \r\nbnnm, kode54, NicknineTheEagle, bxaimc, Thealexbarney\r\nAll contributors: https://github.com/vgmstream/vgmstream/graphs/contributors \r\n\r\n" +
                "CrossArc: https://github.com/Ploaj/ArcCross \r\nPloaj, ScanMountGoat, BenHall-7, shadowninja108, jam1garner, M-1-RLG\r\n\r\n");
        }

        public async Task OnGlobalSettingsOpen()
        {
            var vmGlobalSettings = new GlobalConfigurationViewModel(_mapper, _appSettings.Value);
            var result = await _dialogGlobalSettingsEditor.ShowDialog(_rootDialog.Window, new GlobalConfigurationViewModel(_mapper, _appSettings.Value));
            if(result != null)
            {
                await _guiStateManager.UpdateGlobalSettings(vmGlobalSettings.GetReference());
                IsAdvanced = result.Advanced;
            }
        }

        public void OnAdvancedToggle()
        {
            IsAdvanced = !IsAdvanced;
        }

        public void OnConsoleToggle()
        {
            IsShowingDebug = !IsShowingDebug;
        }
        #endregion

        #region Data Operations
        #region BGM Operations
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
                _vmToneIdCreation.Filename = inputFile;
                _vmToneIdCreation.LoadToneId(Path.GetFileNameWithoutExtension(inputFile));
                var modalToneIdCreation = new ToneIdCreationModalWindow() { DataContext = _vmToneIdCreation };
                var result = await modalToneIdCreation.ShowDialog<ToneIdCreationModalWindow>(_rootDialog.Window);
                if (result != null)
                {
                    string toneId = _vmToneIdCreation.ToneId;

                    var uiBgmId = await _guiStateManager.CreateNewMusicModFromToneId(toneId, inputFile, managerMod.MusicMod);
                    if (!string.IsNullOrEmpty(uiBgmId))
                    {
                        var vmBgmDbRootEntry = _viewModelManager.GetBgmDbRootViewModel(uiBgmId);
                        await EditBgmEntry(vmBgmDbRootEntry);
                    }
                }
            }
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
                    await _guiStateManager.PersistMusicModEntryChanges(result.GetMusicModEntries(), result.MusicMod);
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
                        await Task.Delay(300);

                        _logger.LogInformation("Deleting {ToneId}...", vmBgmEntry.ToneId);

                        //TODO - When supported more complex mods this needs to be updated 
                        //Right now, it is tied to v2 mods
                        var deleteMusicModEntries = new BgmEntryViewModel(vmBgmEntry).GetMusicModDeleteEntries();
                        await _guiStateManager.RemoveMusicModEntries(deleteMusicModEntries, vmBgmEntry.MusicMod);

                        _logger.LogInformation("{ToneId} deleted.", vmBgmEntry.ToneId);
                    }
                }
            }
        }
        #endregion

        #region Game Operations
        public async Task AddNewOrEditGame(Window parent = null, GameTitleEntryViewModel vmGameTitleEntry = null)
        {
            var result = await _dialogGameEditor.ShowDialog(parent ?? _rootDialog.Window, vmGameTitleEntry);
            if (result != null)
                await _guiStateManager.PersistGameTitleEntryChange(result.GetReferenceEntity());
        }

        public async Task EditGame(Window parent = null)
        {
            var result = await _dialogGamePicker.ShowPickerDialog(parent ?? _rootDialog.Window);
            if (result != null)
                await AddNewOrEditGame(parent, result);
        }
        #endregion

        #region Mod Operations
        public async Task AddNewOrEditMod(Window parent = null, ModEntryViewModel vmModEntry = null)
        {
            var result = await _dialogModEditor.ShowDialog(parent ?? _rootDialog.Window, vmModEntry);
            if (result != null)
                await _guiStateManager.PersistModInformationChange(result.MusicMod, result.GetMusicModInformation());
        }

        public async Task EditMod(Window parent = null)
        {
            var result = await _dialogModPicker.ShowPickerDialog(parent ?? _rootDialog.Window);
            if (result != null)
                await AddNewOrEditMod(parent, result);
        }
        #endregion

        #region Playlists Operations
        public async Task AddNewOrEditPlaylist(Window parent = null, PlaylistEntryViewModel vmPlaylist = null)
        {
            var result = await _dialogPlaylistEditor.ShowDialog(parent ?? _rootDialog.Window, vmPlaylist);
            if (result != null)
                await _guiStateManager.PersistPlaylistChanges();
        }

        public async Task EditPlaylist(Window parent = null)
        {
            var result = await _dialogPlaylistPicker.ShowPickerDialog(parent ?? _rootDialog.Window);
            if (result != null)
                await AddNewOrEditPlaylist(parent, result);
        }

        public async Task DeletePlaylist(Window parent = null)
        {
            var result = await _dialogPlaylistDeletePicker.ShowPickerDialog(parent ?? _rootDialog.Window);
            if (result != null)
            {
                var resultConfirm = await _messageDialog.ShowWarningConfirm($"Delete Playlist '{result.Title}'?", "Do you really want to remove this playlist?\r\nIf it's a Core playlist, this could cause unknown issues.");
                if (resultConfirm)
                    await _guiStateManager.RemovePlaylist(result.Id);
            }
        }

        public async Task AssignPlaylistToStage()
        {
            _vmStageAssignement.LoadControl();
            var modalPickerAssignStagePlaylist = new PlaylistStageAssignementModalWindow() { DataContext = _vmStageAssignement };
            var results = await modalPickerAssignStagePlaylist.ShowDialog<PlaylistStageAssignementModalWindow>(_rootDialog.Window);
            if (results != null)
            {
                foreach (var vmStage in _vmStageAssignement.EditableStages)
                    _mapper.Map(vmStage, vmStage.GetStageEntryReference());

                await _guiStateManager.PersistStageChanges();
            }
        }
        #endregion
        #endregion
    }
}
