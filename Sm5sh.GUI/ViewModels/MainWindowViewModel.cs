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
using ReactiveUI;
using System.Reactive;

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
        private readonly IOptions<Sm5shOptions> _config;
        private readonly ObservableCollection<BgmEntry> _bgmEntries;
        private readonly ObservableCollection<IMusicMod> _musicMods;

        public ReactiveCommand<BgmEntryViewModel, Unit> ActionEditBgm { get; }

        public BgmSongsViewModel VMBgmSongs { get; }

        public MainWindowViewModel(IServiceProvider serviceProvider, IOptions<Sm5shOptions> config, IAudioStateService audioState, IVGMMusicPlayer musicPlayer,
            IMusicModManagerService musicModManagerService, IDialogWindow rootDialog, IFileDialog fileDialog, ILogger<MainWindowViewModel> logger)
        {
            _serviceProvider = serviceProvider;
            _musicModManagerService = musicModManagerService;
            _musicPlayer = musicPlayer;
            _fileDialog = fileDialog;
            _rootDialog = rootDialog;
            _logger = logger;
            _config = config;

            //DI
            _audioState = audioState;

            //Initialize observables
            _bgmEntries = new ObservableCollection<BgmEntry>();
            var observableBgmEntriesList = _bgmEntries.ToObservableChangeSet(p => p.ToneId)
                .Transform(p => new BgmEntryViewModel(musicPlayer, p));
            _musicMods = new ObservableCollection<IMusicMod>();
            var observableMusicModsList = _musicMods.ToObservableChangeSet(p => p.Mod.Id);

            //Initialize filters
            VMBgmSongs = ActivatorUtilities.CreateInstance<BgmSongsViewModel>(serviceProvider, observableBgmEntriesList, observableMusicModsList);

            //Listen to request for adding new songs
            this.VMBgmSongs.WhenNewRequestToAddSong.Subscribe(async (o) => await AddNewBgmEntry(o));

            //Commands
            ActionEditBgm = ReactiveCommand.CreateFromTask<BgmEntryViewModel>(EditBgmEntry);
        }

        public void ResetBgmList()
        {
            Task.Run(() =>
            {
                var stateManager = _serviceProvider.GetService<IStateManager>();
                var mods = _serviceProvider.GetServices<ISm5shMod>();
                stateManager.Init();
                foreach (var mod in mods)
                {
                    mod.Init();
                }
                var bgmList = _audioState.GetBgmEntries();
                
                _bgmEntries.AddRange(bgmList);
                _logger.LogInformation("BGM List Loaded.");
                _musicMods.AddRange(_musicModManagerService.MusicMods);
                _logger.LogInformation("Music Mods List Loaded.");
            });
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
                //Edit metadata
                await EditBgmEntry(new BgmEntryViewModel(_musicPlayer, newBgm));
                //Add to UI
                _bgmEntries.Add(newBgm);
            }
        }

        public async Task EditBgmEntry(BgmEntryViewModel bgmEntry)
        {
            var vmEditBgm = _serviceProvider.GetService<BgmPropertiesModalWindowViewModel>();
            vmEditBgm.LoadVMBgmEntry(bgmEntry);
            var modalEditBgmProps = new BgmPropertiesModalWindow() { DataContext = vmEditBgm };
            var results = await modalEditBgmProps.ShowDialog<BgmPropertiesModalWindow>(_rootDialog.Window);

            if (results != null)
            {
                //TODO
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
