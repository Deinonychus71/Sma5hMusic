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
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Sma5hMusic.GUI.ViewModels
{
    public class TestItems
    {
        public string Label { get; set; }
    }

    public class TestItemsTree : TestItems
    {
        public List<TestItems> Items { get; set; }
        public string NbrBgms { get; set; }
    }

    public class TestItemsValue : TestItems
    {
        public string Path { get; set; }
        public BgmDbRootEntryViewModel BgmEntry { get; set; }
    }


    public class BgmSoundTestViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private readonly ReadOnlyObservableCollection<BgmDbRootEntryViewModel> _items;
        private readonly Subject<Unit> _whenNewRequestToReorderBgmEntries;
        private const string DATAOBJECT_FORMAT = "BGM";
        private Action _postReorderSelection;

        public ContextMenuViewModel VMContextMenu { get; }

        public IObservable<Unit> WhenNewRequestToReorderBgmEntries { get { return _whenNewRequestToReorderBgmEntries; } }

        public ReadOnlyObservableCollection<BgmDbRootEntryViewModel> Items { get { return _items; } }

        public List<TestItems> Items2 { get; set; }

        [Reactive]
        public BgmDbRootEntryViewModel SelectedBgmEntry { get; private set; }

        public ReactiveCommand<DataGridCellPointerPressedEventArgs, Unit> ActionReorderBgm { get; }
        public ReactiveCommand<TreeView, Unit> ActionInitializeDragAndDrop { get; }

        public BgmSoundTestViewModel(IServiceProvider serviceProvider, ILogger<BgmSongsViewModel> logger,
            IObservable<IChangeSet<BgmDbRootEntryViewModel, string>> observableBgmEntriesList, ContextMenuViewModel vmContextMenu)
        {
            _logger = logger;
            VMContextMenu = vmContextMenu;

            //Initialize list
            _whenNewRequestToReorderBgmEntries = new Subject<Unit>();

            observableBgmEntriesList
                .Sort(SortExpressionComparer<BgmDbRootEntryViewModel>.Ascending(p => p.HiddenInSoundTest).ThenByAscending(p => p.TestDispOrder), SortOptimisations.ComparesImmutableValuesOnly, 8000)
                .TreatMovesAsRemoveAdd()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _items)
                .DisposeMany()
                .Subscribe((o) =>
                {
                    FocusAfterMove();
                    Items2 = Items.ChunkBy(p => p.SeriesId).Select<IGrouping<string, BgmDbRootEntryViewModel>, TestItems>(p =>
                    {
                        var bgmsPerSeries = p.ToList();
                        var seriesTitle = bgmsPerSeries[0].SeriesViewModel.Title;

                        //Case 1 game in a whole series
                        if (bgmsPerSeries.Count == 1)
                        {
                            var gameTitle = bgmsPerSeries[0].GameTitleViewModel.Title;
                            return new TestItemsValue()
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
                            return new TestItemsTree()
                            {
                                NbrBgms = $"({bgmsPerSeries.Count} bgms)",
                                Label = $"[{seriesTitle}] [{games[0]}]",
                                Items = new List<TestItems>(){ bgmsPerSeries.Select(p => new TestItemsValue()
                                {
                                    Label = p.Title,
                                    BgmEntry = p
                                }).ToList() }
                            };
                        }
                        else
                        {
                            //Case multiple games in the series
                            var chunkBits = bgmsPerSeries.ChunkBy(s => s.UiGameTitleId).Select<IGrouping<string, BgmDbRootEntryViewModel>, TestItems>(p =>
                            {
                                var bgmsPerGame = p.ToList();
                                var gameTitle = bgmsPerGame[0].GameTitleViewModel.Title;

                                //var gameItemsWithMoreThanOneTrack = gameItems.
                                if (bgmsPerGame.Count == 1)
                                {
                                    return new TestItemsValue()
                                    {
                                        Path = $"[{gameTitle}]",
                                        Label = bgmsPerGame[0].Title,
                                        BgmEntry = bgmsPerGame[0]
                                    };
                                }
                                return new TestItemsTree()
                                {
                                    NbrBgms = $"({bgmsPerGame.Count} bgms)",
                                    Label = $"[{gameTitle}]", //[{bgmsPerGame[0].Title}...{bgmsPerGame[bgmsPerGame.Count - 1].Title}]
                                    Items = new List<TestItems>(){ bgmsPerGame.Select(p => new TestItemsValue()
                                    {
                                        Label = p.Title,
                                        BgmEntry = p
                                    }).ToList() }
                                };
                            }).ToList();

                            return new TestItemsTree()
                            {
                                NbrBgms = $"({bgmsPerSeries.Count} bgms)",
                                Label = $"[{seriesTitle}]", //[{games[0]}...{games[games.Count - 1]}]
                                Items = chunkBits
                            };
                        }
                    }).ToList();
                });

            ActionReorderBgm = ReactiveCommand.CreateFromTask<DataGridCellPointerPressedEventArgs>(ReorderBgm);
            ActionInitializeDragAndDrop = ReactiveCommand.Create<TreeView> (InitializeDragAndDropHandlers);
        }

        #region REORDER
        public void InitializeDragAndDropHandlers(TreeView userControl)
        {
            userControl.AddHandler(InputElement.PointerPressedEvent, MouseDownHandler, handledEventsToo: true);
            userControl.AddHandler(InputElement.PointerReleasedEvent, MouseUpHandler, handledEventsToo: true);
            userControl.AddHandler(DragDrop.DropEvent, Drop);
            userControl.AddHandler(DragDrop.DragOverEvent, DragOver);
        }

        private void MouseUpHandler(object sender, PointerReleasedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Mouse released.");
        }

        private async void MouseDownHandler(object sender, PointerPressedEventArgs e)
        {
            var control = e.Source as Control;
            var dataSource = control?.DataContext;
            if(dataSource != null && dataSource is TestItems)
            {
                Console.WriteLine("test");

                var dragData = new DataObject();
                dragData.Set("TEST", dataSource);
                await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
            }
        }

        public async Task ReorderBgm(DataGridCellPointerPressedEventArgs e)
        {
            if (e.Column.DisplayIndex != 0)
                return;

            var dragData = new DataObject();

            if (e.Cell.DataContext is BgmDbRootEntryViewModel vmBgmEntry)
            {
                dragData.Set(DATAOBJECT_FORMAT, vmBgmEntry);

                if (!vmBgmEntry.HiddenInSoundTest)
                    await DragDrop.DoDragDrop(e.PointerPressedEventArgs, dragData, DragDropEffects.Move);
            }
        }

        public void DragOver(object sender, DragEventArgs e)
        {
            e.DragEffects &= DragDropEffects.Move;
            if (!e.Data.Contains(DATAOBJECT_FORMAT))
                e.DragEffects = DragDropEffects.None;

            if (((Control)e.Source).DataContext is BgmDbRootEntryViewModel destinationObj && destinationObj.HiddenInSoundTest)
                e.DragEffects = DragDropEffects.None;
        }

        public void Drop(object sender, DragEventArgs e)
        {
            var source = e.Source;
            while (!(source is DataGrid))
            {
                source = source.InteractiveParent;
            }
            var dataGrid = (DataGrid)source;

            if (((Control)e.Source).DataContext is BgmDbRootEntryViewModel destinationObj
                && e.Data.Get(DATAOBJECT_FORMAT) is BgmDbRootEntryViewModel sourceObj
                && !destinationObj.HiddenInSoundTest
                && sourceObj != destinationObj)
            {
                var isHigherThanDest = sourceObj.TestDispOrder > destinationObj.TestDispOrder;
                if (isHigherThanDest)
                    sourceObj.TestDispOrder = (short)(destinationObj.TestDispOrder - 1);
                else
                    sourceObj.TestDispOrder = (short)(destinationObj.TestDispOrder + 1);
                _postReorderSelection = () => dataGrid.SelectedItem = sourceObj;

                _whenNewRequestToReorderBgmEntries.OnNext(Unit.Default);
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
            if (_whenNewRequestToReorderBgmEntries != null)
            {
                _whenNewRequestToReorderBgmEntries?.OnCompleted();
                _whenNewRequestToReorderBgmEntries?.Dispose();
            }
        }
    }
}
