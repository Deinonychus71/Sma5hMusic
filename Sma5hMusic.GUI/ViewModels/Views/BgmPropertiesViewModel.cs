using Avalonia.Controls;
using Avalonia.Input;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Interactivity;
using Avalonia.Controls.Primitives;
using System.Linq;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Subjects;
using System.Collections.Generic;
using Sma5hMusic.GUI.ViewModels.ReactiveObjects;
using Sma5hMusic.GUI.Helpers;

namespace Sma5hMusic.GUI.ViewModels
{
    public class BgmPropertiesViewModel : ViewModelBase
    {
        private ReadOnlyObservableCollection<BgmDbRootEntryViewModel> _items;
        private readonly Subject<Tuple<IEnumerable<string>, short>> _whenNewRequestToReorderBgmEntries;
        private readonly HashSet<string> _expandedCache;
        private TreeView _treeView;

        public IObservable<Tuple<IEnumerable<string>, short>> WhenNewRequestToReorderBgmEntries { get { return _whenNewRequestToReorderBgmEntries; } }

        public ObservableCollection<OrderItemTreeViewModel> Items { get; }

        [Reactive]
        public string Header { get; set; }
        public ReactiveCommand<TreeView, Unit> ActionInitializeDragAndDrop { get; }
        public ReactiveCommand<TabControl, Unit> ActionSelectedTabChanged { get; }

        [Reactive]
        public BgmDbRootEntryViewModel SelectedBgmEntry { get; set; }

        public BgmPropertiesViewModel(IObservable<IChangeSet<BgmDbRootEntryViewModel, string>> observableBgmEntriesList, IObservable<BgmDbRootEntryViewModel> observableBgmEntry)
        {
            _whenNewRequestToReorderBgmEntries = new Subject<Tuple<IEnumerable<string>, short>>();
            _expandedCache = new HashSet<string>();

            Items = new ObservableCollection<OrderItemTreeViewModel>();

            observableBgmEntriesList
                .DeferUntilLoaded()
                .Filter(p => !p.HiddenInSoundTest)
                .AutoRefresh(p => p.TestDispOrder, TimeSpan.FromMilliseconds(50))
                .Sort(SortExpressionComparer<BgmDbRootEntryViewModel>.Ascending(p => p.TestDispOrder), SortOptimisations.ComparesImmutableValuesOnly, 1)
                .TreatMovesAsRemoveAdd()
                .Throttle(TimeSpan.FromMilliseconds(50))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _items)
                .DisposeMany()
                .Subscribe((o) =>
                {
                    _expandedCache.Clear();
                    foreach (var item in Items)
                    {
                        if (item.IsExpanded)
                        {
                            _expandedCache.Add(item.LowerId);
                            _expandedCache.Add(item.UpperId);
                        }
                    }

                    var items = _items.ChunkBy(p => p.SeriesId).Select(p =>
                    {
                        var bgmsPerSeries = p.ToList();
                        var seriesTitle = bgmsPerSeries[0].SeriesViewModel.Title;
                        var chunkBits = bgmsPerSeries.ChunkBy(s => s.UiGameTitleId).Select(p =>
                        {
                            var bgmsPerGame = p.ToList();
                            var gameTitle = bgmsPerGame[0].GameTitleViewModel.Title;

                            return new OrderItemTreeViewModel()
                            {
                                NbrBgms = $"({bgmsPerGame.Count} bgms)",
                                BgmEntries = bgmsPerGame,
                                Label = $"[{gameTitle}]"
                            };
                        }).ToList();

                        var lowerNodeId = $"{p.Key}_{bgmsPerSeries[0].UiBgmId}";
                        var upperNodeId = $"{p.Key}_{bgmsPerSeries[bgmsPerSeries.Count - 1].UiBgmId}";
                        return new OrderItemTreeViewModel()
                        {
                            LowerId = lowerNodeId,
                            UpperId = upperNodeId,
                            IsExpanded = _expandedCache.Contains(lowerNodeId) || _expandedCache.Contains(upperNodeId),
                            NbrBgms = $"({bgmsPerSeries.Count} bgms)",
                            BgmEntries = bgmsPerSeries,
                            Label = $"[{seriesTitle}]",
                            Items = chunkBits
                        };
                    });
                    Items.Clear();
                    Items.Add(items);
                });

