using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using Sm5shMusic.GUI.Interfaces;
using System;
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

        public GUIStateManager(ISm5shMusicOverride sm5shMusicOverride, IViewModelManager viewModelManager, IAudioStateService audioStateService,
            IMusicModManagerService musicModManagerService, IMessageDialog messageDialog, ILogger<IGUIStateManager> logger)
        {
            _logger = logger;
            _messageDialog = messageDialog;
            _sm5shMusicOverride = sm5shMusicOverride;
            _audioState = audioStateService;
            _musicModManagerService = musicModManagerService;
            _viewModelManager = viewModelManager;
        }

        #region Music Mod Entries
        public async Task UpdateMusicModEntries(MusicModEntries musicModEntries, IMusicMod musicMod = null)
        {
            bool result = false;
            if (musicModEntries != null)
            {
                try
                {
                    _logger.LogInformation("Update Music Mod Entries: {Game}, {DbRoot}, {StreamSet}, {AssignedInfo}, {StreamProperty}, {BgmProperty}",
                        musicModEntries.GameTitleEntries.Count, musicModEntries.BgmDbRootEntries.Count, musicModEntries.BgmStreamSetEntries.Count,
                        musicModEntries.BgmAssignedInfoEntries.Count, musicModEntries.BgmStreamPropertyEntries.Count, musicModEntries.BgmPropertyEntries.Count);

                    if (musicMod != null)
                        result = musicMod.AddOrUpdateMusicModEntries(musicModEntries);
                    else
                        result = _sm5shMusicOverride.UpdateCoreBgmEntries(musicModEntries);

                    _viewModelManager.ReorderSongs();
                }
                catch(Exception e)
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
        }

        public async Task RemoveMusicModEntries(MusicModDeleteEntries musicModDeleteEntries, IMusicMod musicMod = null)
        {
            bool result = false;
            if (musicModDeleteEntries != null)
            {
                try
                {
                    _logger.LogInformation("Delete Music Mod Entries: {Game}, {DbRoot}, {StreamSet}, {AssignedInfo}, {StreamProperty}, {BgmProperty}",
                        musicModDeleteEntries.GameTitleEntries.Count, musicModDeleteEntries.BgmDbRootEntries.Count, musicModDeleteEntries.BgmStreamSetEntries.Count,
                        musicModDeleteEntries.BgmAssignedInfoEntries.Count, musicModDeleteEntries.BgmStreamPropertyEntries.Count, musicModDeleteEntries.BgmPropertyEntries.Count);

                    if (musicMod != null)
                        result = musicMod.RemoveMusicModEntries(musicModDeleteEntries);
                    //else
                    //    result = _sm5shMusicOverride.UpdateCoreBgmEntries(musicModEntries); //Not supported for now.

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
        }
        #endregion

        #region Game
        public async Task<string> CreateNewGameTitleEntry(GameTitleEntry gameTitleEntry)
        {
            bool result = false;

            try
            {
                if (_audioState.AddGameTitleEntry(gameTitleEntry))
                {
                    result = _viewModelManager.AddNewGameTitleEntryViewModel(gameTitleEntry);
                }
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

        public async Task UpdateGameTitleEntry(GameTitleEntry gameTitleEntry)
        {
            bool result;
            try
            {
                if (gameTitleEntry.Source == EntrySource.Mod)
                {
                    //TODO - enumerate mods and update game
                    result = true;
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
        }
        #endregion

        #region Mod
        public async Task<string> CreateNewModEntry(MusicModInformation musicModInformation, string modPath)
        {
            bool result = false;

            IMusicMod newManagerMod = null;
            try
            {
                newManagerMod = _musicModManagerService.AddMusicMod(new MusicModInformation(), modPath);

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

        public async Task UpdateModEntry(IMusicMod musicMod, MusicModInformation musicModInformation)
        {
            bool result;

            try
            {
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
        }
        #endregion

        #region Playlists
        public async Task<string> CreateNewPlaylists(PlaylistEntry playlistEntry)
        {
            bool result = false;

            try
            {
                if (_audioState.AddPlaylistEntry(playlistEntry))
                {
                    result = _viewModelManager.AddNewPlaylistEntryViewModel(playlistEntry);
                }
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

        public async Task UpdatePlaylists()
        {
            bool result = false;

            try
            {
                var playlists = _viewModelManager.GetPlaylistsEntriesViewModels().ToDictionary(p => p.Id, p => p.ToPlaylistEntry());
                result = _sm5shMusicOverride.UpdatePlaylistConfig(playlists);
            }
            catch(Exception e)
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
        }

        public async Task RemovePlaylist(string playlistId)
        {
            bool result = false;

            try
            {
                result = _audioState.RemovePlaylistEntry(playlistId);
                if (result)
                {
                    _viewModelManager.RemovePlaylist(playlistId);
                }
                await UpdatePlaylists();
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
        }

        public async Task UpdateStages()
        {
            bool result = false;

            try
            {
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
        }
        #endregion
    }
}
