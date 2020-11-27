using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using System;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmPropertiesModalWindowViewModel : ViewModelBase
    {
        private readonly ILogger _logger;

        [Reactive]
        public BgmEntryViewModel SelectedBgmEntry { get; set; }

        [Reactive]
        public ReadOnlyObservableCollection<SeriesEntryViewModel> Series { get; set; }
        [Reactive]
        public ReadOnlyObservableCollection<GameTitleEntryViewModel> Games { get; set; }

        public BgmPropertiesModalWindowViewModel(ILogger<BgmPropertiesModalWindowViewModel> logger)
        {

            _logger = logger;
            SelectedBgmEntry = new BgmEntryViewModel();
        }
    }
}
