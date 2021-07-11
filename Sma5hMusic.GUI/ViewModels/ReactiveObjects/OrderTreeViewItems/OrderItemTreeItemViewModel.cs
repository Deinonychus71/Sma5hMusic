using System.Collections.Generic;

namespace Sma5hMusic.GUI.ViewModels.ReactiveObjects.OrderTreeViewItems
{
    public class OrderItemTreeItemViewModel : OrderItemViewModel
    {
        public string NbrBgms { get; set; }

        public List<OrderItemViewModel> Items { get; set; }

        public OrderItemTreeItemViewModel()
        {
        }
    }
}