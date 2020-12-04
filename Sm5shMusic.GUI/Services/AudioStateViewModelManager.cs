using AutoMapper;
using Microsoft.Extensions.Logging;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using Sm5shMusic.GUI.Helpers;
using Sm5shMusic.GUI.Interfaces;
using Sm5shMusic.GUI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using VGMMusic;

namespace Sm5shMusic.GUI.Services
{
    public class AudioStateViewModelManager : IAudioStateViewModelManager
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IAudioStateService _audioState;
        private readonly ISm5shMusicOverride _sm5shMusicOverride;
        private readonly IVGMMusicPlayer _vgmMusicPlayer;
        private readonly Dictionary<string, LocaleViewModel> _vmDictLocalesEntries;
        private readonly Dictionary<string, SeriesEntryViewModel> _vmDictSeriesEntries;
        private readonly Dictionary<string, GameTitleEntryViewModel> _vmDictGameTitlesEntries;
        private readonly Dictionary<string, BgmDbRootEntryViewModel> _vmDictBgmDbRootEntries;
        private readonly Dictionary<string, BgmStreamSetEntryViewModel> _vmDictBgmStreamSetEntries;
        private readonly Dictionary<string, BgmAssignedInfoEntryViewModel> _vmDictBgmAssignedInfoEntries;
        private readonly Dictionary<string, BgmStreamPropertyEntryViewModel> _vmDictBgmStreamPropertyEntries;
        private readonly Dictionary<string, BgmPropertyEntryViewModel> _vmDictBgmPropertyEntries;
        private readonly Dictionary<string, PlaylistEntryViewModel> _vmPlaylistsEntries;
        private readonly Dictionary<string, StageEntryViewModel> _vmStagesEntries;

        public AudioStateViewModelManager(IAudioStateService audioStateService, IVGMMusicPlayer vgmMusicPlayer, ISm5shMusicOverride sm5shMusicOverride, IMapper mapper, ILogger<IAudioStateViewModelManager> logger)
        {
            _mapper = mapper;
            _logger = logger;
            _vgmMusicPlayer = vgmMusicPlayer;
            _audioState = audioStateService;
            _sm5shMusicOverride = sm5shMusicOverride;
            _vmDictLocalesEntries = new Dictionary<string, LocaleViewModel>();
            _vmDictSeriesEntries = new Dictionary<string, SeriesEntryViewModel>();
            _vmDictGameTitlesEntries = new Dictionary<string, GameTitleEntryViewModel>();
            _vmDictBgmDbRootEntries = new Dictionary<string, BgmDbRootEntryViewModel>();
            _vmDictBgmStreamSetEntries = new Dictionary<string, BgmStreamSetEntryViewModel>();
            _vmDictBgmAssignedInfoEntries = new Dictionary<string, BgmAssignedInfoEntryViewModel>();
            _vmDictBgmStreamPropertyEntries = new Dictionary<string, BgmStreamPropertyEntryViewModel>();
            _vmDictBgmPropertyEntries = new Dictionary<string, BgmPropertyEntryViewModel>();
            _vmPlaylistsEntries = new Dictionary<string, PlaylistEntryViewModel>();
            _vmStagesEntries = new Dictionary<string, StageEntryViewModel>();
        }

