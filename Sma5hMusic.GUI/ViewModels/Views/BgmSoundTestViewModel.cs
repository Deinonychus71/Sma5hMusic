using Avalonia.Controls;
using Avalonia.Input;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.ComponentModel;
using Sma5hMusic.GUI.ViewModels.ReactiveObjects.OrderTreeViewItems;
using Avalonia.Interactivity;
using Avalonia.Controls.Primitives;

namespace Sma5hMusic.GUI.ViewModels
{
    public class BgmSoundTestViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private readonly ReadOnlyObservableCollection<BgmDbRootEntryViewModel> _items;
        //private readonly Subject<Unit> _whenNewRequestToReorderBgmEntries;
        private const string DATAOBJECT_FORMAT = "BGM";
        private Action _postReorderSelection;

        public ContextMenuViewModel VMContextMenu { get; }

        //public IObservable<Unit> WhenNewRequestToReorderBgmEntries { get { return _whenNewRequestToReorderBgmEntries; } }

        [Reactive]
        public List<OrderItemViewModel> Items { get; set; }

        public ReactiveCommand<TreeView, Unit> ActionInitializeDragAndDrop { get; }

        public BgmSoundTestViewModel(IServiceProvider serviceProvider, ILogger<BgmSongsViewModel> logger,
            IObservable<IChangeSet<BgmDbRootEntryViewModel, string>> observableBgmEntriesList, ContextMenuViewModel vmContextMenu)
        {
            _logger = logger;
            VMContextMenu = vmContextMenu;

            //Initialize list
            //_whenNewRequestToReorderBgmEntries = new Subject<Unit>();

            observableBgmEntriesList
                .Filter(p => !p.HiddenInSoundTest)
                .Sort(SortExpressionComparer<BgmDbRootEntryViewModel>.Ascending(p => p.TestDispOrder), SortOptimisations.ComparesImmutableValuesOnly, 8000)
                .TreatMovesAsRemoveAdd()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _items)
                .DisposeMany()
                .Subscribe((o) =>
                {
                    FocusAfterMove();
                    Items = _items.ChunkBy(p => p.SeriesId).Select<IGrouping<string, BgmDbRootEntryViewModel>, OrderItemViewModel>(p =>
                    {
                        var bgmsPerSeries = p.ToList();
                        var seriesTitle = bgmsPerSeries[0].SeriesViewModel.Title;

                        //Case 1 game in a whole series
                        if (bgmsPerSeries.Count == 1)
                        {
                            var gameTitle = bgmsPerSeries[0].GameTitleViewModel.Title;
                            return new OrderItemValueViewModel()
                            {
                                Path = $"[{seriesTitle}] [{gameTitle}]",
                                Label = bgmsPerSeries[0].Title,
                                BgmEntry = bgmsPerSeries[0]
                            };
                        }

                        var games = bgmsPerSeries.GroupBy(p => p.GameTitleViewModel.Title).Select(p => p.Key).ToList();

                        //Case 1 game in the series
                        if (games.Count == 1)
                        {
                            var gameTitle = games[0];
                            return new OrderItemTreeItemViewModel()
                            {
                                NbrBgms = $"({bgmsPerSeries.Count} bgms)",
                                Label = $"[{seriesTitle}] [{games[0]}]",
                                Items = new List<OrderItemViewModel>(){ bgmsPerSeries.Select(p => new OrderItemValueViewModel()
                                {
                                    Label = p.Title,
                                    BgmEntry = p
                                }).ToList() }
                            };
                        }
                        else
                        {
                            //Case multiple games in the series
                            var chunkBits = bgmsPerSeries.ChunkBy(s => s.UiGameTitleId).Select<IGrouping<string, BgmDbRootEntryViewModel>, OrderItemViewModel>(p =>
                            {
                                var bgmsPerGame = p.ToList();
                                var gameTitle = bgmsPerGame[0].GameTitleViewModel.Title;

                                //var gameItemsWithMoreThanOneTrack = gameItems.
                                if (bgmsPerGame.Count == 1)
                                {
                                    return new OrderItemValueViewModel()
                                    {
                                        Path = $"[{gameTitle}]",
                                        Label = bgmsPerGame[0].Title,
                                        BgmEntry = bgmsPerGame[0]
                                    };
                                }
                                return new OrderItemTreeItemViewModel()
                                {
                                    NbrBgms = $"({bgmsPerGame.Count} bgms)",
                                    Label = $"[{gameTitle}]", //[{bgmsPerGame[0].Title}...{bgmsPerGame[bgmsPerGame.Count - 1].Title}]
                                    Items = new List<OrderItemViewModel>(){ bgmsPerGame.Select(p => new OrderItemValueViewModel()
                                    {
                                        Label = p.Title,
                                        BgmEntry = p
                                    }).ToList() }
                                };
                            }).ToList();

                            return new OrderItemTreeItemViewModel()
                            {
                                NbrBgms = $"({bgmsPerSeries.Count} bgms)",
                                Label = $"[{seriesTitle}]", //[{games[0]}...{games[games.Count - 1]}]
                                Items = chunkBits
                            };
                        }
                    }).ToList();
                });

            ActionInitializeDragAndDrop = ReactiveCommand.Create<TreeView> (InitializeDragAndDropHandlers);
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
            if(dataSource != null && dataSource is OrderItemViewModel)
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
