using ReactiveUI;
using System.Collections.Generic;
using System.Linq;

namespace Sma5hMusic.GUI.ViewModels.ReactiveObjects
{
    public class OrderItemTreeViewModel : ReactiveObject
    {
        public string LowerId { get; set; }

        public string UpperId { get; set; }

        public bool IsExpanded { get; set; }

        public string Label { get; set; }

        public string NbrBgms { get; set; }

        public short LowerTestDisp
        {
            get
            {
                var val = BgmEntries.FirstOrDefault()?.TestDispOrder;
                if (val == null)
                    return 0;
                return val.Value;
            }
        }

        public short UpperTestDisp
        {
            get
            {
                var val = BgmEntries.LastOrDefault()?.TestDispOrder;
                if (val == null)
                    return 0;
                return val.Value;
            }
        }

        public IEnumerable<OrderItemTreeViewModel> Items { get; set; }

        public IEnumerable<BgmDbRootEntryViewModel> BgmEntries { get; set; }

        public OrderItemTreeViewModel()
        {
        }
    }
}