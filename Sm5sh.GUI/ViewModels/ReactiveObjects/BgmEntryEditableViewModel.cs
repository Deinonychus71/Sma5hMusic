using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sm5sh.GUI.ViewModels;
using Sm5sh.Mods.Music.Models;
using System.Collections.Generic;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmEntryEditableViewModel : ReactiveObject
    {
        protected readonly BgmEntry _refBgmEntry;

        public string ToneId { get { return _refBgmEntry.ToneId; } }
        public string Filename { get { return _refBgmEntry.Filename; } }
        [Reactive]
        public GameTitleEntryViewModel GameTitleViewModel { get; set; }
        public BgmEntryEditableViewModels.NUS3BankConfigEntryEditableViewModel NUS3BankConfig { get; }
        public BgmEntryEditableViewModels.MSBTLabelsEntryEditableViewModel MSBTLabels { get; }
        public BgmEntryEditableViewModels.BgmPropertyEntryEditableViewModel BgmProperties { get; }
        public BgmEntryEditableViewModels.BgmDbRootEntryEditableViewModel DbRoot { get; set; }
        public BgmEntryEditableViewModels.BgmAssignedInfoEntryEditableViewModel AssignedInfo { get; }
        public BgmEntryEditableViewModels.BgmStreamSetEntryEditableViewModel StreamSet { get; }
        public BgmEntryEditableViewModels.BgmStreamPropertyEntryEditableViewModel StreamingProperty { get; }

        public BgmEntryEditableViewModel() {
            MSBTLabels = new BgmEntryEditableViewModels.MSBTLabelsEntryEditableViewModel(this);
            BgmProperties = new BgmEntryEditableViewModels.BgmPropertyEntryEditableViewModel(this);
            NUS3BankConfig = new BgmEntryEditableViewModels.NUS3BankConfigEntryEditableViewModel(this);
            DbRoot = new BgmEntryEditableViewModels.BgmDbRootEntryEditableViewModel(this);
            AssignedInfo = new BgmEntryEditableViewModels.BgmAssignedInfoEntryEditableViewModel(this);
            StreamSet = new BgmEntryEditableViewModels.BgmStreamSetEntryEditableViewModel(this);
            StreamingProperty = new BgmEntryEditableViewModels.BgmStreamPropertyEntryEditableViewModel(this);
        }

        public BgmEntryEditableViewModel(BgmEntry bgmEntry)
        {
            _refBgmEntry = bgmEntry;
            MSBTLabels = new BgmEntryEditableViewModels.MSBTLabelsEntryEditableViewModel(this);
            BgmProperties = new BgmEntryEditableViewModels.BgmPropertyEntryEditableViewModel(this);
            NUS3BankConfig = new BgmEntryEditableViewModels.NUS3BankConfigEntryEditableViewModel(this);
            DbRoot = new BgmEntryEditableViewModels.BgmDbRootEntryEditableViewModel(this);
            AssignedInfo = new BgmEntryEditableViewModels.BgmAssignedInfoEntryEditableViewModel(this);
            StreamSet = new BgmEntryEditableViewModels.BgmStreamSetEntryEditableViewModel(this);
            StreamingProperty = new BgmEntryEditableViewModels.BgmStreamPropertyEntryEditableViewModel(this);
        }

        public BgmEntry GetBgmEntryReference()
        {
            return _refBgmEntry;
        }

        public override string ToString()
        {
            return ToneId;
        }
    }

    namespace BgmEntryEditableViewModels
    {
        public class BgmPropertyEntryEditableViewModel : ReactiveObject
        {
            public BgmEntryEditableViewModel Parent { get; }
            public string NameId { get { return Parent.ToneId; } }
            public ulong LoopStartMs { get; set; }
            public ulong LoopStartSample { get; set; }
            public ulong LoopEndMs { get; set; }
            public ulong LoopEndSample { get; set; }
            public ulong TotalTimeMs { get; set; }
            public ulong TotalSamples { get; set; }
            public ulong Frequency { get { return TotalTimeMs / 1000 * TotalSamples; } }

            public BgmPropertyEntryEditableViewModel(BgmEntryEditableViewModel parent)
            {
                Parent = parent;
            }
        }

        public class NUS3BankConfigEntryEditableViewModel : ReactiveObject
        {
            public BgmEntryEditableViewModel Parent { get; }
            [Reactive]
            public float AudioVolume { get; set; }

            public NUS3BankConfigEntryEditableViewModel(BgmEntryEditableViewModel parent)
            {
                Parent = parent;
            }
        }

        public class MSBTLabelsEntryEditableViewModel : ReactiveObject
        {
            public BgmEntryEditableViewModel Parent { get; }
            public Dictionary<string, string> Title { get; set; }
            public Dictionary<string, string> Copyright { get; set; }
            public Dictionary<string, string> Author { get; set; }

            public MSBTLabelsEntryEditableViewModel(BgmEntryEditableViewModel parent)
            {
                Parent = parent;
            }
        }

        public class BgmDbRootEntryEditableViewModel : ReactiveObject
        {
            public BgmEntryEditableViewModel Parent { get; }

            public string UiBgmId { get; set; }
            public string StreamSetId { get; set; }
            public string Rarity { get; set; }
            [Reactive]
            public string RecordType { get; set; }
            public string UiGameTitleId { get; set; }
            public string UiGameTitleId1 { get; set; }
            public string UiGameTitleId2 { get; set; }
            public string UiGameTitleId3 { get; set; }
            public string UiGameTitleId4 { get; set; }
            public string NameId { get; set; }
            public short SaveNo { get; set; }
            [Reactive]
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

            public BgmDbRootEntryEditableViewModel(BgmEntryEditableViewModel parent)
            {
                Parent = parent;
            }
        }

        public class BgmStreamSetEntryEditableViewModel : ReactiveObject
        {
            public BgmEntryEditableViewModel Parent { get; }

            public string StreamSetId { get; set; }
            [Reactive]
            public string SpecialCategory { get; set; }
            public string Info0 { get; set; }
            [Reactive]
            public string Info1 { get; set; }
            public string Info2 { get; set; }
            public string Info3 { get; set; }
            public string Info4 { get; set; }
            public string Info5 { get; set; }
            public string Info6 { get; set; }
            public string Info7 { get; set; }
            public string Info8 { get; set; }
            public string Info9 { get; set; }
            public string Info10 { get; set; }
            public string Info11 { get; set; }
            public string Info12 { get; set; }
            public string Info13 { get; set; }
            public string Info14 { get; set; }
            public string Info15 { get; set; }

            public BgmStreamSetEntryEditableViewModel(BgmEntryEditableViewModel parent)
            {
                Parent = parent;
            }
        }

        public class BgmAssignedInfoEntryEditableViewModel : ReactiveObject
        {
            public BgmEntryEditableViewModel Parent { get; }
            public string InfoId { get; set; }
            public string StreamId { get; set; }
            public string Condition { get; set; }
            public string ConditionProcess { get; set; }
            public int StartFrame { get; set; }
            public int ChangeFadeInFrame { get; set; }
            public int ChangeStartDelayFrame { get; set; }
            public int ChangeFadoutFrame { get; set; }
            public int ChangeStopDelayFrame { get; set; }
            public int MenuChangeFadeInFrame { get; set; }
            public int MenuChangeStartDelayFrame { get; set; }
            public int MenuChangeFadeOutFrame { get; set; }
            public int Unk1 { get; set; }

            public BgmAssignedInfoEntryEditableViewModel(BgmEntryEditableViewModel parent)
            {
                Parent = parent;
            }
        }

        public class BgmStreamPropertyEntryEditableViewModel : ReactiveObject
        {
            public BgmEntryEditableViewModel Parent { get; }

            public string StreamId { get; set; }
            public string DateName0 { get { return Parent.ToneId; } }
            public string DateName1 { get; set; }
            public string DateName2 { get; set; }
            public string DateName3 { get; set; }
            public string DateName4 { get; set; }
            public byte Loop { get; set; }
            public string EndPoint { get; set; }
            public ushort FadeOutFrame { get; set; }
            public string StartPointSuddenDeath { get; set; }
            public string StartPointTransition { get; set; }
            public string StartPoint0 { get; set; }
            public string StartPoint1 { get; set; }
            public string StartPoint2 { get; set; }
            public string StartPoint3 { get; set; }
            public string StartPoint4 { get; set; }

            public BgmStreamPropertyEntryEditableViewModel(BgmEntryEditableViewModel parent)
            {
                Parent = parent;
            }
        }
    }
}
