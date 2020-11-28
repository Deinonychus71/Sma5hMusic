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

namespace Sm5sh.GUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IAudioStateService _audioState;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMusicModManagerService _musicModManagerService;
        private readonly IVGMMusicPlayer _musicPlayer;
        private readonly IFileDialog _fileDialog;
        private readonly IDialogWindow _rootDialog;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IOptions<Sm5shOptions> _config;
        private readonly ObservableCollection<LocaleViewModel> _locales;
        private readonly ObservableCollection<SeriesEntryViewModel> _seriesEntries;
        private readonly ObservableCollection<GameTitleEntryViewModel> _gameTitleEntries;
        private readonly ObservableCollection<BgmEntryViewModel> _bgmEntries;
        private readonly ObservableCollection<ModEntryViewModel> _musicMods;

        public BgmPropertiesModalWindowViewModel VMBgmEditor { get; }
        public GamePropertiesModalWindowViewModel VMGameEditor { get; }
        public ModPropertiesModalWindowViewModel VMModEditor { get; }
        public BgmSongsViewModel VMBgmSongs { get; }

        public MainWindowViewModel(IServiceProvider serviceProvider, IOptions<Sm5shOptions> config, IMapper mapper, IAudioStateService audioState, IVGMMusicPlayer musicPlayer,
            IMusicModManagerService musicModManagerService, IDialogWindow rootDialog, IFileDialog fileDialog, ILogger<MainWindowViewModel> logger)
        {
            _serviceProvider = serviceProvider;
            _musicModManagerService = musicModManagerService;
            _musicPlayer = musicPlayer;
            _fileDialog = fileDialog;
            _rootDialog = rootDialog;
            _logger = logger;
            _mapper = mapper;
            _config = config;

            //DI
            _audioState = audioState;

            //Initialize observables
            _locales = new ObservableCollection<LocaleViewModel>();
            var observableLocaleList = _locales.ToObservableChangeSet(p => p.Id);
            _seriesEntries = new ObservableCollection<SeriesEntryViewModel>();
            var observableSeriesEntriesList = _seriesEntries.ToObservableChangeSet(p => p.SeriesId);
            
            _bgmEntries = new ObservableCollection<BgmEntryViewModel>();
            var observableBgmEntriesList = _bgmEntries.ToObservableChangeSet(p => p.ToneId);
            _musicMods = new ObservableCollection<ModEntryViewModel>();
            var observableMusicModsList = _musicMods.ToObservableChangeSet(p => p.ModId);

            //Initialize filters
            VMBgmSongs = ActivatorUtilities.CreateInstance<BgmSongsViewModel>(serviceProvider, observableBgmEntriesList, 
                observableMusicModsList);

            //Initialize Editors
            _gameTitleEntries = new ObservableCollection<GameTitleEntryViewModel>();
            var observableGameEntriesList = _gameTitleEntries.ToObservableChangeSet(p => p.UiGameTitleId)
                .AutoRefreshOnObservable(p => VMBgmSongs.WhenLocaleChanged)
                .ForEachChange(o => o.Current.LoadLocalized(VMBgmSongs.SelectedLocale));
            VMGameEditor = ActivatorUtilities.CreateInstance<GamePropertiesModalWindowViewModel>(serviceProvider,
                observableLocaleList, observableSeriesEntriesList, observableGameEntriesList);
            VMModEditor = ActivatorUtilities.CreateInstance<ModPropertiesModalWindowViewModel>(serviceProvider,
                observableMusicModsList);
            VMBgmEditor = ActivatorUtilities.CreateInstance<BgmPropertiesModalWindowViewModel>(serviceProvider, 
                observableLocaleList,  observableSeriesEntriesList, observableGameEntriesList, observableBgmEntriesList);
            VMBgmEditor.VMGamePropertiesModal = VMGameEditor;

            //Listen to requests from children
            this.VMBgmSongs.WhenNewRequestToAddBgmEntry.Subscribe(async (o) => await AddNewBgmEntry(o));
            this.VMBgmSongs.VMBgmList.WhenNewRequestToEditBgmEntry.Subscribe(async (o) => await EditBgmEntry(o));
            this.VMBgmSongs.VMBgmList.WhenNewRequestToReorderBgmEntries.Subscribe((o) => ReorderSongs());
            this.VMGameEditor.WhenNewRequestToAddGameEntry.Subscribe((o) => AddNewGame(o));
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

                //Load
                var stateManager = _serviceProvider.GetService<IStateManager>();
                var mods = _serviceProvider.GetServices<ISm5shMod>();
                stateManager.Init();
                foreach (var mod in mods)
                {
                    mod.Init();
                }

                //Init view models
                var seriesList = _audioState.GetSeriesEntries().Select(p => new SeriesEntryViewModel(p)).ToDictionary(p => p.SeriesId, p => p);
                var gameList = _audioState.GetGameTitleEntries().Select(p => _mapper.Map(p, new GameTitleEntryViewModel(p) { SeriesViewModel = seriesList[p.UiSeriesId] })).ToDictionary(p => p.UiGameTitleId, p => p);
                var bgmList = _audioState.GetBgmEntries().Select(p => _mapper.Map(p, new BgmEntryViewModel(_musicPlayer, p) { GameTitleViewModel = gameList[p.GameTitleId] }));
                var localeList = _audioState.GetLocales().Select(p => new LocaleViewModel(p, Constants.GetLocaleDisplayName(p)));
                var modsList = _musicModManagerService.MusicMods.Select(p => new ModEntryViewModel(p.Id, p));

                //Bind
                _locales.AddRange(localeList);
                _logger.LogInformation("Locales Loaded.");
                _seriesEntries.AddRange(seriesList.Values);
                _logger.LogInformation("Series Loaded.");
                _gameTitleEntries.AddRange(gameList.Values);
                _logger.LogInformation("Game Titles Loaded.");
                _bgmEntries.AddRange(bgmList);
                _logger.LogInformation("BGM List Loaded.");
                _musicMods.AddRange(modsList);
                _logger.LogInformation("Music Mods List Loaded.");
            });
        }

        public void ReorderSongs()
        {
            short i = 0;
            var listBgms = _bgmEntries.Where(p => !p.HiddenInSoundTest).OrderBy(p => p.DbRoot.TestDispOrder);
            foreach (var bgmEntry in listBgms)
            {
                bgmEntry.DbRoot.TestDispOrder = i;
                i++;
                i++;
            }
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
                vmBgmEntry.LoadLocalized(VMBgmSongs.SelectedLocale);
                var bgmNew = _mapper.Map(vmBgmEntry, vmBgmEntry.GetBgmEntryReference());
                bgmNew.GameTitle = _mapper.Map(vmBgmEntry.GameTitleViewModel, new GameTitleEntry(vmBgmEntry.UiGameTitleId));
                vmBgmEntry.MusicMod.UpdateBgm(bgmNew);
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
