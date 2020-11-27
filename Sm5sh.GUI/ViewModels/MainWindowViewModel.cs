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
        private readonly ObservableCollection<IMusicMod> _musicMods;

        public BgmPropertiesModalWindowViewModel VMBgmEditor { get; }
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
            _gameTitleEntries = new ObservableCollection<GameTitleEntryViewModel>();
            var observableGameEntriesList = _gameTitleEntries.ToObservableChangeSet(p => p.UiGameTitleId);
            _bgmEntries = new ObservableCollection<BgmEntryViewModel>();
            var observableBgmEntriesList = _bgmEntries.ToObservableChangeSet(p => p.ToneId);
            _musicMods = new ObservableCollection<IMusicMod>();
            var observableMusicModsList = _musicMods.ToObservableChangeSet(p => p.Mod.Id);

            //Initialize filters
            VMBgmSongs = ActivatorUtilities.CreateInstance<BgmSongsViewModel>(serviceProvider, observableBgmEntriesList, 
                observableMusicModsList);

            //Initialize BGM Editor
            VMBgmEditor = ActivatorUtilities.CreateInstance<BgmPropertiesModalWindowViewModel>(serviceProvider, 
                observableLocaleList,  observableSeriesEntriesList, observableGameEntriesList, observableBgmEntriesList);

            //Listen to requests from children
            this.VMBgmSongs.WhenNewRequestToAddBgmEntry.Subscribe(async (o) => await AddNewBgmEntry(o));
            this.VMBgmSongs.VMBgmList.WhenNewRequestToEditBgmEntry.Subscribe(async (o) => await EditBgmEntry(o));
            this.VMBgmSongs.VMBgmList.WhenNewRequestToReorderBgmEntries.Subscribe((o) => ReorderSongs());
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
                var locales = _audioState.GetLocales().Select(p => new LocaleViewModel(p, Constants.GetLocaleDisplayName(p)));

                //Bind
                _locales.AddRange(locales);
                _logger.LogInformation("Locales Loaded.");
                _seriesEntries.AddRange(seriesList.Values);
                _logger.LogInformation("Series Loaded.");
                _gameTitleEntries.AddRange(gameList.Values);
                _logger.LogInformation("Game Titles Loaded.");
                _bgmEntries.AddRange(bgmList);
                _logger.LogInformation("BGM List Loaded.");
                _musicMods.AddRange(_musicModManagerService.MusicMods);
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

        public async Task AddNewBgmEntry(IMusicMod managerMod)
        {
            if (managerMod == null)
                managerMod = await CreateNewMod();

            if (managerMod == null)
                return;

            var results = await _fileDialog.OpenFileDialogAudio();
            if (results.Length == 0)
                return;

            _logger.LogInformation("Adding {NbrFiles} files to Mod {ModPath}", results.Length, managerMod.ModPath);
            foreach (var inputFile in results)
            {
                var newBgm = managerMod.AddBgm(inputFile);
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

        public async Task EditBgmEntry(BgmEntryViewModel bgmEntry)
        {
            VMBgmEditor.LoadBgmEntry(bgmEntry);
            var modalEditBgmProps = new BgmPropertiesModalWindow() { DataContext = VMBgmEditor };
            var results = await modalEditBgmProps.ShowDialog<BgmPropertiesModalWindow>(_rootDialog.Window);

            if (results != null)
            {
                bgmEntry.LoadLocalized(VMBgmSongs.SelectedLocale);
            }
        }

        public async Task<IMusicMod> CreateNewMod()
        {
            var vmCreateMod = _serviceProvider.GetService<ModPropertiesModalWindowViewModel>();
            var modalCreateMod = new ModPropertiesModalWindow() { DataContext = vmCreateMod };
            var results = await modalCreateMod.ShowDialog<ModPropertiesModalWindow>(_rootDialog.Window);

            if (results != null)
            {
                var newManagerMod = _musicModManagerService.AddMusicMod(new MusicModInformation()
                {
                    Name = vmCreateMod.ModName,
                    Author = vmCreateMod.ModAuthor,
                    Website = vmCreateMod.ModWebsite,
                    Description = vmCreateMod.ModDescription
                }, vmCreateMod.ModPath);

                _musicMods.Add(newManagerMod);
                return newManagerMod;
            }
            return null;
        }
    }
}
