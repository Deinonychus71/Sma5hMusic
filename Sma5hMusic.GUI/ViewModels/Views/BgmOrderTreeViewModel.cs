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
using Sma5hMusic.GUI.ViewModels.ReactiveObjects.OrderTreeViewItems;
using Avalonia.Interactivity;
using Avalonia.Controls.Primitives;
using System.Linq;
using System.Collections.Generic;
using ReactiveUI.Fody.Helpers;

namespace Sma5hMusic.GUI.ViewModels
{
    public class BgmOrderTreeViewModel : ViewModelBase
    {
        private ReadOnlyObservableCollection<BgmDbRootEntryViewModel> _items;
        private const string DATAOBJECT_FORMAT = "BGM";
        private Action _postReorderSelection;

        [Reactive]
        public ObservableCollection<OrderItemViewModel> Items { get; set; }

        public ReactiveCommand<TreeView, Unit> ActionInitializeDragAndDrop { get; }

        public BgmOrderTreeViewModel(IObservable<IChangeSet<BgmDbRootEntryViewModel, string>> observableBgmEntriesList)
        {
            Items = new ObservableCollection<OrderItemViewModel>();

            observableBgmEntriesList
                .DeferUntilLoaded()
                .Filter(p => !p.HiddenInSoundTest)
                .AutoRefresh(p => p.TestDispOrder, TimeSpan.FromMilliseconds(50))
                .Sort(SortExpressionComparer<BgmDbRootEntryViewModel>.Ascending(p => p.TestDispOrder), SortOptimisations.ComparesImmutableValuesOnly, 1)
                .TreatMovesAsRemoveAdd()
                .Throttle(TimeSpan.FromMilliseconds(150))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _items)
                .DisposeMany()
                .Subscribe((o) =>
                {
                    FocusAfterMove();
                    var items = _items.ChunkBy(p => p.SeriesId).Select<IGrouping<string, BgmDbRootEntryViewModel>, OrderItemViewModel>(p =>
                    {
                        var bgmsPerSeries = p.ToList();
                        var seriesTitle = bgmsPerSeries[0].SeriesViewModel.Title;

                        //Case multiple games in the series
                        var chunkBits = bgmsPerSeries.ChunkBy(s => s.UiGameTitleId).Select<IGrouping<string, BgmDbRootEntryViewModel>, OrderItemViewModel>(p =>
                        {
                            var bgmsPerGame = p.ToList();
                            var gameTitle = bgmsPerGame[0].GameTitleViewModel.Title;

                            return new OrderItemTreeItemViewModel()
                            {
                                NbrBgms = $"({bgmsPerGame.Count} bgms)",
                                Label = $"[{gameTitle}]"
                            };
                        }).ToList();

                        return new OrderItemTreeItemViewModel()
                        {
                            NbrBgms = $"({bgmsPerSeries.Count} bgms)",
                            Label = $"[{seriesTitle}]", //[{games[0]}...{games[games.Count - 1]}]
                            Items = chunkBits
                        };
                    });
                    Items.Clear();
                    Items.Add(items);
                });
            /*.ForEachAsync(p => { 
            if(p.SortedItems != null)
                {
                    foreach(var sortedItem in p.SortedItems)
                    {
                        Console.WriteLine("OK");
                    }
                }
            }).Wait()
            .Transform(p => new OrderItemTreeItemViewModel() { Label = p.SeriesViewModel.Title })
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _items)
            .DisposeMany()
            .Subscribe()*/
            ;

            ActionInitializeDragAndDrop = ReactiveCommand.Create<TreeView>(InitializeDragAndDropHandlers);
        }

        #region REORDER
        public void InitializeDragAndDropHandlers(TreeView userControl)
        {
            userControl.AddHandler(InputElement.PointerPressedEvent, MouseDownHandler, RoutingStrategies.Tunnel, true);
            userControl.AddHandler(DragDrop.DropEvent, Drop);
            userControl.AddHandler(DragDrop.DragOverEvent, DragOver);
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
            if (dataSource != null && dataSource is OrderItemViewModel)
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

            //if (((Control)e.Source).DataContext is TestItems destinationObj && destinationObj.HiddenInSoundTest)
            //   e.DragEffects = DragDropEffects.None;
        }

        public void Drop(object sender, DragEventArgs e)
        {
            var source = e.Source;
            while (!(source is TreeView))
            {
                source = source.InteractiveParent;
            }
            var treeView = (TreeView)source;

            if (((Control)e.Source).DataContext is OrderItemViewModel destinationObj
                && e.Data.Get(DATAOBJECT_FORMAT) is OrderItemViewModel sourceObj
                && sourceObj != destinationObj)
            {
                Console.WriteLine("TEST");
                /*var isHigherThanDest = sourceObj.TestDispOrder > destinationObj.TestDispOrder;
                if (isHigherThanDest)
                    sourceObj.TestDispOrder = (short)(destinationObj.TestDispOrder - 1);
                else
                    sourceObj.TestDispOrder = (short)(destinationObj.TestDispOrder + 1);
                _postReorderSelection = () => dataGrid.SelectedItem = sourceObj;*/

                //_whenNewRequestToReorderBgmEntries.OnNext(Unit.Default);
            }
        }

        public void FocusAfterMove()
        {
            if (_postReorderSelection != null)
            {
                _postReorderSelection();
                _postReorderSelection = null;
            }
        }
        #endregion

        public void Dispose()
        {
            //if (_whenNewRequestToReorderBgmEntries != null)
            //{
            //    _whenNewRequestToReorderBgmEntries?.OnCompleted();
            //    _whenNewRequestToReorderBgmEntries?.Dispose();
            //}
        }
    }
}
