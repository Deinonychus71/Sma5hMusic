using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sm5sh.Data.Sound.Config;
using Sm5sh.Data.Ui.Param.Database;
using Sm5sh.Services.FileSpecific.Interfaces;
using Sm5sh.Services.Interfaces;

namespace Sm5sh.Services
{
    public class StateService : IStateService
    {
        private readonly ILogger _logger;
        private readonly IOptions<Options> _config;
        private readonly IPrcService _prcService;
        private readonly IBgmPropertyService _bgmPropertyService;

        public PrcUiBgmDatabase UiBgmDatabase { get; private set; }
        public PrcUiGameTitleDatabase UiGameTitleDatabase { get; private set; }
        public PrcUiStageDatabase UiStageDatabase { get; private set; }
        public BinBgmProperty BgmProperty { get; private set; }

        public StateService(IOptions<Options> config, IPrcService prcService, IBgmPropertyService bgmPropertyService, ILogger<IStateService> logger)
        {
            _logger = logger;
            _config = config;
            _prcService = prcService;
            _bgmPropertyService = bgmPropertyService;
            UiBgmDatabase = _prcService.ReadPrcFile<PrcUiBgmDatabase>(_config.Value.PrcUiBgmDbResourceFile);
            UiGameTitleDatabase = _prcService.ReadPrcFile<PrcUiGameTitleDatabase>(_config.Value.PrcUiGameTitleDbResourceFile);
            UiStageDatabase = _prcService.ReadPrcFile<PrcUiStageDatabase>(_config.Value.PrcUiStageDbResourceFile);
            BgmProperty = _bgmPropertyService.ReadBgmPropertyFile(_config.Value.ToolsBgmPropertyExeFile, _config.Value.ToolsBgmPropertyHashesFile, _config.Value.BinBgmPropertyResourceFile, _config.Value.TempPath);
        }

        public bool WriteChanges()
        {
            _logger.LogInformation("Write State Changes to {OutputPath}", _config.Value.OutputPath);

            _prcService.WritePrcFile(_config.Value.OutputPrcUiBgmDbResourceFile, UiBgmDatabase);
            _prcService.WritePrcFile(_config.Value.OutputPrcUiGameTitleDbResourceFile, UiGameTitleDatabase);
            _prcService.WritePrcFile(_config.Value.OutputPrcUiStageDbResourceFile, UiStageDatabase);
            _bgmPropertyService.WriteBgmPropertyFile(_config.Value.ToolsBgmPropertyExeFile, _config.Value.OutputBinBgmPropertyResourceFile, _config.Value.TempPath, BgmProperty);

            return true;
        }
    }
}
