using AutoMapper;
using DynamicData;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sma5h.Mods.Music;
using Sma5h.Mods.Music.Interfaces;
using Sma5h.Mods.Music.Models;
using Sma5hMusic.GUI.Helpers;
using Sma5hMusic.GUI.Interfaces;
using Sma5hMusic.GUI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using VGMMusic;

namespace Sma5hMusic.GUI.Services
{
    public class ViewModelManager : IViewModelManager
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IAudioStateService _audioState;
        private readonly IMusicModManagerService _musicModManager;
        private readonly IVGMMusicPlayer _vgmMusicPlayer;
        private readonly bool _inGameVolume;
        private readonly Dictionary<string, LocaleViewModel> _vmDictLocalesEntries;
        private readonly Dictionary<string, SeriesEntryViewModel> _vmDictSeriesEntries;
        private readonly Dictionary<string, GameTitleEntryViewModel> _vmDictGameTitlesEntries;
        private readonly Dictionary<string, BgmDbRootEntryViewModel> _vmDictBgmDbRootEntries;
        private readonly Dictionary<string, BgmStreamSetEntryViewModel> _vmDictBgmStreamSetEntries;
        private readonly Dictionary<string, BgmAssignedInfoEntryViewModel> _vmDictBgmAssignedInfoEntries;
        private readonly Dictionary<string, BgmStreamPropertyEntryViewModel> _vmDictBgmStreamPropertyEntries;
        private readonly Dictionary<string, BgmPropertyEntryViewModel> _vmDictBgmPropertyEntries;
        private readonly Dictionary<string, PlaylistEntryViewModel> _vmDictPlaylistsEntries;
        private readonly Dictionary<string, StageEntryViewModel> _vmDictStagesEntries;
        private readonly Dictionary<string, ModEntryViewModel> _vmDictModsEntries;

        private readonly SourceCache<LocaleViewModel, string> _vmObsvLocalesEntries;
        private readonly SourceCache<SeriesEntryViewModel, string> _vmObsvSeriesEntries;
        private readonly SourceCache<GameTitleEntryViewModel, string> _vmObsvGameTitlesEntries;
        private readonly SourceCache<BgmDbRootEntryViewModel, string> _vmObsvBgmDbRootEntries;
        private readonly SourceCache<BgmStreamSetEntryViewModel, string> _vmObsvBgmStreamSetEntries;
        private readonly SourceCache<BgmAssignedInfoEntryViewModel, string> _vmObsvBgmAssignedInfoEntries;
        private readonly SourceCache<BgmStreamPropertyEntryViewModel, string> _vmObsvBgmStreamPropertyEntries;
        private readonly SourceCache<BgmPropertyEntryViewModel, string> _vmObsvBgmPropertyEntries;
        private readonly SourceCache<PlaylistEntryViewModel, string> _vmObsvPlaylistsEntries;
        private readonly SourceCache<StageEntryViewModel, string> _vmObsvStagesEntries;
        private readonly SourceCache<ModEntryViewModel, string> _vmObsvModsEntries;

