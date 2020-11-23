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

namespace Sm5sh.GUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IAudioStateService _audioState;
        private readonly ILogger _logger;
        private readonly RangeObservableCollection<BgmEntry> _bgmEntries;

        public BgmSongsViewModel VMBgmSongs { get; }

        public MainWindowViewModel(IServiceProvider serviceProvider, IAudioStateService audioState, IVGMMusicPlayer musicPlayer, ILogger<MainWindowViewModel> logger)
        {
            _logger = logger;

            //DI
            _audioState = audioState;

            //Initialize observables
            _bgmEntries = new RangeObservableCollection<BgmEntry>();
            var observableBgmEntriesList = _bgmEntries.ToObservableChangeSet(p => p.ToneId)
                .Transform(p => new BgmEntryListViewModel(musicPlayer, p));

            //Initialize filters
            VMBgmSongs = ActivatorUtilities.CreateInstance<BgmSongsViewModel>(serviceProvider, observableBgmEntriesList);
        }

        public void ResetBgmList()
        {
            Task.Run(() =>
            {
                var stateManager = Locator.Current.GetService<IStateManager>();
                var mods = Locator.Current.GetServices<ISm5shMod>();
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
    }
}
