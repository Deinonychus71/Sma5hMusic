using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using Sm5sh.GUI.Helpers;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using VGMMusic;

namespace Sm5sh.GUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IAudioStateService _audioState;
        private readonly RangeObservableCollection<BgmEntry> _bgmEntries;
        private readonly RangeObservableCollection<string> _locales = new RangeObservableCollection<string>(new List<string>() { "us_en", "jp_ja" });

        public BgmListViewModel VMBgmList { get; }
        public BgmFiltersViewModel VMBgmFilters { get; }
        public RangeObservableCollection<string> Locales { get { return _locales; } }
        [Reactive]
        public string SelectedLocale { get; set; }

        public MainWindowViewModel(IAudioStateService audioState, IVGMMusicPlayer musicPlayer, ILogger<MainWindowViewModel> logger)
        {
            SelectedLocale = Constants.DEFAULT_LOCALE;

            //DI
            _audioState = audioState;

            //Initialize observables
            _bgmEntries = new RangeObservableCollection<BgmEntry>();
            var whenLocaleChanged = this.WhenAnyValue(p => p.SelectedLocale);
            var observableBgmEntriesList = _bgmEntries.ToObservableChangeSet(p => p.ToneId)
                .Transform(p => new BgmEntryListViewModel(p))
                .AutoRefreshOnObservable(p => whenLocaleChanged)
                .ForEachChange(o => o.Current.LoadLocalized(SelectedLocale));

            //Initialize filters
            VMBgmFilters = new BgmFiltersViewModel(observableBgmEntriesList);

            //Initialize list
            VMBgmList = new BgmListViewModel(musicPlayer, VMBgmFilters.WhenFiltersAreApplied);
        }

        public void ResetBgmList()
        {
            var bgmList = _audioState.GetBgmEntries();
            _bgmEntries.AddRange(bgmList);
        }
    }
}
