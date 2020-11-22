using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using Sm5sh.GUI.Helpers;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using System.Collections.Generic;
using VGMMusic;
using Sm5sh.Interfaces;
using Splat;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using System;
using System.Reactive;

namespace Sm5sh.GUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IAudioStateService _audioState;
        private readonly ILogger _logger;
        private readonly RangeObservableCollection<BgmEntry> _bgmEntries;
        private readonly RangeObservableCollection<string> _locales = new RangeObservableCollection<string>(new List<string>() { "us_en", "jp_ja" });

        public BgmListViewModel VMBgmList { get; }
        public BgmFiltersViewModel VMBgmFilters { get; }
        public BgmPropertiesViewModel VMBgmProperties { get; }
        public RangeObservableCollection<string> Locales { get { return _locales; } }
        [Reactive]
        public string SelectedLocale { get; set; }

        public MainWindowViewModel(IAudioStateService audioState, IVGMMusicPlayer musicPlayer, ILogger<MainWindowViewModel> logger)
        {
            _logger = logger;
            SelectedLocale = Constants.DEFAULT_LOCALE;

            //DI
            _audioState = audioState;

            //Initialize observables
            _bgmEntries = new RangeObservableCollection<BgmEntry>();
            var whenLocaleChanged = this.WhenAnyValue(p => p.SelectedLocale);
            var observableBgmEntriesList = _bgmEntries.ToObservableChangeSet(p => p.ToneId)
                .Transform(p => new BgmEntryListViewModel(musicPlayer, p))
                .AutoRefreshOnObservable(p => whenLocaleChanged)
                .ForEachChange(o => o.Current.LoadLocalized(SelectedLocale));

            //Initialize filters
            VMBgmFilters = new BgmFiltersViewModel(observableBgmEntriesList);

            //Initialize list
            VMBgmList = new BgmListViewModel(musicPlayer, VMBgmFilters.WhenFiltersAreApplied);

            //Initialize properties
            var whenSelectedBgmEntryChanged = this.WhenAnyValue(p => p.VMBgmList.SelectedBgmEntry);
            VMBgmProperties = new BgmPropertiesViewModel(musicPlayer, whenSelectedBgmEntryChanged);
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
