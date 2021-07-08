using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sma5hMusic.GUI.ViewModels.ReactiveObjects.OrderTreeViewItems
{
    public class OrderItemValueViewModel : OrderItemViewModel
    {
        public string Path { get; set; }
        public BgmDbRootEntryViewModel BgmEntry { get; set; }
    }
}
