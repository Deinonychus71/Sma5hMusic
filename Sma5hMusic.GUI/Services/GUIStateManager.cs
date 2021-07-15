using AutoMapper;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sma5h.Mods.Music;
using Sma5h.Mods.Music.Helpers;
using Sma5h.Mods.Music.Interfaces;
using Sma5h.Mods.Music.Models;
using Sma5h.Mods.Music.Models.PlaylistEntryModels;
using Sma5hMusic.GUI.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sma5hMusic.GUI.Services
{
    public class GUIStateManager : IGUIStateManager
    {
        private readonly ILogger _logger;
        protected readonly IMapper _mapper;
        private readonly IMessageDialog _messageDialog;
        private readonly IAudioStateService _audioState;
        private readonly IAudioMetadataService _audioMetadataService;
        private readonly ISma5hMusicOverride _sma5hMusicOverride;
        private readonly IMusicModManagerService _musicModManagerService;
        private readonly IViewModelManager _viewModelManager;
        private readonly IOptions<ApplicationSettings> _config;

        public GUIStateManager(ISma5hMusicOverride Sma5hMusicOverride, IViewModelManager viewModelManager, IAudioStateService audioStateService, IAudioMetadataService audioMetadataService,
            IOptions<ApplicationSettings> config, IMusicModManagerService musicModManagerService, IMessageDialog messageDialog, IMapper mapper, ILogger<IGUIStateManager> logger)
        {
            _logger = logger;
            _mapper = mapper;
            _config = config;
            _messageDialog = messageDialog;
            _sma5hMusicOverride = Sma5hMusicOverride;
            _audioState = audioStateService;
            _audioMetadataService = audioMetadataService;
            _musicModManagerService = musicModManagerService;
            _viewModelManager = viewModelManager;
        }

        //These functions aren't as flexible as they should be, as they require a set of dbroot/streamset/assignedinfo/streampingproperty/bgmproperty to function properly.
        //Need to figure out a better way to handle these.
        #region Music Mod Entries (Hackish)
        public async Task<string> CreateNewMusicModFromToneId(string toneId, string filename, IMusicMod musicMod)
        {
            try
            {
                _logger.LogInformation("Create Music Mod from ToneId: {ToneId}, Filename: {Filename}, Mod: {ModId}, Path: {ModPath}", toneId, filename, musicMod?.Id, musicMod?.ModPath);

                //TODO - "Simple" way to add a song, it should become more flexible
                //Link all items manually
                var musicModEntries = CreateNewMusicModEntriesSet(toneId, filename, musicMod);
                var newBgmPropertyEntry = musicModEntries.BgmPropertyEntries.FirstOrDefault();

                //Calculate cues
                var audioCuePoints = await UpdateAudioCuePoints(filename);
                if (audioCuePoints == null)
                    throw new Exception();
                _mapper.Map(audioCuePoints, newBgmPropertyEntry);

                //Return
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

        public async Task<bool> RenameMusicModToneId(MusicModEntries musicModEntries, IMusicMod musicMod, string newToneId)
        {
            //TODO - "Simple" way to rename a song, it should become more flexible
            var result = false;
            try
            {
                if (musicModEntries == null ||
                    musicModEntries.BgmDbRootEntries.Count != 1 ||
                    musicModEntries.BgmAssignedInfoEntries.Count != 1 ||
                    musicModEntries.BgmStreamSetEntries.Count != 1 ||
                    musicModEntries.BgmStreamPropertyEntries.Count != 1 ||
                    musicModEntries.BgmPropertyEntries.Count != 1 ||
                    musicMod == null || string.IsNullOrEmpty(newToneId))
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await _messageDialog.ShowError("Error", $"This song cannot be renamed.");
                    }, DispatcherPriority.Background);
                    return false;
                }

                var bgmProperty = musicModEntries.BgmPropertyEntries.FirstOrDefault();
                _logger.LogInformation("Rename Music Mod from ToneId: {OldToneId} to ToneId: {NewToneId}", bgmProperty.NameId, newToneId);
                var newMusicModEntries = DuplicateMusicModEntriesSet(musicModEntries, newToneId, bgmProperty.Filename, musicMod);

                if (newMusicModEntries == null)
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await _messageDialog.ShowError("Renaming Song Tone Id", "There was an error while duplicating the music mod set. Please check the logs.");
                    }, DispatcherPriority.Background);
                    return false;
                }

                //Backup playlist entries
                var uiBgmId = musicModEntries.BgmDbRootEntries.FirstOrDefault().UiBgmId;
                var playlistStates = BackupPlaylistStateFromBgmId(uiBgmId);

                //Create new song
                var createdUiBgmId = await CreateNewMusicMod(newMusicModEntries, musicMod);
                if (!string.IsNullOrEmpty(createdUiBgmId))
                {
                    result = await RemoveMusicModEntries(musicModEntries.GetMusicModDeleteEntries(), musicMod);
                }

                //Restore playlists
                RestorePlaylistStateToBgmId(playlistStates, createdUiBgmId);


                //Save order
                if (result)
                    result = await ReorderSongs();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while renaming Tone Id");
            }

            if (!result)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowError("Renaming Song Tone Id", "There was an error while renaming a Tone Id. Please check the logs.");
                }, DispatcherPriority.Background);
            }

            return result;
        }

        public async Task<bool> MoveMusicModEntrySetToAnotherMod(MusicModEntries musicModEntries, IMusicMod fromMusicMod, IMusicMod toMusicMod)
        {
            var result = false;
            try
            {
                if (musicModEntries == null ||
                        musicModEntries.BgmDbRootEntries.Count != 1 ||
                        musicModEntries.BgmAssignedInfoEntries.Count != 1 ||
                        musicModEntries.BgmStreamSetEntries.Count != 1 ||
                        musicModEntries.BgmStreamPropertyEntries.Count != 1 ||
                        musicModEntries.BgmPropertyEntries.Count != 1 ||
                        musicModEntries.GameTitleEntries.Count != 1 ||
                        fromMusicMod == null || toMusicMod == null)
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await _messageDialog.ShowError("Error", $"This song cannot be moved.");
                    }, DispatcherPriority.Background);
                    return false;
                }

                if (musicModEntries != null && musicModEntries.BgmPropertyEntries != null)
                {
                    var bgmProperty = musicModEntries.BgmPropertyEntries.FirstOrDefault();
                    _logger.LogInformation("Moving {ToneId} to Mod {ModPath}", bgmProperty.NameId, toMusicMod.ModPath);

                    //Check Loop - Trying to avoid issues before they happen
                    foreach (var bgmPropertyEntry in musicModEntries.BgmPropertyEntries)
                    {
                        if (!File.Exists(bgmPropertyEntry.Filename))
                        {
                            await Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                await _messageDialog.ShowError("Move to Mod Error", $"There was an error while moving mod entries to Mod {toMusicMod.ModPath}. The file {bgmPropertyEntry.Filename} was not found.");
                            }, DispatcherPriority.Background);
                            return false;
                        }

                        var newPath = Path.Combine(toMusicMod.ModPath, Path.GetFileName(bgmPropertyEntry.Filename));
                        if (File.Exists(newPath))
                        {
                            await Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                await _messageDialog.ShowError("Move to Mod Error", $"There was an error while moving mod entries to Mod {toMusicMod.ModPath}. The file {bgmPropertyEntry.Filename} already exist in the target location.");
                            }, DispatcherPriority.Background);
                            return false;
                        }
                    }

                    //Copy loop
                    foreach (var bgmPropertyEntry in musicModEntries.BgmPropertyEntries)
                    {
                        try
                        {
                            var newPath = Path.Combine(toMusicMod.ModPath, Path.GetFileName(bgmPropertyEntry.Filename));
                            File.Copy(bgmPropertyEntry.Filename, newPath);
                        }
                        catch (Exception e)
                        {
                            await Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                await _messageDialog.ShowError("Move to Mod Error", $"There was an error while copying file {bgmPropertyEntry.Filename} to Mod {toMusicMod.ModPath}.", e);
                            }, DispatcherPriority.Background);
                            return false;
                        }
                    }

                    //Backup playlist entries
                    var uiBgmId = musicModEntries.BgmDbRootEntries.FirstOrDefault().UiBgmId;
                    var playlistStates = BackupPlaylistStateFromBgmId(uiBgmId);

                    //Delete old media
                    var deleteMusicModMedia = musicModEntries.GetMusicModDeleteEntries();
                    result = await RemoveMusicModEntries(deleteMusicModMedia, fromMusicMod);

                    //Add media to new mod
                    if (result)
                    {
                        //Create the copy
                        var newMusicModEntries = DuplicateMusicModEntriesSet(musicModEntries, bgmProperty.NameId, bgmProperty.Filename, toMusicMod);

                        if (newMusicModEntries == null)
                        {
                            await Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                await _messageDialog.ShowError("Move to Mod Error", "There was an error while duplicating the music mod set. Please check the logs.");
                            }, DispatcherPriority.Background);
                            return false;
                        }

                        //Create new entry
                        var createdUiBgmId = await CreateNewMusicMod(newMusicModEntries, toMusicMod);
                        result = !string.IsNullOrEmpty(createdUiBgmId);

                        //Restore playlists
                        RestorePlaylistStateToBgmId(playlistStates, createdUiBgmId);

                        //Need to call update to add the game title
                        if (result)
                        {
                            var gameTitle = musicModEntries.GameTitleEntries.FirstOrDefault();
                            gameTitle.MusicMod = gameTitle.MusicMod != null ? toMusicMod : null;
                            newMusicModEntries.GameTitleEntries.Add(gameTitle);
                            result = await PersistMusicModEntryChanges(newMusicModEntries, toMusicMod);
                        }

                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while moving a ToneId to {ModPath}", toMusicMod.ModPath);
            }

            if (!result)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowError("Move to Mod Error", $"There was an error while moving the song to mod {toMusicMod.ModPath}. Please check the logs.");
                }, DispatcherPriority.Background);
            }
            return result;

        }
        #endregion

        #region Music Mod Entries
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
                    await _messageDialog.ShowError("Check Audio State DB", "One or more elements of this mod cannot be added to the DB. \r\nPlease check the logs.", e);
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
                    await _messageDialog.ShowError("Add Music Mod Entries", "One or more elements of this mod cannot be added to the DB. \r\nPlease check the logs.", e);
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
                        result = _sma5hMusicOverride.UpdateCoreBgmEntries(musicModEntries);

                    if (result)
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
                    //    result = _sma5hMusicOverride.UpdateCoreBgmEntries(musicModEntries); //Not supported for now.

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

        public async Task<bool> RemoveGameTitleEntry(string gameTitleId)
        {
            bool result = false;

            try
            {
                _logger.LogInformation("Remove Game Title {GameTitleId}", gameTitleId);

                _viewModelManager.RemoveGameTitleView(gameTitleId);
                result = _sma5hMusicOverride.DeleteGameTitleEntry(gameTitleId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while deleting game entry");
                result = false;
            }

            if (!result)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowError("Delete Game Title Entry Error", "There was an error while deleting a game entry. Please check the logs.");
                }, DispatcherPriority.Background);
            }

            return result;
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
                //We save in CoreGameTitle no matter what so it gets loaded
                //else
                //{
                result = _sma5hMusicOverride.UpdateGameTitleEntry(gameTitleEntry);
                //}
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
                result = _sma5hMusicOverride.UpdatePlaylistConfig(playlists);
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
                result = _sma5hMusicOverride.UpdateMusicStageOverride(stages);
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

        private Dictionary<string, List<PlaylistValueEntry>> BackupPlaylistStateFromBgmId(string uiBgmId)
        {
            var playlistStates = new Dictionary<string, List<PlaylistValueEntry>>();
            foreach (var playlist in _viewModelManager.GetPlaylistsEntriesViewModels())
            {
                var playlistValueEntry = playlist.ToPlaylistEntry();
                foreach (var track in playlistValueEntry.Tracks)
                {
                    if (track.UiBgmId == uiBgmId)
                    {
                        if (!playlistStates.ContainsKey(playlist.Id))
                            playlistStates.Add(playlist.Id, new List<PlaylistValueEntry>());
                        playlistStates[playlist.Id].Add(track);
                    }
                }
            }
            return playlistStates;
        }

        private void RestorePlaylistStateToBgmId(Dictionary<string, List<PlaylistValueEntry>> playlistStates, string uiBgmId)
        {
            var vmBgmRoot = _viewModelManager.GetBgmDbRootViewModel(uiBgmId);
            foreach (var playlistState in playlistStates)
            {
                var vmPlaylist = _viewModelManager.GetPlaylistViewModel(playlistState.Key);
                foreach (var value in playlistState.Value)
                {
                    vmPlaylist.AddSong(vmBgmRoot, value);
                }
            }
        }

        #endregion

        #region Fixes
        public async Task<bool> FixUnknownValues()
        {
            bool confirm = false;
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                confirm = await _messageDialog.ShowWarningConfirm("Fix Hidden Songs in Song Selector",
                    "Running this script will set 'is_selectable_original' and 'is_selectable_movie_edit' to true in all your mod bgms.\r\n" +
                    "This can be used to fix an issue where some songs wouldn't show up in the song selector for Battlefield/Final Destination.\r\n" +
                    "A lite backup will be performed before running this script.\r\n" +
                    "Continue?");
            }, DispatcherPriority.Background);

            if (confirm && await BackupProject(false, false))
            {
                foreach (var bgmDbRootEntry in _viewModelManager.GetBgmDbRootEntriesViewModels())
                {
                    if (bgmDbRootEntry.IsMod)
                    {
                        if (!bgmDbRootEntry.IsSelectableOriginal || !bgmDbRootEntry.IsSelectableMovieEdit)
                        {
                            bgmDbRootEntry.IsSelectableOriginal = true;
                            bgmDbRootEntry.IsSelectableMovieEdit = true;
                            bgmDbRootEntry.SaveChanges();
                            if (!await bgmDbRootEntry.MusicMod.AddOrUpdateMusicModEntries(new ViewModels.BgmEntryViewModel(bgmDbRootEntry).GetMusicModEntries()))
                            {
                                await Dispatcher.UIThread.InvokeAsync(async () =>
                                {
                                    await _messageDialog.ShowError("Fix Hidden Songs in Song Selector", "There was an error while updating BGM values.");
                                }, DispatcherPriority.Background);
                                return true;
                            }
                        }
                    }
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowInformation("Fix Hidden Songs in Song Selector", "Done!\r\n If Core Bgm were affected, please also use the option 'Reset Modifications to Core Game Songs Metadata'.");
                }, DispatcherPriority.Background);
                return true;
            }

            return false;
        }
        #endregion

        #region Scripts
        public async Task UpdateBgmSelectorStages(bool enable)
        {
            bool confirm = false;
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                string state = enable ? "enable" : "disable";
                confirm = await _messageDialog.ShowWarningConfirm("Update Song Selector Stages",
                    $"Running will {state} the Song Selector on all stages.\r\n" +
                    $"Battlefield / Final Destination values will NOT be affected.\r\n" +
                    "A lite backup will be performed before running this script.\r\n" +
                    "Continue?");
            }, DispatcherPriority.Background);

            if (confirm && await BackupProject(false, false))
            {
                var stages = _viewModelManager.GetStagesEntriesViewModels();
                foreach (var stageEntryVm in _viewModelManager.GetStagesEntriesViewModels())
                {
                    if (stageEntryVm.UiStageId != "ui_stage_battle_field_s"
                        && stageEntryVm.UiStageId != "ui_stage_battle_field"
                        && stageEntryVm.UiStageId != "ui_stage_battle_field_l"
                        && stageEntryVm.UiStageId != "ui_stage_end")
                        stageEntryVm.BgmSelector = enable;
                }

                if (!_sma5hMusicOverride.UpdateMusicStageOverride(stages.Select(p => _mapper.Map(p, p.GetStageEntryReference())).ToList()))
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await _messageDialog.ShowError("Update Song Selector Stages", "There was an error while updating Song Selector value.");
                    }, DispatcherPriority.Background);
                    return;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowInformation("Update Song Selector Stages", "Done!");
                }, DispatcherPriority.Background);
            }
        }

        public async Task<bool> ResetModOverrideFile(string file)
        {
            bool confirm = false;
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                confirm = await _messageDialog.ShowWarningConfirm("Reset Mod Override File",
                    $"This script will delete the file '{file}' in your ModOverrides folder and reload your files.\r\n" +
                    "A lite backup will be performed before running this script.\r\n" +
                    "Continue?");
            }, DispatcherPriority.Background);

            if (confirm && await BackupProject(false, false))
            {
                if (!_sma5hMusicOverride.ResetOverrideFile(file))
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await _messageDialog.ShowError("Reset Mod Override File", "There was an error while resetting the file.");
                    }, DispatcherPriority.Background);
                    return true;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowInformation("Reset Mod Override File", "Done!");
                }, DispatcherPriority.Background);

                return true;
            }

            return false;
        }
        #endregion

        public async Task<AudioCuePoints> UpdateAudioCuePoints(string filename)
        {
            //Calculate cues
            var audioCuePoints = await _audioMetadataService.GetCuePoints(filename);
            if (audioCuePoints == null || audioCuePoints.TotalSamples <= 0)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowError("Update Audio Cue Points", $"The filename {filename} didn't have cue points. Make sure audio library is properly installed.");
                }, DispatcherPriority.Background);
                return null;
            }
            if (audioCuePoints.Frequency < 32000)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowError("Update Audio Cue Points", $"The frequency of the audio file {filename} must be at least 32Khz.");
                }, DispatcherPriority.Background);
                return null;
            }
            return audioCuePoints;
        }

        public string GameVersion
        {
            get
            {
                return _audioState.GameVersion == 0 ? "?? (custom)" : $"{_audioState.GameVersion}.0";
            }
        }

        public bool IsGameVersionFound
        {
            get
            {
                return _audioState.GameVersion != 0;
            }
        }

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
                    if (Directory.Exists(_config.Value.Sma5hMusic.CachePath))
                    {
                        var existingFiles = Directory.GetFiles(_config.Value.Sma5hMusic.CachePath, "*.nus3audio", SearchOption.AllDirectories);
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
                result = _sma5hMusicOverride.UpdateSoundTestOrderConfig(_viewModelManager.GetBgmDbRootEntriesViewModels().ToDictionary(p => p.UiBgmId, p => p.TestDispOrder));
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

        public async Task<bool> ReorderSongs(IEnumerable<string> bgmEntriesToReorder, short newPosition)
        {
            bool result;

            try
            {
                _viewModelManager.ReorderSongs(bgmEntriesToReorder, newPosition);
                result = _sma5hMusicOverride.UpdateSoundTestOrderConfig(_viewModelManager.GetBgmDbRootEntriesViewModels().ToDictionary(p => p.UiBgmId, p => p.TestDispOrder));
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

        private MusicModEntries DuplicateMusicModEntriesSet(MusicModEntries musicModEntries, string toneId, string filename, IMusicMod musicMod)
        {
            if (musicModEntries.BgmDbRootEntries.Count != 1 ||
              musicModEntries.BgmAssignedInfoEntries.Count != 1 ||
              musicModEntries.BgmStreamSetEntries.Count != 1 ||
              musicModEntries.BgmStreamPropertyEntries.Count != 1 ||
              musicModEntries.BgmPropertyEntries.Count != 1 ||
              musicMod == null || string.IsNullOrEmpty(toneId) || string.IsNullOrEmpty(filename))
            {
                return null;
            }
            //Entries from mod to be duplicated
            var dbRoot = musicModEntries.BgmDbRootEntries.FirstOrDefault();
            var streamSet = musicModEntries.BgmStreamSetEntries.FirstOrDefault(); ;
            var assignedInfo = musicModEntries.BgmAssignedInfoEntries.FirstOrDefault();
            var streamProperty = musicModEntries.BgmStreamPropertyEntries.FirstOrDefault();
            var bgmProperty = musicModEntries.BgmPropertyEntries.FirstOrDefault();

            //Create mod
            var newMusicModEntries = CreateNewMusicModEntriesSet(toneId, filename, musicMod);

            //Copy data to new mod
            var newDbRoot = newMusicModEntries.BgmDbRootEntries.FirstOrDefault();
            var newStreamSet = newMusicModEntries.BgmStreamSetEntries.FirstOrDefault();
            var newAssignedInfo = newMusicModEntries.BgmAssignedInfoEntries.FirstOrDefault();
            var newStreamProperty = newMusicModEntries.BgmStreamPropertyEntries.FirstOrDefault();
            var newBgmProperty = newMusicModEntries.BgmPropertyEntries.FirstOrDefault();
            var newStreamSetId = newStreamSet.StreamSetId;
            var newInfoId = newAssignedInfo.InfoId;
            var newStreamId = newStreamProperty.StreamId;
            var newNameId = newBgmProperty.NameId;
            _mapper.Map(dbRoot, newDbRoot);
            _mapper.Map(streamSet, newStreamSet);
            _mapper.Map(assignedInfo, newAssignedInfo);
            _mapper.Map(streamProperty, newStreamProperty);
            _mapper.Map(bgmProperty, newBgmProperty);
            //Fix Ids references
            newDbRoot.StreamSetId = newStreamSetId;
            newDbRoot.MusicMod = musicMod;
            newStreamSet.Info0 = newInfoId;
            newStreamSet.MusicMod = musicMod;
            newAssignedInfo.StreamId = newStreamId;
            newAssignedInfo.MusicMod = musicMod;
            newStreamProperty.DataName0 = newNameId;
            newStreamProperty.MusicMod = musicMod;
            newBgmProperty.MusicMod = musicMod;

            return newMusicModEntries;
        }

        private MusicModEntries CreateNewMusicModEntriesSet(string toneId, string filename, IMusicMod musicMod)
        {
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

            return musicModEntries;
        }

        public async Task<bool> BackupProject(bool fullBackup, bool showConfirm = true)
        {
            try
            {
                bool confirm = false;
                if (showConfirm)
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        confirm = await _messageDialog.ShowWarningConfirm("Backup Project",
                            $"Running a backup might take a while and the UI may become unresponsive. Continue?");
                    }, DispatcherPriority.Background);
                }

                if (confirm || !showConfirm)
                {
                    var modFolder = _config.Value.Sma5hMusic.ModPath;
                    var modOverrideFolder = _config.Value.Sma5hMusicOverride.ModPath;
                    var dateFolder = $"backup_{DateTime.Now:yyyy_MM_dd_hh_mm_ss_tt}";
                    var backupModFolder = Path.Combine(_config.Value.BackupPath, dateFolder, modFolder);
                    var backupModOverrideFolder = Path.Combine(_config.Value.BackupPath, dateFolder, modOverrideFolder);
                    if (Directory.Exists(modFolder))
                        CopyDirHelper.Copy(modFolder, backupModFolder, fullBackup ? "*" : "*.json");
                    if (Directory.Exists(modOverrideFolder))
                        CopyDirHelper.Copy(modOverrideFolder, backupModOverrideFolder, fullBackup ? "*" : "*.json");

                    if (showConfirm)
                    {
                        await Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            await _messageDialog.ShowInformation("Backup Project", "Done!");
                        }, DispatcherPriority.Background);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowError("Backup Project", "There was an error while performing a backup. Please check the logs.", e);
                }, DispatcherPriority.Background);
                return false;
            }
        }
    }
}
