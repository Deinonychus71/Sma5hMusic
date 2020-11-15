using Sm5sh.Data.Sound.Config;
using Sm5sh.Data.Ui.Param.Database;

namespace Sm5sh.Services.Interfaces
{
    public interface IStateService
    {
        PrcUiBgmDatabase UiBgmDatabase { get; }
        PrcUiGameTitleDatabase UiGameTitleDatabase { get; }
        PrcUiStageDatabase UiStageDatabase { get; }
        BinBgmProperty BgmProperty { get; }

        bool WriteChanges();
    }
}
