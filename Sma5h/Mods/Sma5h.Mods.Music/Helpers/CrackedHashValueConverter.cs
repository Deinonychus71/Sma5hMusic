using Sma5h.Mods.Music.MusicMods.MusicModModels;
using Sma5h.Mods.Music.MusicOverride.MusicOverrideConfigModels;

namespace Sma5h.Mods.Music.Helpers
{
    public static class CrackedHashValueConverter
    {
        public static void UpdateBgmDbRootConfig(BgmDbRootConfig bgmBbRootEntry)
        {
            if (bgmBbRootEntry.Unk1 != null)
                bgmBbRootEntry.IsSelectableMovieEdit = bgmBbRootEntry.Unk1.Value;
            if (bgmBbRootEntry.Unk2 != null)
                bgmBbRootEntry.IsSelectableOriginal = bgmBbRootEntry.Unk2.Value;
            if (!string.IsNullOrEmpty(bgmBbRootEntry.Unk3))
                bgmBbRootEntry.DlcUiCharaId = bgmBbRootEntry.Unk3;
            if (!string.IsNullOrEmpty(bgmBbRootEntry.Unk4))
                bgmBbRootEntry.DlcMiiHatMotifId = bgmBbRootEntry.Unk4;
            if (!string.IsNullOrEmpty(bgmBbRootEntry.Unk5))
                bgmBbRootEntry.DlcMiiBodyMotifId = bgmBbRootEntry.Unk5;
        }

        public static void UpdateAssignedInfoConfig(BgmAssignedInfoConfig bgmAssignedInfoEntry)
        {
            if (bgmAssignedInfoEntry.Unk1 != null)
                bgmAssignedInfoEntry.MenuChangeStopDelayFrame = bgmAssignedInfoEntry.Unk1.Value;
        }

        public static void UpdateStageConfig(StageConfig stageEntry)
        {
            if (stageEntry.Unk2 != null)
                stageEntry.IsUsableFlag = stageEntry.Unk2.Value;
            if (stageEntry.Unk3 != null)
                stageEntry.IsUsableAmiibo = stageEntry.Unk3.Value;
            if (stageEntry.Unk4 != null)
                stageEntry.BgmSelector = stageEntry.Unk4.Value;
        }
    }
}