        public ViewModelManager(IOptions<ApplicationSettings> config, IAudioStateService audioStateService, IMusicModManagerService musicModManager, IVGMMusicPlayer vgmMusicPlayer, IMapper mapper, ILogger<IViewModelManager> logger)
        {
            _inGameVolume = config.Value.Sma5hMusicGUI.InGameVolume;
            _mapper = mapper;
            _logger = logger;
            _vgmMusicPlayer = vgmMusicPlayer;
            _audioState = audioStateService;
            _musicModManager = musicModManager;
            _vmDictLocalesEntries = new Dictionary<string, LocaleViewModel>();
            _vmDictSeriesEntries = new Dictionary<string, SeriesEntryViewModel>();
            _vmDictGameTitlesEntries = new Dictionary<string, GameTitleEntryViewModel>();
            _vmDictBgmDbRootEntries = new Dictionary<string, BgmDbRootEntryViewModel>();
            _vmDictBgmStreamSetEntries = new Dictionary<string, BgmStreamSetEntryViewModel>();
            _vmDictBgmAssignedInfoEntries = new Dictionary<string, BgmAssignedInfoEntryViewModel>();
            _vmDictBgmStreamPropertyEntries = new Dictionary<string, BgmStreamPropertyEntryViewModel>();
            _vmDictBgmPropertyEntries = new Dictionary<string, BgmPropertyEntryViewModel>();
            _vmDictPlaylistsEntries = new Dictionary<string, PlaylistEntryViewModel>();
            _vmDictStagesEntries = new Dictionary<string, StageEntryViewModel>();
            _vmDictModsEntries = new Dictionary<string, ModEntryViewModel>();

            _vmObsvLocalesEntries = new SourceCache<LocaleViewModel, string>(p => p.Id);
            _vmObsvSeriesEntries = new SourceCache<SeriesEntryViewModel, string>(p => p.SeriesId);
            _vmObsvGameTitlesEntries = new SourceCache<GameTitleEntryViewModel, string>(p => p.UiGameTitleId);
            _vmObsvBgmDbRootEntries = new SourceCache<BgmDbRootEntryViewModel, string>(p => p.UiBgmId);
            _vmObsvBgmStreamSetEntries = new SourceCache<BgmStreamSetEntryViewModel, string>(p => p.StreamSetId);
            _vmObsvBgmAssignedInfoEntries = new SourceCache<BgmAssignedInfoEntryViewModel, string>(p => p.InfoId);
            _vmObsvBgmStreamPropertyEntries = new SourceCache<BgmStreamPropertyEntryViewModel, string>(p => p.StreamId);
            _vmObsvBgmPropertyEntries = new SourceCache<BgmPropertyEntryViewModel, string>(p => p.NameId);
            _vmObsvPlaylistsEntries = new SourceCache<PlaylistEntryViewModel, string>(p => p.Id);
            _vmObsvStagesEntries = new SourceCache<StageEntryViewModel, string>(p => p.UiStageId);
            _vmObsvModsEntries = new SourceCache<ModEntryViewModel, string>(p => p.Id);
        }

