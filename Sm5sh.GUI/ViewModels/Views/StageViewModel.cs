using DynamicData;
using Microsoft.Extensions.Logging;
using ReactiveUI.Fody.Helpers;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using System;
using System.Collections.Generic;

namespace Sm5sh.GUI.ViewModels
{
    public class StageViewModel : ViewModelBase
    {
        private readonly ILogger _logger;

        [Reactive]
        public string SelectedLocale { get; set; }

        public StageViewModel(ILogger<StageViewModel> logger)
        {
            _logger = logger;
        }
    }
}