        public void Init()
        {
            _vmDictLocalesEntries.Clear();
            _vmDictSeriesEntries.Clear();
            _vmDictGameTitlesEntries.Clear();
            _vmDictBgmDbRootEntries.Clear();
            _vmDictBgmStreamSetEntries.Clear();
            _vmDictBgmAssignedInfoEntries.Clear();
            _vmDictBgmStreamPropertyEntries.Clear();
            _vmDictBgmPropertyEntries.Clear();
            _vmPlaylistsEntries.Clear();
            _vmStagesEntries.Clear();

            var vmLocalesList = _audioState.GetLocales().Select(p => new LocaleViewModel(p, Constants.GetLocaleDisplayName(p)));
            foreach(var vmLocale in vmLocalesList)
                _vmDictLocalesEntries.Add(vmLocale.Id, vmLocale);
            _logger.LogInformation("Locales Loaded.");

            var vmSeriesList = _audioState.GetSeriesEntries().Select(p => new SeriesEntryViewModel(p));
            foreach (var vmSeries in vmSeriesList)
                _vmDictSeriesEntries.Add(vmSeries.SeriesId, vmSeries);
            _logger.LogInformation("Series Loaded.");

            var vmGameTitlesList = _audioState.GetGameTitleEntries().Select(p => _mapper.Map(p, new GameTitleEntryViewModel(this, _mapper, p)));
            foreach (var vmGame in vmGameTitlesList)
                _vmDictGameTitlesEntries.Add(vmGame.UiGameTitleId, vmGame);
            _logger.LogInformation("Game Titles Loaded.");

            var vmBgmDbRootEntriesList = _audioState.GetBgmDbRootEntries().Select(p => _mapper.Map(p, new BgmDbRootEntryViewModel(this, _mapper, p)));
            foreach (var vmBgmDbRootEntry in vmBgmDbRootEntriesList)
                _vmDictBgmDbRootEntries.Add(vmBgmDbRootEntry.UiBgmId, vmBgmDbRootEntry);
            ReorderSongs();
            _logger.LogInformation("BGM DB Root List Loaded.");

            var vmBgmStreamSetEntriesList = _audioState.GetBgmStreamSetEntries().Select(p => _mapper.Map(p, new BgmStreamSetEntryViewModel(this, _mapper, p)));
            foreach (var vmBgmStreamSetEntry in vmBgmStreamSetEntriesList)
                _vmDictBgmStreamSetEntries.Add(vmBgmStreamSetEntry.StreamSetId, vmBgmStreamSetEntry);
            _logger.LogInformation("BGM Stream Set List Loaded.");

            var vmBgmAssignedInfoEntriesList = _audioState.GetBgmAssignedInfoEntries().Select(p => _mapper.Map(p, new BgmAssignedInfoEntryViewModel(this, _mapper, p)));
            foreach (var vmBgmAssignedInfoEntry in vmBgmAssignedInfoEntriesList)
                _vmDictBgmAssignedInfoEntries.Add(vmBgmAssignedInfoEntry.InfoId, vmBgmAssignedInfoEntry);
            _logger.LogInformation("BGM Assigned Info List Loaded.");

            var vmBgmStreamPropertyEntriesList = _audioState.GetBgmStreamPropertyEntries().Select(p => _mapper.Map(p, new BgmStreamPropertyEntryViewModel(this, _mapper, p)));
            foreach (var vmBgmStreamPropertyEntry in vmBgmStreamPropertyEntriesList)
                _vmDictBgmStreamPropertyEntries.Add(vmBgmStreamPropertyEntry.StreamId, vmBgmStreamPropertyEntry);
            _logger.LogInformation("BGM Stream Property List Loaded.");

            var vmBgmPropertyEntriesList = _audioState.GetBgmPropertyEntries().Select(p => _mapper.Map(p, new BgmPropertyEntryViewModel(_vgmMusicPlayer, this, _mapper, p)));
            foreach (var vmBgmPropertyEntry in vmBgmPropertyEntriesList)
                _vmDictBgmPropertyEntries.Add(vmBgmPropertyEntry.NameId, vmBgmPropertyEntry);
            _logger.LogInformation("BGM Property List Loaded.");

            var playlistsList = _audioState.GetPlaylists().Select(p => new PlaylistEntryViewModel(p, _vmDictBgmDbRootEntries));
            foreach (var vmPlaylist in playlistsList)
                _vmPlaylistsEntries.Add(vmPlaylist.Id, vmPlaylist);
            _logger.LogInformation("Playlists Loaded.");

            var stagesList = _audioState.GetStagesEntries().Select(p => new StageEntryViewModel(p));
            foreach (var vmStage in stagesList)
                _vmStagesEntries.Add(vmStage.UiStageId, vmStage);
            _logger.LogInformation("Stages Loaded.");
        }

        #region GET ALL
        public IEnumerable<LocaleViewModel> GetLocalesViewModels()
        {
            return _vmDictLocalesEntries.Values;
        }

        public IEnumerable<SeriesEntryViewModel> GetSeriesViewModels()
        {
            return _vmDictSeriesEntries.Values;
        }

        public IEnumerable<GameTitleEntryViewModel> GetGameTitlesViewModels()
        {
            return _vmDictGameTitlesEntries.Values;
        }