        public void Init()
        {
            _logger.LogInformation("Initialize data");

            _vmDictModsEntries.Clear();
            _vmDictLocalesEntries.Clear();
            _vmDictSeriesEntries.Clear();
            _vmDictGameTitlesEntries.Clear();
            _vmDictBgmDbRootEntries.Clear();
            _vmDictBgmStreamSetEntries.Clear();
            _vmDictBgmAssignedInfoEntries.Clear();
            _vmDictBgmStreamPropertyEntries.Clear();
            _vmDictBgmPropertyEntries.Clear();
            _vmDictPlaylistsEntries.Clear();
            _vmDictStagesEntries.Clear();

            _vmObsvModsEntries.Clear();
            _vmObsvLocalesEntries.Clear();
            _vmObsvSeriesEntries.Clear();
            _vmObsvGameTitlesEntries.Clear();
            _vmObsvBgmDbRootEntries.Clear();
            _vmObsvBgmStreamSetEntries.Clear();
            _vmObsvBgmAssignedInfoEntries.Clear();
            _vmObsvBgmStreamPropertyEntries.Clear();
            _vmObsvBgmPropertyEntries.Clear();
            _vmObsvPlaylistsEntries.Clear();
            _vmObsvStagesEntries.Clear();

            var modsList = _musicModManager.MusicMods.Select(p => new ModEntryViewModel(this, _mapper, p)).ToList();
            foreach (var vmMod in modsList)
                _vmDictModsEntries.Add(vmMod.Id, vmMod);

            var vmLocalesList = _audioState.GetLocales().Select(p => new LocaleViewModel(p, Constants.GetLocaleDisplayName(p))).ToList();
            foreach (var vmLocale in vmLocalesList)
                _vmDictLocalesEntries.Add(vmLocale.Id, vmLocale);

            var vmSeriesList = _audioState.GetSeriesEntries().Select(p => new SeriesEntryViewModel(p)).ToList();
            foreach (var vmSeries in vmSeriesList)
                _vmDictSeriesEntries.Add(vmSeries.SeriesId, vmSeries);

            var vmGameTitlesList = _audioState.GetGameTitleEntries().Select(p => _mapper.Map(p, new GameTitleEntryViewModel(this, _mapper, p))).ToList();
            foreach (var vmGame in vmGameTitlesList)
                _vmDictGameTitlesEntries.Add(vmGame.UiGameTitleId, vmGame);

            var vmBgmDbRootEntriesList = _audioState.GetBgmDbRootEntries().Select(p => _mapper.Map(p, new BgmDbRootEntryViewModel(this, _mapper, p))).ToList();
            foreach (var vmBgmDbRootEntry in vmBgmDbRootEntriesList)
                _vmDictBgmDbRootEntries.Add(vmBgmDbRootEntry.UiBgmId, vmBgmDbRootEntry);
            ReorderSongs();

            var vmBgmStreamSetEntriesList = _audioState.GetBgmStreamSetEntries().Select(p => _mapper.Map(p, new BgmStreamSetEntryViewModel(this, _mapper, p))).ToList();
            foreach (var vmBgmStreamSetEntry in vmBgmStreamSetEntriesList)
                _vmDictBgmStreamSetEntries.Add(vmBgmStreamSetEntry.StreamSetId, vmBgmStreamSetEntry);

            var vmBgmAssignedInfoEntriesList = _audioState.GetBgmAssignedInfoEntries().Select(p => _mapper.Map(p, new BgmAssignedInfoEntryViewModel(this, _mapper, p))).ToList();
            foreach (var vmBgmAssignedInfoEntry in vmBgmAssignedInfoEntriesList)
                _vmDictBgmAssignedInfoEntries.Add(vmBgmAssignedInfoEntry.InfoId, vmBgmAssignedInfoEntry);

            var vmBgmStreamPropertyEntriesList = _audioState.GetBgmStreamPropertyEntries().Select(p => _mapper.Map(p, new BgmStreamPropertyEntryViewModel(this, _mapper, p))).ToList();
            foreach (var vmBgmStreamPropertyEntry in vmBgmStreamPropertyEntriesList)
                _vmDictBgmStreamPropertyEntries.Add(vmBgmStreamPropertyEntry.StreamId, vmBgmStreamPropertyEntry);

            var vmBgmPropertyEntriesList = _audioState.GetBgmPropertyEntries().Select(p => _mapper.Map(p, new BgmPropertyEntryViewModel(_vgmMusicPlayer, this, _mapper, p, _inGameVolume))).ToList();
            foreach (var vmBgmPropertyEntry in vmBgmPropertyEntriesList)
                _vmDictBgmPropertyEntries.Add(vmBgmPropertyEntry.NameId, vmBgmPropertyEntry);

            var playlistsList = _audioState.GetPlaylists().Select(p => new PlaylistEntryViewModel(p, _vmDictBgmDbRootEntries)).ToList();
            foreach (var vmPlaylist in playlistsList)
                _vmDictPlaylistsEntries.Add(vmPlaylist.Id, vmPlaylist);

            var stagesList = _audioState.GetStagesEntries().Select(p => new StageEntryViewModel(p)).ToList();
            foreach (var vmStage in stagesList)
                _vmDictStagesEntries.Add(vmStage.UiStageId, vmStage);

            _vmObsvModsEntries.AddOrUpdate(modsList);
            _logger.LogInformation("Music Mods List Loaded.");
            _vmObsvLocalesEntries.AddOrUpdate(vmLocalesList);
            _logger.LogInformation("Locales Loaded.");
            _vmObsvSeriesEntries.AddOrUpdate(vmSeriesList);
            _logger.LogInformation("Series Loaded.");
            _vmObsvGameTitlesEntries.AddOrUpdate(vmGameTitlesList);
            _logger.LogInformation("Game Titles Loaded.");
            _vmObsvBgmDbRootEntries.AddOrUpdate(vmBgmDbRootEntriesList);
            _logger.LogInformation("BGM DB Root List Loaded.");
            _vmObsvBgmStreamSetEntries.AddOrUpdate(vmBgmStreamSetEntriesList);
            _logger.LogInformation("BGM Stream Set List Loaded.");
            _vmObsvBgmAssignedInfoEntries.AddOrUpdate(vmBgmAssignedInfoEntriesList);
            _logger.LogInformation("BGM Assigned Info List Loaded.");
            _vmObsvBgmStreamPropertyEntries.AddOrUpdate(vmBgmStreamPropertyEntriesList);
            _logger.LogInformation("BGM Stream Property List Loaded.");
            _vmObsvBgmPropertyEntries.AddOrUpdate(vmBgmPropertyEntriesList);
            _logger.LogInformation("BGM Property List Loaded.");
            _vmObsvPlaylistsEntries.AddOrUpdate(playlistsList);
            _logger.LogInformation("Playlists Loaded.");
            _vmObsvStagesEntries.AddOrUpdate(stagesList);
            _logger.LogInformation("Stages Loaded.");
        }