            ActionInitializeDragAndDrop = ReactiveCommand.Create<TreeView>(InitializeDragAndDropHandlers);
            ActionSelectedTabChanged = ReactiveCommand.Create<TabControl>(SelectedTabChangedHandlers);

            observableBgmEntry.Subscribe(o =>
            {
                if (o != null)
                    SelectedBgmEntry = o;
            });
        }

        #region REORDER
        public void InitializeDragAndDropHandlers(TreeView userControl)
        {
            userControl.AddHandler(InputElement.PointerPressedEvent, MouseDownHandler, RoutingStrategies.Tunnel, true);
            userControl.AddHandler(DragDrop.DropEvent, Drop);
            userControl.AddHandler(DragDrop.DragOverEvent, DragOver);
            _treeView = userControl;
        }

        public void SelectedTabChangedHandlers(TabControl userControl)
        {
            Header = (userControl.SelectedItem as TabItem)?.Header.ToString();
        }

        private async void MouseDownHandler(object sender, PointerPressedEventArgs e)
        {
            var checkForToggle = e.Source;
            while (checkForToggle != null)
            {
                if (checkForToggle is ToggleButton)
                    return;
                checkForToggle = checkForToggle.InteractiveParent;
            }

            var control = e.Source as Control;
            var dataSource = control?.DataContext;
            if (dataSource != null && dataSource is OrderItemTreeViewModel)
            {
                var dragData = new DataObject();
                dragData.Set(Constants.DragAndDropDataFormats.DATAOBJECT_FORMAT_TREEVIEW, dataSource);
                AddTreeDragDrop();
                await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
                RemoveTreeDragDrop();
            }
        }

        public void DragOver(object sender, DragEventArgs e)
        {
            e.DragEffects &= DragDropEffects.Move;
            if (!e.Data.Contains(Constants.DragAndDropDataFormats.DATAOBJECT_FORMAT_TREEVIEW) && !e.Data.Contains(Constants.DragAndDropDataFormats.DATAOBJECT_FORMAT_BGM))
                e.DragEffects = DragDropEffects.None;
        }

        public void Drop(object sender, DragEventArgs e)
        {
            RemoveTreeDragDrop();

            if (((Control)e.Source).DataContext is OrderItemTreeViewModel destinationObj)
            {
                var treeViewItem = VisualTreeHelper.GetControlParent<TreeViewItem>(e.Source);
                var point = e.GetPosition(treeViewItem);
                var position = destinationObj.LowerTestDisp;
                if (point.Y >= treeViewItem.Bounds.Height / 2)
                    position = (short)(destinationObj.UpperTestDisp + 1);

                if (e.Data.Get(Constants.DragAndDropDataFormats.DATAOBJECT_FORMAT_TREEVIEW) is OrderItemTreeViewModel sourceTreeObj && sourceTreeObj != destinationObj)
                {
                    _whenNewRequestToReorderBgmEntries.OnNext(new Tuple<IEnumerable<string>, short>(sourceTreeObj.BgmEntries.Select(p => p.UiBgmId), position));
                }
                else if (e.Data.Get(Constants.DragAndDropDataFormats.DATAOBJECT_FORMAT_BGM) is List<BgmDbRootEntryViewModel> sourceBgmObj && sourceBgmObj.Count > 0)
                {
                    _whenNewRequestToReorderBgmEntries.OnNext(new Tuple<IEnumerable<string>, short>(sourceBgmObj.Select(p => p.UiBgmId), position));
                }
            }
        }

        public void AddTreeDragDrop()
        {
            VisualTreeHelper.AddClassStyle<TreeView>(_treeView, VisualTreeHelper.STYLES_CLASS_IS_DRAGGING);
        }

        public void RemoveTreeDragDrop()
        {
            VisualTreeHelper.RemoveClassStyle<TreeView>(_treeView, VisualTreeHelper.STYLES_CLASS_IS_DRAGGING);
        }
        #endregion

        public void Dispose()
        {
            if (_whenNewRequestToReorderBgmEntries != null)
            {
                _whenNewRequestToReorderBgmEntries?.OnCompleted();
                _whenNewRequestToReorderBgmEntries?.Dispose();
            }
        }
    }
}
