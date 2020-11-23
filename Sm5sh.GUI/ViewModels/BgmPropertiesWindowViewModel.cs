using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmPropertiesWindowViewModel : ViewModelBase
    {
        private readonly ILogger _logger;

        public BgmPropertiesViewModel VMBgmProperties { get; }

        public BgmPropertiesWindowViewModel(IServiceProvider serviceProvider, ILogger<BgmPropertiesWindowViewModel> logger)
        {
            _logger = logger;

            VMBgmProperties = ActivatorUtilities.CreateInstance<BgmPropertiesViewModel>(serviceProvider, null);
        }
    }
}