        #region Observers
        public IObservableCache<LocaleViewModel, string> ObservableLocales { get { return _vmObsvLocalesEntries; } }
        public IObservableCache<SeriesEntryViewModel, string> ObservableSeries { get { return _vmObsvSeriesEntries; } }
        public IObservableCache<GameTitleEntryViewModel, string> ObservableGameTitles { get { return _vmObsvGameTitlesEntries; } }
        public IObservableCache<BgmDbRootEntryViewModel, string> ObservableDbRootEntries { get { return _vmObsvBgmDbRootEntries; } }
        public IObservableCache<BgmStreamSetEntryViewModel, string> ObservableStreamSetEntries { get { return _vmObsvBgmStreamSetEntries; } }
        public IObservableCache<BgmAssignedInfoEntryViewModel, string> ObservableAssignedInfoEntries { get { return _vmObsvBgmAssignedInfoEntries; } }
        public IObservableCache<BgmStreamPropertyEntryViewModel, string> ObservableStreamPropertyEntries { get { return _vmObsvBgmStreamPropertyEntries; } }
        public IObservableCache<BgmPropertyEntryViewModel, string> ObservableBgmPropertyEntries { get { return _vmObsvBgmPropertyEntries; } }
        public IObservableCache<PlaylistEntryViewModel, string> ObservablePlaylistsEntries { get { return _vmObsvPlaylistsEntries; } }
        public IObservableCache<StageEntryViewModel, string> ObservableStagesEntries { get { return _vmObsvStagesEntries; } }
        public IObservableCache<ModEntryViewModel, string> ObservableModsEntries { get { return _vmObsvModsEntries; } }
        #endregion

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
            return _vmDictPlaylistsEntries.Values;
        }

        public IEnumerable<StageEntryViewModel> GetStagesEntriesViewModels()
        {
            return _vmDictStagesEntries.Values;
        }
        #endregion

        #region GET
        public ModEntryViewModel GetModEntryViewModel(string modId)
        {
            if (string.IsNullOrEmpty(modId))
                return null;
            return _vmDictModsEntries.ContainsKey(modId) ? _vmDictModsEntries[modId] : null;
        }

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

        public PlaylistEntryViewModel GetPlaylistViewModel(string playlistId)
        {
            if (string.IsNullOrEmpty(playlistId))
                return null;
            return _vmDictPlaylistsEntries.ContainsKey(playlistId) ? _vmDictPlaylistsEntries[playlistId] : null;
        }
        #endregion

        #region REMOVE
        public void RemoveGameTitleView(string uiGameTitleId)
        {
            if (_vmDictGameTitlesEntries.ContainsKey(uiGameTitleId))
            {
                _vmObsvGameTitlesEntries.Remove(_vmDictGameTitlesEntries[uiGameTitleId]);
                _vmDictGameTitlesEntries.Remove(uiGameTitleId);
            }
        }

        public void RemoveBgmDbRootView(string uiBgmId)
        {
            if (_vmDictBgmDbRootEntries.ContainsKey(uiBgmId))
            {
                _vmObsvBgmDbRootEntries.Remove(_vmDictBgmDbRootEntries[uiBgmId]);
                _vmDictBgmDbRootEntries.Remove(uiBgmId);
            }
        }

        public void RemoveBgmStreamSetView(string streamSetId)
        {
            if (_vmDictBgmStreamSetEntries.ContainsKey(streamSetId))
            {
                _vmObsvBgmStreamSetEntries.Remove(_vmDictBgmStreamSetEntries[streamSetId]);
                _vmDictBgmStreamSetEntries.Remove(streamSetId);
            }
        }

        public void RemoveBgmAssignedInfoView(string infoId)
        {
            if (_vmDictBgmAssignedInfoEntries.ContainsKey(infoId))
            {
                _vmObsvBgmAssignedInfoEntries.Remove(_vmDictBgmAssignedInfoEntries[infoId]);
                _vmDictBgmAssignedInfoEntries.Remove(infoId);
            }
        }

        public void RemoveBgmStreamPropertyView(string streamId)
        {
            if (_vmDictBgmStreamPropertyEntries.ContainsKey(streamId))
            {
                _vmObsvBgmStreamPropertyEntries.Remove(_vmDictBgmStreamPropertyEntries[streamId]);
                _vmDictBgmStreamPropertyEntries.Remove(streamId);
            }
        }

        public void RemoveBgmPropertyView(string nameId)
        {
            if (_vmDictBgmPropertyEntries.ContainsKey(nameId))
            {
                _vmObsvBgmPropertyEntries.Remove(_vmDictBgmPropertyEntries[nameId]);
                _vmDictBgmPropertyEntries.Remove(nameId);
            }
        }

        public void RemovePlaylist(string playlistId)
        {
            if (_vmDictPlaylistsEntries.ContainsKey(playlistId))
            {
                _vmObsvPlaylistsEntries.Remove(_vmDictPlaylistsEntries[playlistId]);
                _vmDictPlaylistsEntries.Remove(playlistId);
            }
        }

        public void RemoveBgmInAllPlaylists(string uiBgmId)
        {
            foreach (var playlist in _vmDictPlaylistsEntries.Values)
            {
                playlist.RemoveSong(uiBgmId);
            }
        }
        #endregion

        #region CREATE
        public bool AddNewModEntryViewModel(IMusicMod musicMod)
        {
            var newVM = new ModEntryViewModel(this, _mapper, musicMod);
            _vmDictModsEntries.Add(newVM.Id, newVM);
            _vmObsvModsEntries.AddOrUpdate(newVM);
            return true;
        }

        public bool AddNewGameTitleEntryViewModel(GameTitleEntry gameTitleEntry)
        {
            var newVM = _mapper.Map(gameTitleEntry, new GameTitleEntryViewModel(this, _mapper, gameTitleEntry));
            _vmDictGameTitlesEntries.Add(newVM.UiGameTitleId, newVM);
            _vmObsvGameTitlesEntries.AddOrUpdate(newVM);
            return true;
        }

        public bool AddNewBgmDbRootEntryViewModel(BgmDbRootEntry bgmDbRootEntry)
        {
            var newVM = _mapper.Map(bgmDbRootEntry, new BgmDbRootEntryViewModel(this, _mapper, bgmDbRootEntry));
            _vmDictBgmDbRootEntries.Add(newVM.UiBgmId, newVM);
            _vmObsvBgmDbRootEntries.AddOrUpdate(newVM);
            return true;
        }

        public bool AddNewBgmStreamSetEntryViewModel(BgmStreamSetEntry bgmStreamSetEntry)
        {
            var newVM = _mapper.Map(bgmStreamSetEntry, new BgmStreamSetEntryViewModel(this, _mapper, bgmStreamSetEntry));
            _vmDictBgmStreamSetEntries.Add(newVM.StreamSetId, newVM);
            _vmObsvBgmStreamSetEntries.AddOrUpdate(newVM);
            return true;
        }

        public bool AddNewBgmAssignedInfoEntryViewModel(BgmAssignedInfoEntry bgmAssignedInfoEntry)
        {
            var newVM = _mapper.Map(bgmAssignedInfoEntry, new BgmAssignedInfoEntryViewModel(this, _mapper, bgmAssignedInfoEntry));
            _vmDictBgmAssignedInfoEntries.Add(newVM.InfoId, newVM);
            _vmObsvBgmAssignedInfoEntries.AddOrUpdate(newVM);
            return true;
        }

        public bool AddNewBgmStreamPropertyEntryViewModel(BgmStreamPropertyEntry bgmStreamPropertyEntry)
        {
            var newVM = _mapper.Map(bgmStreamPropertyEntry, new BgmStreamPropertyEntryViewModel(this, _mapper, bgmStreamPropertyEntry));
            _vmDictBgmStreamPropertyEntries.Add(newVM.StreamId, newVM);
            _vmObsvBgmStreamPropertyEntries.AddOrUpdate(newVM);
            return true;
        }

        public bool AddNewBgmPropertyEntryViewModel(BgmPropertyEntry bgmPropertyEntry)
        {
            var newVM = _mapper.Map(bgmPropertyEntry, new BgmPropertyEntryViewModel(_vgmMusicPlayer, this, _mapper, bgmPropertyEntry, _inGameVolume));
            _vmDictBgmPropertyEntries.Add(newVM.NameId, newVM);
            _vmObsvBgmPropertyEntries.AddOrUpdate(newVM);
            return true;
        }

        public bool AddNewPlaylistEntryViewModel(PlaylistEntry playlistEntry)
        {
            var newVM = new PlaylistEntryViewModel(playlistEntry, null);
            _vmDictPlaylistsEntries.Add(newVM.Id, newVM);
            _vmObsvPlaylistsEntries.AddOrUpdate(newVM);
            return true;
        }
        #endregion

        public void ReorderSongs()
        {
            short i = 0;
            var listVmBgms = _vmDictBgmDbRootEntries.Values.Where(p => !p.HiddenInSoundTest).OrderBy(p => p.TestDispOrder);
            foreach (var vmBgmEntry in listVmBgms)
            {
                ReOrderVmBgmEntry(vmBgmEntry, i);
                i++;
            }
        }

        public void ReorderSongs(IEnumerable<string> bgmEntriesToReorder, short newPosition)
        {
            var minAffected = newPosition;
            var maxAffected = newPosition;

            var listSelectedVmBgms = new List<BgmDbRootEntryViewModel>();
            var orderValues = new List<short>();
            foreach (var vmBgmEntry in _vmDictBgmDbRootEntries.Values.Where(p => !p.HiddenInSoundTest && bgmEntriesToReorder.Contains(p.UiBgmId)).OrderBy(p => p.TestDispOrder))
            {
                listSelectedVmBgms.Add(vmBgmEntry);
                if (vmBgmEntry.TestDispOrder < minAffected)
                    minAffected = vmBgmEntry.TestDispOrder;
                else if (vmBgmEntry.TestDispOrder > maxAffected)
                    maxAffected = vmBgmEntry.TestDispOrder;
                orderValues.Add(vmBgmEntry.TestDispOrder);
            }

            var listUnselectedButAffected = new List<BgmDbRootEntryViewModel>();
            foreach (var vmBgmEntry in _vmDictBgmDbRootEntries.Values
                .Where(p => !p.HiddenInSoundTest && p.TestDispOrder >= minAffected && p.TestDispOrder <= maxAffected && !bgmEntriesToReorder.Contains(p.UiBgmId))
                .OrderBy(p => p.TestDispOrder))
            {
                listUnselectedButAffected.Add(vmBgmEntry);
                orderValues.Add(vmBgmEntry.TestDispOrder);
            }

            orderValues = orderValues.OrderBy(p => p).ToList();

            for (short i = minAffected; i <= maxAffected; i++)
            {
                if (listUnselectedButAffected.Count > 0 && listUnselectedButAffected.First().TestDispOrder < newPosition)
                {
                    ReOrderVmBgmEntry(listUnselectedButAffected[0], orderValues[0]);
                    orderValues.RemoveAt(0);
                    listUnselectedButAffected.RemoveAt(0);
                }
                else
                {
                    if (listSelectedVmBgms.Count > 0)
                    {
                        ReOrderVmBgmEntry(listSelectedVmBgms[0], orderValues[0]);
                        orderValues.RemoveAt(0);
                        listSelectedVmBgms.RemoveAt(0);
                    }
                    else if (listUnselectedButAffected.Count > 0)
                    {
                        ReOrderVmBgmEntry(listUnselectedButAffected[0], orderValues[0]);
                        orderValues.RemoveAt(0);
                        listUnselectedButAffected.RemoveAt(0);
                    }
                }
            }
        }

        private void ReOrderVmBgmEntry(BgmDbRootEntryViewModel vmBgmEntry, short position)
        {
            vmBgmEntry.TestDispOrder = position;
            vmBgmEntry.GetReferenceEntity().TestDispOrder = position;
        }
    }
}
