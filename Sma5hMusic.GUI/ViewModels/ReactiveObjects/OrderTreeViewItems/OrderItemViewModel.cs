using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sma5hMusic.GUI.ViewModels.ReactiveObjects.OrderTreeViewItems
{
    public class OrderItemViewModel : ReactiveObject
    {
        public string Label { get; set; }

        [Reactive]
        public int Order { get; set; }
    }
}
