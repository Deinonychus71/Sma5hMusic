using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using Sm5sh.GUI.Helpers;
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
using System.IO;

namespace Sm5sh.GUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IAudioStateService _audioState;
        private readonly IServiceProvider _serviceProvider;
        private readonly IFileDialog _fileDialog;
        private readonly ILogger _logger;
        private readonly IOptions<Sm5shOptions> _config;
        private readonly RangeObservableCollection<BgmEntry> _bgmEntries;

        public BgmSongsViewModel VMBgmSongs { get; }

        public MainWindowViewModel(IServiceProvider serviceProvider, IOptions<Sm5shOptions> config, IFileDialog fileDialog, 
            IAudioStateService audioState, IVGMMusicPlayer musicPlayer, ILogger<MainWindowViewModel> logger)
        {
            _serviceProvider = serviceProvider;
            _fileDialog = fileDialog;
            _logger = logger;
            _config = config;

            //DI
            _audioState = audioState;

            //Initialize observables
            _bgmEntries = new RangeObservableCollection<BgmEntry>();
            var observableBgmEntriesList = _bgmEntries.ToObservableChangeSet(p => p.ToneId)
                .Transform(p => new BgmEntryListViewModel(musicPlayer, p));

            //Initialize filters
            VMBgmSongs = ActivatorUtilities.CreateInstance<BgmSongsViewModel>(serviceProvider, observableBgmEntriesList);

            //Listen to request for adding new songs
            this.VMBgmSongs.WhenNewRequestToAddSong.Subscribe(async (o) => await AddNewBgmEntry(o));
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
            });
        }

        public async Task AddNewBgmEntry(string modPath)
        {
            var results = await _fileDialog.OpenFileDialogAudio();

            if (results.Length == 0)
                return;

            //TODO
            var tmpFolder = Path.Combine(_config.Value.TempPath, Path.GetDirectoryName(modPath));
            _logger.LogInformation("Adding {NbrFiles} files to Mod {ModPath}", results.Length, modPath);
            foreach (var inputFile in results)
            {
                //Adding entry to DB
                string inputFilename = Path.GetFileName(inputFile);
                /*var newBgmEntry = _audioState.AddBgmEntry(modPath, inputFilename);

                // Adding Audio to Temp Folder
                var tmpOutput = Path.Combine(tmpFolder, inputFilename);
                _logger.LogInformation("Adding {InputFile} to temporary path {OutputFile}.", inputFile);
                File.Copy(inputFile, tmpOutput);

                //Add to live DB
                _bgmEntries.Add(newBgmEntry);*/
            }
        }
    }
}
