namespace Sma5h.Mods.Music.Models
{
    public class StageEntry
    {
        public string UiStageId { get; set; }
        public string NameId { get; set; }
        public short SaveNo { get; set; }
        public string UiSeriesId { get; set; }
        public bool CanSelect { get; set; }
        public sbyte DispOrder { get; set; }
        public string StagePlaceId { get; set; }
        public string SecretStagePlaceId { get; set; }
        public bool CanDemo { get; set; }
        public bool Unk1 { get; set; }
        public bool IsUsableFlag { get; set; }
        public bool IsUsableAmiibo { get; set; }
        public string SecretCommandId { get; set; }
        public string SecretCommandIdJoycon { get; set; }
        public string BgmSetId { get; set; }
        public byte BgmSettingNo { get; set; }
        public bool BgmSelector { get; set; }
        public bool IsDlc { get; set; }
        public bool IsPatch { get; set; }
        public string DlcCharaId { get; set; }

        public override string ToString()
        {
            return UiStageId.ToString();
        }
    }
}
