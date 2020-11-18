using Microsoft.Extensions.Logging;
using Sm5sh.GUI.Models;
using Sm5sh.Interfaces;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Sm5sh.GUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IAudioStateService _audioState;

        public BgmListViewModel BgmList { get; }

        public MainWindowViewModel(IAudioStateService audioState, ILogger<MainWindowViewModel> logger)
        {
            _audioState = audioState;
            BgmList = new BgmListViewModel(_audioState.GetBgmEntries());
        }
    }
}
