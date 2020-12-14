using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sm5sh.Mods.Music;
using Sm5sh.Mods.Music.Helpers;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using Sm5shMusic.GUI.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sm5shMusic.GUI.Services
{
    public class GUIStateManager : IGUIStateManager
    {
        private readonly ILogger _logger;
        private readonly IMessageDialog _messageDialog;
        private readonly IAudioStateService _audioState;
        private readonly ISm5shMusicOverride _sm5shMusicOverride;
        private readonly IMusicModManagerService _musicModManagerService;
        private readonly IViewModelManager _viewModelManager;
        private readonly IOptions<ApplicationSettings> _config;

        public GUIStateManager(ISm5shMusicOverride sm5shMusicOverride, IViewModelManager viewModelManager, IAudioStateService audioStateService,
            IOptions<ApplicationSettings> config, IMusicModManagerService musicModManagerService, IMessageDialog messageDialog, ILogger<IGUIStateManager> logger)
        {
            _logger = logger;
            _config = config;
            _messageDialog = messageDialog;
            _sm5shMusicOverride = sm5shMusicOverride;
            _audioState = audioStateService;
            _musicModManagerService = musicModManagerService;
            _viewModelManager = viewModelManager;
        }

        #region Music Mod Entries
        public async Task<string> CreateNewMusicModFromToneId(string toneId, string filename, IMusicMod musicMod)
        {
            try
            {
                _logger.LogInformation("Create Music Mod from ToneId: {ToneId}, Filename: {Filename}, Mod: {ModId}, Path: {ModPath}", toneId, filename, musicMod?.Id, musicMod?.ModPath);

                //TODO - "Simple" way to add a song, it should become more flexible
                //Link all items manually
                var newBgmPropertyEntry = new BgmPropertyEntry(toneId, filename, musicMod);
                var newBgmStreamPropertyEntry = new BgmStreamPropertyEntry($"{MusicConstants.InternalIds.STREAM_PREFIX}{toneId}", musicMod) { DataName0 = newBgmPropertyEntry.NameId };
                var newBgmAssignedInfoEntry = new BgmAssignedInfoEntry($"{MusicConstants.InternalIds.INFO_ID_PREFIX}{toneId}", musicMod) { StreamId = newBgmStreamPropertyEntry.StreamId };
                var newBgmStreamSetEntry = new BgmStreamSetEntry($"{MusicConstants.InternalIds.STREAM_SET_PREFIX}{toneId}", musicMod) { Info0 = newBgmAssignedInfoEntry.InfoId };
                var newBgmDbRootEntry = new BgmDbRootEntry($"{MusicConstants.InternalIds.UI_BGM_ID_PREFIX}{toneId}", musicMod) { StreamSetId = newBgmStreamSetEntry.StreamSetId };

                //Create mod
                var musicModEntries = new MusicModEntries();
                musicModEntries.BgmDbRootEntries.Add(newBgmDbRootEntry);
                musicModEntries.BgmStreamSetEntries.Add(newBgmStreamSetEntry);
                musicModEntries.BgmAssignedInfoEntries.Add(newBgmAssignedInfoEntry);
                musicModEntries.BgmStreamPropertyEntries.Add(newBgmStreamPropertyEntry);
                musicModEntries.BgmPropertyEntries.Add(newBgmPropertyEntry);

                return await CreateNewMusicMod(musicModEntries, musicMod);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while creating new song entry");
            }

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await _messageDialog.ShowError("Create Song Entry Error", "There was an error while creating a song entry. Please check the logs.");
            }, DispatcherPriority.Background);

            return string.Empty;
        }

        public async Task<string> CreateNewMusicMod(MusicModEntries musicModEntries, IMusicMod musicMod)
        {
            //TODO - Need to be more flexible. It should be possible to add multiple entries for each db

            try
            {
                _logger.LogInformation("Create Music Mod Entries: Game: {Game}, DbRoot: {DbRoot}, StreamSet: {StreamSet}, AssignedInfo: {AssignedInfo}, StreamProperty: {StreamProperty}, BgmProperty: {BgmProperty}, Mod: {ModId}",
                        musicModEntries.GameTitleEntries.Count, musicModEntries.BgmDbRootEntries.Count, musicModEntries.BgmStreamSetEntries.Count,
                        musicModEntries.BgmAssignedInfoEntries.Count, musicModEntries.BgmStreamPropertyEntries.Count, musicModEntries.BgmPropertyEntries.Count, musicMod?.Id);

                //Check if exists
                if (!await CanAddMusicModEntries(musicModEntries))
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await _messageDialog.ShowError("Error", $"At least one entry cannot be added to the DB.\r\nMake sure to pick a unique ID.");
                    }, DispatcherPriority.Background);

                    return string.Empty;
                }

                // Add to mod
                if (!await PersistMusicModEntryChanges(musicModEntries, musicMod))
                {
                    throw new Exception($"At least one entry could not be added to the mod.\r\nPlease check the logs.");
                }

                // Add to DB
                if (!await AddMusicModEntries(musicModEntries))
                {
                    throw new Exception($"At least one entry could not be added to the DB.\r\nPlease check the logs.");
                }

                return musicModEntries.BgmDbRootEntries.FirstOrDefault().UiBgmId;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while creating new song entry");
            }

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await _messageDialog.ShowError("Create Song Entry Error", "There was an error while creating a song entry. Please check the logs.");
            }, DispatcherPriority.Background);

            return string.Empty;
        }

        public async Task<bool> CanAddMusicModEntries(MusicModEntries musicModEntries)
        {
            try
            {
                _logger.LogInformation("Test whether the Music Mod Entries can be added to the DB");

                foreach (var bgmDbRootEntry in musicModEntries.BgmDbRootEntries)
                {
                    if (!_audioState.CanAddBgmDbRootEntry(bgmDbRootEntry.UiBgmId))
                    {
                        _logger.LogError("DBRoot ID {BgmDbRootId} was found in the database", bgmDbRootEntry.UiBgmId);
                        throw new Exception("DbRoot ID already exists");
                    }
                }

                foreach (var bgmStreamSetEntry in musicModEntries.BgmStreamSetEntries)
                {
                    if (!_audioState.CanAddBgmStreamSetEntry(bgmStreamSetEntry.StreamSetId))
                    {
                        _logger.LogError("StreamSet ID {BgmStreamSetId} was found in the database", bgmStreamSetEntry.StreamSetId);
                        throw new Exception("StreamSet ID already exists");
                    }
                }

                foreach (var bgmAssignedInfoEntry in musicModEntries.BgmAssignedInfoEntries)
                {
                    if (!_audioState.CanAddBgmAssignedInfoEntry(bgmAssignedInfoEntry.InfoId))
                    {
                        _logger.LogError("Info ID {BgmInfoId} was found in the database", bgmAssignedInfoEntry.InfoId);
                        throw new Exception("Info ID already exists");
                    }
                }

                foreach (var bgmStreamPropertyEntry in musicModEntries.BgmStreamPropertyEntries)
                {
                    if (!_audioState.CanAddBgmStreamPropertyEntry(bgmStreamPropertyEntry.StreamId))
                    {
                        _logger.LogError("Stream ID {BgmStreamId} was found in the database", bgmStreamPropertyEntry.StreamId);
                        throw new Exception("Stream ID already exists");
                    }
                }

                foreach (var bgmPropertyEntry in musicModEntries.BgmPropertyEntries)
                {
                    if (!_audioState.CanAddBgmPropertyEntry(bgmPropertyEntry.NameId))
                    {
                        _logger.LogError("Name ID {NameId} was found in the database", bgmPropertyEntry.NameId);
                        throw new Exception("Name ID already exists");
                    }
                }

                foreach (var gameTitleEntry in musicModEntries.GameTitleEntries)
                {
                    if (!_audioState.CanAddGameTitleEntry(gameTitleEntry.UiGameTitleId))
                    {
                        _logger.LogError("GameTitle ID {GameTitleId} was found in the database", gameTitleEntry.UiGameTitleId);
                        throw new Exception("GameTitle ID already exists");
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in CanAddMusicModEntries");
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowError("Check Audio State DB", "One or more elements of this mod cannot be added to the DB. \r\nPlease check the logs.");
                }, DispatcherPriority.Background);
            }
            return false;
        }

        private async Task<bool> AddMusicModEntries(MusicModEntries musicModEntries)
        {
            try
            {
                foreach (var gameTitleEntry in musicModEntries.GameTitleEntries)
                {
                    _logger.LogDebug("Adding GameTitle {GameTitleId}, Mod: {ModId}, Source: {Source}", gameTitleEntry?.UiGameTitleId, gameTitleEntry?.ModId, gameTitleEntry?.Source);
                    if (string.IsNullOrEmpty(await CreateNewGameTitleEntry(gameTitleEntry)))
                    {
                        throw new Exception("GameTitle Add Error");
                    }
                }

                foreach (var bgmPropertyEntry in musicModEntries.BgmPropertyEntries)
                {
                    _logger.LogDebug("Adding BgmProperty {BgmPropertyId}, Mod: {ModId}, Source: {Source}", bgmPropertyEntry?.NameId, bgmPropertyEntry?.ModId, bgmPropertyEntry?.Source);
                    if (!_audioState.AddBgmPropertyEntry(bgmPropertyEntry) ||
                        !_viewModelManager.AddNewBgmPropertyEntryViewModel(bgmPropertyEntry))
                    {
                        throw new Exception("Name Add Error");
                    }
                }

                foreach (var bgmStreamPropertyEntry in musicModEntries.BgmStreamPropertyEntries)
                {
                    _logger.LogDebug("Adding StreamProperty {StreamPropertyId}, Mod: {ModId}, Source: {Source}", bgmStreamPropertyEntry?.StreamId, bgmStreamPropertyEntry?.ModId, bgmStreamPropertyEntry?.Source);
                    if (!_audioState.AddBgmStreamPropertyEntry(bgmStreamPropertyEntry) ||
                        !_viewModelManager.AddNewBgmStreamPropertyEntryViewModel(bgmStreamPropertyEntry))
                    {
                        throw new Exception("Stream Add Error");
                    }
                }

                foreach (var bgmAssignedInfoEntry in musicModEntries.BgmAssignedInfoEntries)
                {
                    _logger.LogDebug("Adding AssignedInfo {AssignedInfoId}, Mod: {ModId}, Source: {Source}", bgmAssignedInfoEntry?.InfoId, bgmAssignedInfoEntry?.ModId, bgmAssignedInfoEntry?.Source);
                    if (!_audioState.AddBgmAssignedInfoEntry(bgmAssignedInfoEntry) ||
                        !_viewModelManager.AddNewBgmAssignedInfoEntryViewModel(bgmAssignedInfoEntry))
                    {
                        throw new Exception("Info Add Error");
                    }
                }

                foreach (var bgmStreamSetEntry in musicModEntries.BgmStreamSetEntries)
                {
                    _logger.LogDebug("Adding StreamSet {StreamSetId}, Mod: {ModId}, Source: {Source}", bgmStreamSetEntry?.StreamSetId, bgmStreamSetEntry?.ModId, bgmStreamSetEntry?.Source);
                    if (!_audioState.AddBgmStreamSetEntry(bgmStreamSetEntry) ||
                        !_viewModelManager.AddNewBgmStreamSetEntryViewModel(bgmStreamSetEntry))
                    {
                        throw new Exception("StreamSet Add Error");
                    }
                }

                foreach (var bgmDbRootEntry in musicModEntries.BgmDbRootEntries)
                {
                    _logger.LogDebug("Adding DBRoot {DBRootId}, Mod: {ModId}, Source: {Source}", bgmDbRootEntry?.UiBgmId, bgmDbRootEntry?.ModId, bgmDbRootEntry?.Source);
                    if (!_audioState.AddBgmDbRootEntry(bgmDbRootEntry) ||
                        !_viewModelManager.AddNewBgmDbRootEntryViewModel(bgmDbRootEntry))
                    {
                        throw new Exception("DbRoot Add Error");
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while adding music mod entries");
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowError("Add Music Mod Entries", "One or more elements of this mod cannot be added to the DB. \r\nPlease check the logs.");
                }, DispatcherPriority.Background);
            }

            return false;
        }

        public async Task<bool> PersistMusicModEntryChanges(MusicModEntries musicModEntries, IMusicMod musicMod = null)
        {
            //TODO In the future IMusicMod should not be passed, instead rely on the MusicMod from each item

            bool result = false;
            if (musicModEntries != null)
            {
                try
                {
                    _logger.LogInformation("Update Music Mod Entries: Game: {Game}, DbRoot: {DbRoot}, StreamSet: {StreamSet}, AssignedInfo: {AssignedInfo}, StreamProperty: {StreamProperty}, BgmProperty: {BgmProperty}, Mod: {ModId}",
                        musicModEntries.GameTitleEntries.Count, musicModEntries.BgmDbRootEntries.Count, musicModEntries.BgmStreamSetEntries.Count,
                        musicModEntries.BgmAssignedInfoEntries.Count, musicModEntries.BgmStreamPropertyEntries.Count, musicModEntries.BgmPropertyEntries.Count, musicMod?.Id);

                    if (musicMod != null)
                        result = await musicMod.AddOrUpdateMusicModEntries(musicModEntries);
                    else
                        result = _sm5shMusicOverride.UpdateCoreBgmEntries(musicModEntries);

                    if(result)
                        await ReorderSongs();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while updating music mod entries");
                    result = false;
                }
            }

            if (!result)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowError("Update Music Mod Entries Error", "There was an error while persisting some modifications. Please check the logs.");
                }, DispatcherPriority.Background);
            }

            return result;
        }

        public async Task<bool> RemoveMusicModEntries(MusicModDeleteEntries musicModDeleteEntries, IMusicMod musicMod = null)
        {
            //TODO In the future IMusicMod should not be passed, instead rely on the MusicMod from each item

            bool result = false;
            if (musicModDeleteEntries != null)
            {
                try
                {
                    _logger.LogInformation("Delete Music Mod Entries: Game: {Game}, DbRoot: {DbRoot}, StreamSet: {StreamSet}, AssignedInfo: {AssignedInfo}, StreamProperty: {StreamProperty}, BgmProperty: {BgmProperty}",
                        musicModDeleteEntries.GameTitleEntries.Count, musicModDeleteEntries.BgmDbRootEntries.Count, musicModDeleteEntries.BgmStreamSetEntries.Count,
                        musicModDeleteEntries.BgmAssignedInfoEntries.Count, musicModDeleteEntries.BgmStreamPropertyEntries.Count, musicModDeleteEntries.BgmPropertyEntries.Count);

                    if (musicMod != null)
                        result = musicMod.RemoveMusicModEntries(musicModDeleteEntries);
                    else
                        _logger.LogError("Deleting Core resources is not supported at this time.");
                    //    result = _sm5shMusicOverride.UpdateCoreBgmEntries(musicModEntries); //Not supported for now.

                    if (result)
                    {
                        foreach (var gameTitle in musicModDeleteEntries.GameTitleEntries)
                        {
                            _logger.LogInformation("Deleting {GameTitle} from AudioState service", gameTitle);
                            _audioState.RemoveGameTitleEntry(gameTitle);
                            _viewModelManager.RemoveGameTitleView(gameTitle);
                        }

                        foreach (var dbRootEntry in musicModDeleteEntries.BgmDbRootEntries)
                        {
                            _logger.LogInformation("Deleting {DbRoot} from AudioState service", dbRootEntry);
                            _audioState.RemoveBgmDbRootEntry(dbRootEntry);
                            _viewModelManager.RemoveBgmDbRootView(dbRootEntry);
                            _viewModelManager.RemoveBgmInAllPlaylists(dbRootEntry);
                        }

                        foreach (var streamSetEntry in musicModDeleteEntries.BgmStreamSetEntries)
                        {
                            _logger.LogInformation("Deleting {StreamSet} from AudioState service", streamSetEntry);
                            _audioState.RemoveBgmStreamSetEntry(streamSetEntry);
                            _viewModelManager.RemoveBgmStreamSetView(streamSetEntry);
                        }

                        foreach (var assignedInfoEntry in musicModDeleteEntries.BgmAssignedInfoEntries)
                        {
                            _logger.LogInformation("Deleting {AssignedInfo} from AudioState service", assignedInfoEntry);
                            _audioState.RemoveBgmAssignedInfoEntry(assignedInfoEntry);
                            _viewModelManager.RemoveBgmAssignedInfoView(assignedInfoEntry);
                        }

                        foreach (var streamPropertyEntry in musicModDeleteEntries.BgmStreamPropertyEntries)
                        {
                            _logger.LogInformation("Deleting {StreamProperty} from AudioState service", streamPropertyEntry);
                            _audioState.RemoveBgmStreamPropertyEntry(streamPropertyEntry);
                            _viewModelManager.RemoveBgmStreamPropertyView(streamPropertyEntry);
                        }

                        foreach (var bmPropertyEntry in musicModDeleteEntries.BgmPropertyEntries)
                        {
                            _logger.LogInformation("Deleting {BgmProperty} from AudioState service", bmPropertyEntry);
                            _audioState.RemoveBgmPropertyEntry(bmPropertyEntry);
                            _viewModelManager.RemoveBgmPropertyView(bmPropertyEntry);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while deleting music mod entries");
                    result = false;
                }
            }

            if (!result)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowError("Delete Music Mod Entries Error", "There was an error while persisting some modifications. Please check the logs.");
                }, DispatcherPriority.Background);
            }

            return result;
        }
        #endregion

        #region Game
        public async Task<string> CreateNewGameTitleEntry(GameTitleEntry gameTitleEntry)
        {
            bool result = false;

            try
            {
                _logger.LogInformation("Create New Game Title {GameTitleId}, Mod: {ModId}, Source: {Source}", gameTitleEntry?.UiGameTitleId, gameTitleEntry?.ModId, gameTitleEntry?.Source);

                if (_audioState.AddGameTitleEntry(gameTitleEntry))
                {
                    result = _viewModelManager.AddNewGameTitleEntryViewModel(gameTitleEntry);
                }
                await PersistGameTitleEntryChange(gameTitleEntry);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while creating new game entry");
                result = false;
            }

            if (!result)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowError("Create Game Title Entry Error", "There was an error while creating a game entry. Please check the logs.");
                }, DispatcherPriority.Background);
            }

            return gameTitleEntry.UiGameTitleId;
        }

        public async Task<bool> PersistGameTitleEntryChange(GameTitleEntry gameTitleEntry)
        {
            bool result;
            try
            {
                _logger.LogInformation("Persist Game Title Changes for {GameTitleId}, Mod: {ModId}, Source: {Source}", gameTitleEntry?.UiGameTitleId, gameTitleEntry?.ModId, gameTitleEntry?.Source);

                if (gameTitleEntry.Source == EntrySource.Mod)
                {
                    result = await _musicModManagerService.UpdateGameEntry(gameTitleEntry);
                }
                else
                {
                    result = _sm5shMusicOverride.UpdateCoreGameTitleEntry(gameTitleEntry);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while updating game entry");
                result = false;
            }

            if (!result)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowError("Update Game Title Entry Error", "There was an error while persisting a game entry. Please check the logs.");
                }, DispatcherPriority.Background);
            }

            return result;
        }
        #endregion

        #region Mod
        public async Task<string> CreateNewModEntry(MusicModInformation musicModInformation, string modPath)
        {
            bool result = false;

            IMusicMod newManagerMod = null;
            try
            {
                _logger.LogInformation("Create New Mod {ModName}, {ModPath}", musicModInformation?.Name, modPath);

                newManagerMod = _musicModManagerService.AddMusicMod(musicModInformation, modPath);

                if (newManagerMod != null)
                {
                    result = _viewModelManager.AddNewModEntryViewModel(newManagerMod);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while creating new mod entry");
                result = false;
            }

            if (!result)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowError("Create Mod Entry Error", "There was an error while creating a mod entry. Please check the logs.");
                }, DispatcherPriority.Background);
            }

            return newManagerMod?.Id;
        }

        public async Task<bool> PersistModInformationChange(IMusicMod musicMod, MusicModInformation musicModInformation)
        {
            bool result;

            try
            {
                _logger.LogInformation("Persist Mod Information Changes for {ModId}, {ModName}, {ModPath}", musicMod?.Id, musicModInformation?.Name, musicMod?.ModPath);

                result = musicMod.UpdateModInformation(musicModInformation);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while updating mod entry");
                result = false;
            }

            if (!result)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowError("Update Mod Entry Error", "There was an error while persisting mod information. Please check the logs.");
                }, DispatcherPriority.Background);
            }

            return result;
        }
        #endregion

        #region Playlists
        public async Task<string> CreateNewPlaylist(PlaylistEntry playlistEntry)
        {
            bool result = false;

            try
            {
                _logger.LogInformation("Create New Playlist {PlaylistId}", playlistEntry?.Id);

                if (_audioState.AddPlaylistEntry(playlistEntry))
                {
                    result = _viewModelManager.AddNewPlaylistEntryViewModel(playlistEntry);
                }
                await PersistPlaylistChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while creating new playlist entry");
                result = false;
            }

            if (!result)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowError("Create Playlist Entry Error", "There was an error while creating a playlist entry. Please check the logs.");
                }, DispatcherPriority.Background);
            }

            return playlistEntry?.Id;
        }

        public async Task<bool> PersistPlaylistChanges()
        {
            bool result = false;

            try
            {
                _logger.LogInformation("Persist Playlist Changes");


                var playlists = _viewModelManager.GetPlaylistsEntriesViewModels().ToDictionary(p => p.Id, p => p.ToPlaylistEntry());
                result = _sm5shMusicOverride.UpdatePlaylistConfig(playlists);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while updating playlists");
                result = false;
            }

            if (!result)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowError("Update Playlists Error", "There was an error while updating playlists. Please check the logs.");
                }, DispatcherPriority.Background);
            }

            return result;
        }

        public async Task<bool> RemovePlaylist(string playlistId)
        {
            bool result = false;

            try
            {
                _logger.LogInformation("Remove Playlist {PlaylistId}", playlistId);

                result = _audioState.RemovePlaylistEntry(playlistId);
                if (result)
                {
                    _viewModelManager.RemovePlaylist(playlistId);
                }
                await PersistPlaylistChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while deleting playlist");
                result = false;
            }

            if (!result)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowError("Delete Playlist Error", "There was an error while deleting a playlist. Please check the logs.");
                }, DispatcherPriority.Background);
            }

            return result;
        }

        public async Task<bool> PersistStageChanges()
        {
            bool result = false;

            try
            {
                _logger.LogInformation("Persist Stage Changes");

                var stages = _viewModelManager.GetStagesEntriesViewModels().Select(p => p.GetStageEntryReference()).ToList();
                result = _sm5shMusicOverride.UpdateMusicStageOverride(stages);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while updating stages");
                result = false;
            }

            if (!result)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowError("Update Stage Error", "There was an error while updating stages. Please check the logs.");
                }, DispatcherPriority.Background);
            }

            return result;
        }
        #endregion

        public async Task<bool> UpdateGlobalSettings(ApplicationSettings appSettings)
        {
            bool result;

            try
            {
                var settings = JsonConvert.SerializeObject(appSettings, Formatting.Indented);
                File.WriteAllText("appsettings.json", settings);
                result = true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while updating global settings");
                result = false;
            }

            if (!result)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowError("Update Global Settings", "There was an error while updating appsettings.json. Please check the logs.");
                }, DispatcherPriority.Background);
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowInformation("Update Global Settings", "Your settings were saved. Please restart the application.");
                }, DispatcherPriority.Background);
            }

            return result;
        }

        public async Task<bool> WipeAudioCache()
        {
            bool result;

            try
            {
                if (await _messageDialog.ShowWarningConfirm("Wipe Audio Cache", "Are you sure you want to delete the audio cache?"))
                {
                    if (Directory.Exists(_config.Value.Sm5shMusic.CachePath))
                    {
                        var existingFiles = Directory.GetFiles(_config.Value.Sm5shMusic.CachePath, "*.nus3audio", SearchOption.AllDirectories);
                        foreach (var fileToDelete in existingFiles)
                        {
                            File.Delete(fileToDelete);
                        }
                    }
                    result = true;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while deleting audio cache");
                result = false;
            }

            if (!result)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowError("Wipe Audio Cache", "There was an error while deleting audio cache. Please check the logs.");
                }, DispatcherPriority.Background);
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowInformation("Wipe Audio Cache", "Success!");
                }, DispatcherPriority.Background);
            }

            return result;
        }

        public async Task<bool> ReorderSongs()
        {
            bool result;

            try
            {
                _viewModelManager.ReorderSongs();
                result = _sm5shMusicOverride.UpdateSoundTestOrderConfig(_viewModelManager.GetBgmDbRootEntriesViewModels().ToDictionary(p => p.UiBgmId, p => p.TestDispOrder));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while updating music mod entries");
                result = false;
            }

            if (!result)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowError("Update Tracks order", "There was an error while persisting some modifications. Please check the logs.");
                }, DispatcherPriority.Background);
            }

            return result;
        }
    }
}
