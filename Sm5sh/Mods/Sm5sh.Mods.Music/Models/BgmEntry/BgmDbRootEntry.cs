using Sm5sh.Mods.Music.Helpers;
using Sm5sh.Mods.Music.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Sm5sh.Mods.Music.Models
{
    public class BgmDbRootEntry : BgmBase
    {
        public string UiBgmId { get; }
        public string StreamSetId { get; set; }
        public string Rarity { get; set; }
        public string RecordType { get; set; }
        public string UiGameTitleId { get; set; }
        public string UiGameTitleId1 { get; set; }
        public string UiGameTitleId2 { get; set; }
        public string UiGameTitleId3 { get; set; }
        public string UiGameTitleId4 { get; set; }
        public string NameId { get; set; }
        public short SaveNo { get; set; }
        public short TestDispOrder { get; set; }
        public int MenuValue { get; set; }
        public bool JpRegion { get; set; }
        public bool OtherRegion { get; set; }
        public bool Possessed { get; set; }
        public bool PrizeLottery { get; set; }
        public uint ShopPrice { get; set; }
        public bool CountTarget { get; set; }
        public byte MenuLoop { get; set; }
        public bool IsSelectableStageMake { get; set; }
        public bool Unk1 { get; set; }
        public bool Unk2 { get; set; }
        public bool IsDlc { get; set; }
        public bool IsPatch { get; set; }
        public string Unk3 { get; set; }
        public string Unk4 { get; set; }
        public string Unk5 { get; set; }

        public Dictionary<string, string> Title { get; set; }
        public Dictionary<string, string> Copyright { get; set; }
        public Dictionary<string, string> Author { get; set; }
        public string TitleKey { get { return !string.IsNullOrEmpty(NameId) ? string.Format(Constants.InternalIds.MSBT_BGM_TITLE, NameId) : null; } }
        public string AuthorKey { get { return !string.IsNullOrEmpty(NameId) ? string.Format(Constants.InternalIds.MSBT_BGM_AUTHOR, NameId) : null; } }
        public string CopyrightKey { get { return !string.IsNullOrEmpty(NameId) ? string.Format(Constants.InternalIds.MSBT_BGM_COPYRIGHT, NameId) : null; } }


        public BgmDbRootEntry(string bgmId, IMusicMod musicMod = null)
            : base(musicMod)
        {
            UiBgmId = bgmId;
            Rarity = Constants.InternalIds.RARITY_DEFAULT;
            RecordType = Constants.InternalIds.RECORD_TYPE_DEFAULT;
            UiGameTitleId1 = Constants.InternalIds.GAME_TITLE_ID_DEFAULT;
            UiGameTitleId2 = Constants.InternalIds.GAME_TITLE_ID_DEFAULT;
            UiGameTitleId3 = Constants.InternalIds.GAME_TITLE_ID_DEFAULT;
            UiGameTitleId4 = Constants.InternalIds.GAME_TITLE_ID_DEFAULT;
            SaveNo = -1;
            TestDispOrder = short.MaxValue;
            JpRegion = true;
            OtherRegion = true;
            Possessed = true;
            PrizeLottery = false;
            ShopPrice = 0;
            CountTarget = true;
            MenuLoop = 1;
            IsSelectableStageMake = true;
            Unk1 = true;
            Unk2 = true;
            IsDlc = false;
            IsPatch = false;
            Title = new Dictionary<string, string>();
            Author = new Dictionary<string, string>();
            Copyright = new Dictionary<string, string>();
        }

        public bool ContainsValidLabels
        {
            get
            {
                return
                    (Title != null && Title.Values.Any(p => !string.IsNullOrEmpty(p))) ||
                    (Author != null && Author.Values.Any(p => !string.IsNullOrEmpty(p))) ||
                    (Copyright != null && Copyright.Values.Any(p => !string.IsNullOrEmpty(p)));
            }
        }

        public override string ToString()
        {
            return UiBgmId;
        }
    }
}