        public IEnumerable<BgmDbRootEntryViewModel> GetBgmDbRootEntriesViewModels()
        {
            return _vmDictBgmDbRootEntries.Values;
        }

        public IEnumerable<BgmStreamSetEntryViewModel> GetBgmStreamSetEntriesViewModels()
        {
            return _vmDictBgmStreamSetEntries.Values;
        }

        public IEnumerable<BgmAssignedInfoEntryViewModel> GetBgmAssignedInfoEntriesViewModels()
        {
            return _vmDictBgmAssignedInfoEntries.Values;
        }

        public IEnumerable<BgmStreamPropertyEntryViewModel> GetBgmStreamPropertyEntriesViewModels()
        {
            return _vmDictBgmStreamPropertyEntries.Values;
        }

        public IEnumerable<BgmPropertyEntryViewModel> GetBgmPropertyEntriesViewModels()
        {
            return _vmDictBgmPropertyEntries.Values;
        }

        public IEnumerable<PlaylistEntryViewModel> GetPlaylistsEntriesViewModels()
        {
            return _vmPlaylistsEntries.Values;
        }

        public IEnumerable<StageEntryViewModel> GetStagesEntriesViewModels()
        {
            return _vmStagesEntries.Values;
        }
        #endregion

        #region GET
        public GameTitleEntryViewModel GetGameTitleViewModel(string uiGameTitleId)
        {
            if (string.IsNullOrEmpty(uiGameTitleId))
                return null;
            return _vmDictGameTitlesEntries.ContainsKey(uiGameTitleId) ? _vmDictGameTitlesEntries[uiGameTitleId] : null;
        }

        public SeriesEntryViewModel GetSeriesViewModel(string uiSeriesId)
        {
            if (string.IsNullOrEmpty(uiSeriesId))
                return null;
            return _vmDictSeriesEntries.ContainsKey(uiSeriesId) ? _vmDictSeriesEntries[uiSeriesId] : null;
        }

        public BgmDbRootEntryViewModel GetBgmDbRootViewModel(string uiBgmId)
        {
            if (string.IsNullOrEmpty(uiBgmId))
                return null;
            return _vmDictBgmDbRootEntries.ContainsKey(uiBgmId) ? _vmDictBgmDbRootEntries[uiBgmId] : null;
        }

        public BgmStreamSetEntryViewModel GetBgmStreamSetViewModel(string streamSetId)
        {
            if (string.IsNullOrEmpty(streamSetId))
                return null;
            return _vmDictBgmStreamSetEntries.ContainsKey(streamSetId) ? _vmDictBgmStreamSetEntries[streamSetId] : null;
        }

        public BgmAssignedInfoEntryViewModel GetBgmAssignedInfoViewModel(string assignedInfoId)
        {
            if (string.IsNullOrEmpty(assignedInfoId))
                return null;
            return _vmDictBgmAssignedInfoEntries.ContainsKey(assignedInfoId) ? _vmDictBgmAssignedInfoEntries[assignedInfoId] : null;
        }

        public BgmStreamPropertyEntryViewModel GetBgmStreamPropertyViewModel(string streamId)
        {
            if (string.IsNullOrEmpty(streamId))
                return null;
            return _vmDictBgmStreamPropertyEntries.ContainsKey(streamId) ? _vmDictBgmStreamPropertyEntries[streamId] : null;
        }

        public BgmPropertyEntryViewModel GetBgmPropertyViewModel(string nameId)
        {
            if (string.IsNullOrEmpty(nameId))
                return null;
            return _vmDictBgmPropertyEntries.ContainsKey(nameId) ? _vmDictBgmPropertyEntries[nameId] : null;
        }
        #endregion

        public void ReorderSongs()
        {
            short i = 0;
            var listBgms = _vmDictBgmDbRootEntries.Values.Where(p => !p.HiddenInSoundTest).OrderBy(p => p.TestDispOrder);
            foreach (var bgmEntry in listBgms)
            {
                bgmEntry.TestDispOrder = i;
                i += 2;
            }
            //TODO - The data should be first persisted in the BgmEntries
            _sm5shMusicOverride.UpdateSoundTestOrderConfig(listBgms.ToDictionary(p => p.UiBgmId, p => p.TestDispOrder));
        }
    }
}
