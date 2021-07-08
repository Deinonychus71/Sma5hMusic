using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sma5hMusic.GUI.ViewModels.ReactiveObjects.OrderTreeViewItems
{
    public class OrderItemTreeItemViewModel : OrderItemViewModel
    {
        public List<OrderItemViewModel> Items { get; set; }
        public string NbrBgms { get; set; }
    }
}
