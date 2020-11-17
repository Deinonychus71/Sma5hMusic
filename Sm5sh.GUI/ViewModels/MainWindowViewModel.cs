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

        public ObservableCollection<BgmEntry> BgmEntries { get; }

        public MainWindowViewModel(IAudioStateService audioState, ILogger<MainWindowViewModel> logger)
        {
            _audioState = audioState;
            BgmEntries = new ObservableCollection<BgmEntry>(_audioState.GetBgmEntries());
        }

        public MainWindowViewModel()
        {
        }
    }
}
