using DynamicData;
using Microsoft.Extensions.Logging;
using ReactiveUI.Fody.Helpers;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using System;
using System.Collections.Generic;

namespace Sm5sh.GUI.ViewModels
{
    public class PlaylistViewModel : ViewModelBase
    {
        private readonly ILogger _logger;

        [Reactive]
        public string SelectedLocale { get; set; }

        public PlaylistViewModel(ILogger<PlaylistViewModel> logger)
        {
            _logger = logger;
        }
    }
}
