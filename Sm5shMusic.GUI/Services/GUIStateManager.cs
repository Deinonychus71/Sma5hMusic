using Microsoft.Extensions.Logging;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using Sm5shMusic.GUI.Interfaces;
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

        public async Task UpdateMusicModEntries(MusicModEntries musicModEntries, IMusicMod musicMod = null)
        {
            bool result = false;
            if (musicModEntries != null)
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

            if (!result)
            {
                await _messageDialog.ShowError("Update Music Mod Entries Error", "There was an error while persisting some modifications. Please check the logs.");
            }
        }

        public async Task RemoveMusicModEntries(MusicModDeleteEntries musicModDeleteEntries, IMusicMod musicMod = null)
        {
            bool result = false;
            if (musicModDeleteEntries != null)
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

            if (!result)
            {
                await _messageDialog.ShowError("Delete Music Mod Entries Error", "There was an error while persisting some modifications. Please check the logs.");
            }
        }

        public async Task<string> CreateNewGameTitleEntry(GameTitleEntry gameTitleEntry)
        {
            bool result = false;
            if (_audioState.AddGameTitleEntry(gameTitleEntry))
            {
                result = _viewModelManager.AddNewGameTitleEntryViewModel(gameTitleEntry);
            }

            if (!result)
            {
                await _messageDialog.ShowError("Create Game Title Entry Error", "There was an error while creating a game entry. Please check the logs.");
            }

            return gameTitleEntry.UiGameTitleId;
        }

        public async Task UpdateGameTitleEntry(GameTitleEntry gameTitleEntry)
        {
            bool result = false;
            if (gameTitleEntry.Source == EntrySource.Mod)
            {
                //TODO - enumerate mods and update game
                result = true;
            }
            else
            {
                result = _sm5shMusicOverride.UpdateCoreGameTitleEntry(gameTitleEntry);
            }

            if (!result)
            {
                await _messageDialog.ShowError("Update Game Title Entry Error", "There was an error while persisting a game entry. Please check the logs.");
            }
        }

        public async Task<string> CreateNewModEntry(MusicModInformation musicModInformation, string modPath)
        {
            bool result = false;

            var newManagerMod = _musicModManagerService.AddMusicMod(new MusicModInformation(), modPath);

            if (newManagerMod != null)
            {
                result = _viewModelManager.AddNewModEntryViewModel(newManagerMod);
            }

            if (!result)
            {
                await _messageDialog.ShowError("Create Mod Entry Error", "There was an error while creating a mod entry. Please check the logs.");
            }

            return newManagerMod.Id;
        }

        public async Task UpdateModEntry(IMusicMod musicMod, MusicModInformation musicModInformation)
        {
            if (!musicMod.UpdateModInformation(musicModInformation))
            {
                await _messageDialog.ShowError("Update Mod Entry Error", "There was an error while persisting mod information. Please check the logs.");
            }
        }
    }
}
