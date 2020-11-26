using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reactive.Subjects;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmPropertiesModalWindowViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private readonly Subject<BgmEntryViewModel> _vmBgmEntrySubject;

        public BgmPropertiesViewModel VMBgmProperties { get; }

        public BgmPropertiesModalWindowViewModel(IServiceProvider serviceProvider, ILogger<BgmPropertiesModalWindowViewModel> logger)
        {
            _logger = logger;

            _vmBgmEntrySubject = new Subject<BgmEntryViewModel>();
            VMBgmProperties = ActivatorUtilities.CreateInstance<BgmPropertiesViewModel>(serviceProvider, _vmBgmEntrySubject);
        }

        public void LoadVMBgmEntry(BgmEntryViewModel vmBgmEntry)
        {
            _logger.LogDebug("Loading {ToneId} for edit", vmBgmEntry.ToneId);
            _vmBgmEntrySubject.OnNext(vmBgmEntry);
        }
    }
}
