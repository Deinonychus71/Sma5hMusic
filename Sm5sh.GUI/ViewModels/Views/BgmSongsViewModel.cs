using DynamicData;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmSongsViewModel : ViewModelBase
    {
        private readonly ILogger _logger;

        public BgmListViewModel VMBgmList { get; }
        public BgmPropertiesViewModel VMBgmProperties { get; }

        public BgmSongsViewModel(IServiceProvider serviceProvider, ILogger<BgmSongsViewModel> logger, 
            IObservable<IChangeSet<BgmEntryViewModel, string>> observableBgmEntriesList)
        {
            _logger = logger;

            //Initialize list
            VMBgmList = ActivatorUtilities.CreateInstance<BgmListViewModel>(serviceProvider, observableBgmEntriesList);

            //Initialize properties
            var whenSelectedBgmEntryChanged = this.WhenAnyValue(p => p.VMBgmList.SelectedBgmEntry);
            VMBgmProperties = ActivatorUtilities.CreateInstance<BgmPropertiesViewModel>(serviceProvider, whenSelectedBgmEntryChanged);
        }
    }
}
