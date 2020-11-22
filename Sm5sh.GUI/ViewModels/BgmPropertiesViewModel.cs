using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using VGMMusic;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmPropertiesViewModel : ViewModelBase
    {
        private readonly IVGMMusicPlayer _musicPlayer;


        [Reactive]
        public BgmEntryListViewModel SelectedBgmEntry { get; set; }

        public BgmPropertiesViewModel(IVGMMusicPlayer musicPlayer, IObservable<BgmEntryListViewModel> observableBgmEntry)
        {
            _musicPlayer = musicPlayer;

            observableBgmEntry.Subscribe(o => SelectedBgmEntry = o);
        }
    }
}
