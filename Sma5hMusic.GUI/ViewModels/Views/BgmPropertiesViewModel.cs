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

namespace Sma5hMusic.GUI.ViewModels
{
    public class BgmPropertiesViewModel : ViewModelBase
    {
        private ReadOnlyObservableCollection<BgmDbRootEntryViewModel> _items;
        private readonly Subject<Tuple<IEnumerable<string>, short>> _whenNewRequestToReorderBgmEntries;
        private readonly HashSet<string> _expandedCache;
        private const string DATAOBJECT_FORMAT = "BGM";

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
                dragData.Set(DATAOBJECT_FORMAT, dataSource);
                await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
            }
        }

        public void DragOver(object sender, DragEventArgs e)
        {
            e.DragEffects &= DragDropEffects.Move;
            if (!e.Data.Contains(DATAOBJECT_FORMAT))
                e.DragEffects = DragDropEffects.None;
        }

        public void Drop(object sender, DragEventArgs e)
        {
            if (((Control)e.Source).DataContext is OrderItemTreeViewModel destinationObj)
            {
                var dataObject = e.Data.Get(DATAOBJECT_FORMAT);
                if (dataObject is OrderItemTreeViewModel sourceTreeObj && sourceTreeObj != destinationObj)
                {
                    short position;
                    if (sourceTreeObj.LowerTestDisp < destinationObj.LowerTestDisp)
                        position = destinationObj.UpperTestDisp;
                    else
                        position = destinationObj.LowerTestDisp;

                    _whenNewRequestToReorderBgmEntries.OnNext(new Tuple<IEnumerable<string>, short>(sourceTreeObj.BgmEntries.Select(p => p.UiBgmId), position));
                }
                else if (dataObject is BgmDbRootEntryViewModel sourceBgmObj)
                {
                    short position;
                    if (sourceBgmObj.TestDispOrder < destinationObj.LowerTestDisp)
                        position = destinationObj.UpperTestDisp;
                    else
                        position = (short)(destinationObj.UpperTestDisp + 1);

                    _whenNewRequestToReorderBgmEntries.OnNext(new Tuple<IEnumerable<string>, short>(new List<string>() { sourceBgmObj.UiBgmId }, position));
                }
            }
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
