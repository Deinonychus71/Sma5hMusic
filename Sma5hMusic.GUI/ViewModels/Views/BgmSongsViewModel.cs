using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sma5h.Mods.Music;
using Sma5hMusic.GUI.Helpers;
using Sma5hMusic.GUI.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sma5hMusic.GUI.ViewModels
{
    public class BgmSongsViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private readonly ReadOnlyObservableCollection<BgmDbRootEntryViewModel> _items;
        private readonly Subject<BgmDbRootEntryViewModel> _whenNewRequestToEditBgmEntry;
        private readonly Subject<BgmDbRootEntryViewModel> _whenNewRequestToDeleteBgmEntry;
        private readonly Subject<List<BgmDbRootEntryViewModel>> _whenNewRequestToDeleteBgmEntries;
        private readonly Subject<BgmDbRootEntryViewModel> _whenNewRequestToRenameToneId;
        private readonly Subject<List<BgmDbRootEntryViewModel>> _whenNewRequestToMoveToOtherMod;
        private readonly Subject<Tuple<IEnumerable<string>, short>> _whenNewRequestToReorderBgmEntry;
        private readonly IOptionsMonitor<ApplicationSettings> _config;
        private readonly IMessageDialog _messageDialog;
        private Action _postReorderSelection;
        private DataGrid _refBgmGrid;

        public ContextMenuViewModel VMContextMenu { get; }

        [Reactive]
        public string Debug { get; set; }

        public IObservable<BgmDbRootEntryViewModel> WhenNewRequestToEditBgmEntry { get { return _whenNewRequestToEditBgmEntry; } }
        public IObservable<BgmDbRootEntryViewModel> WhenNewRequestToDeleteBgmEntry { get { return _whenNewRequestToDeleteBgmEntry; } }
        public IObservable<List<BgmDbRootEntryViewModel>> WhenNewRequestToDeleteBgmEntries { get { return _whenNewRequestToDeleteBgmEntries; } }
        public IObservable<BgmDbRootEntryViewModel> WhenNewRequestToRenameToneId { get { return _whenNewRequestToRenameToneId; } }
        public IObservable<List<BgmDbRootEntryViewModel>> WhenNewRequestToMoveToOtherMod { get { return _whenNewRequestToMoveToOtherMod; } }
        public IObservable<Tuple<IEnumerable<string>, short>> WhenNewRequestToReorderBgmEntry { get { return _whenNewRequestToReorderBgmEntry; } }
        public IObservable<Tuple<IEnumerable<string>, short>> WhenNewRequestToReorderBgmEntries { get; private set; }

        public ReadOnlyObservableCollection<BgmDbRootEntryViewModel> Items { get { return _items; } }

        [Reactive]
        public BgmDbRootEntryViewModel SelectedBgmEntry { get; private set; }

        public ReactiveCommand<DataGridCellPointerPressedEventArgs, Unit> ActionReorderBgm { get; }
        public ReactiveCommand<DataGrid, Unit> ActionInitializeDragAndDrop { get; }
        public ReactiveCommand<BgmDbRootEntryViewModel, Unit> ActionEditBgm { get; }
        public ReactiveCommand<BgmDbRootEntryViewModel, Unit> ActionRenameToneId { get; }
        public ReactiveCommand<BgmDbRootEntryViewModel, Unit> ActionMoveToOtherMod { get; }
        public ReactiveCommand<BgmDbRootEntryViewModel, Unit> ActionDeleteBgm { get; }
        public BgmPropertiesViewModel VMBgmProperties { get; }

        public BgmSongsViewModel(IServiceProvider serviceProvider, IMessageDialog messageDialog, IOptionsMonitor<ApplicationSettings> config, ILogger<BgmSongsViewModel> logger,
            IObservable<IChangeSet<BgmDbRootEntryViewModel, string>> observableBgmEntriesFilteredList, ContextMenuViewModel vmContextMenu)
        {
            _logger = logger;
            _config = config;
            _messageDialog = messageDialog;
            VMContextMenu = vmContextMenu;
            config.OnChange((p) => SetColumnsVisibility(p));

            //Initialize list
            _whenNewRequestToEditBgmEntry = new Subject<BgmDbRootEntryViewModel>();
            _whenNewRequestToDeleteBgmEntry = new Subject<BgmDbRootEntryViewModel>();
            _whenNewRequestToDeleteBgmEntries = new Subject<List<BgmDbRootEntryViewModel>>();
            _whenNewRequestToReorderBgmEntry = new Subject<Tuple<IEnumerable<string>, short>>();
            _whenNewRequestToRenameToneId = new Subject<BgmDbRootEntryViewModel>();
            _whenNewRequestToMoveToOtherMod = new Subject<List<BgmDbRootEntryViewModel>>();

            observableBgmEntriesFilteredList
                .Sort(SortExpressionComparer<BgmDbRootEntryViewModel>.Ascending(p => p.HiddenInSoundTest).ThenByAscending(p => p.TestDispOrder), SortOptimisations.None, 8000)
                .TreatMovesAsRemoveAdd()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _items)
                .DisposeMany()
                .Subscribe((o) => FocusAfterMove());

            ActionReorderBgm = ReactiveCommand.CreateFromTask<DataGridCellPointerPressedEventArgs>(ReorderBgm);
            ActionInitializeDragAndDrop = ReactiveCommand.Create<DataGrid>(InitializeDragAndDropHandlers);
            ActionEditBgm = ReactiveCommand.Create<BgmDbRootEntryViewModel>(EditBgmEntry);
            ActionDeleteBgm = ReactiveCommand.Create<BgmDbRootEntryViewModel>(DeleteBgmEntry);
            ActionRenameToneId = ReactiveCommand.CreateFromTask<BgmDbRootEntryViewModel>(RenameToneId);
            ActionMoveToOtherMod = ReactiveCommand.Create<BgmDbRootEntryViewModel>(MoveToOtherMod);

            //Initialize properties
            var whenSelectedBgmEntryChanged = this.WhenAnyValue(p => p.SelectedBgmEntry);
            VMBgmProperties = ActivatorUtilities.CreateInstance<BgmPropertiesViewModel>(serviceProvider, whenSelectedBgmEntryChanged);
            WhenNewRequestToReorderBgmEntries = VMBgmProperties.WhenNewRequestToReorderBgmEntries;
        }

        #region DELETE/EDIT/RENAME BGM PROPERTIES
        public void EditBgmEntry(BgmDbRootEntryViewModel bgmEntry)
        {
            _whenNewRequestToEditBgmEntry.OnNext(bgmEntry);
        }
        public void DeleteBgmEntry(BgmDbRootEntryViewModel bgmEntry)
        {
            _whenNewRequestToDeleteBgmEntry.OnNext(bgmEntry);
        }
        public void DeleteBgmEntries(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && _refBgmGrid.SelectedItems.Count > 0)
            {
                var items = new List<BgmDbRootEntryViewModel>();
                foreach (BgmDbRootEntryViewModel item in _refBgmGrid.SelectedItems)
                    items.Add(item);
                _whenNewRequestToDeleteBgmEntries.OnNext(items);
            }
        }
        public async Task RenameToneId(BgmDbRootEntryViewModel bgmEntry)
        {
            if (_refBgmGrid.SelectedItems.Count > 1)
            {
                var items = new List<BgmDbRootEntryViewModel>();
                foreach (BgmDbRootEntryViewModel item in _refBgmGrid.SelectedItems)
                    items.Add(item);
                if (items.Contains(bgmEntry))
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await _messageDialog.ShowError("Rename Tone Id", "You rename the Tone Id of one BGM at a time.");
                    }, DispatcherPriority.Background);
                    return;
                }
            }

            _whenNewRequestToRenameToneId.OnNext(bgmEntry);
        }

        public void MoveToOtherMod(BgmDbRootEntryViewModel bgmEntry)
        {
            if (_refBgmGrid.SelectedItems.Count > 1)
            {
                var items = new List<BgmDbRootEntryViewModel>();
                foreach (BgmDbRootEntryViewModel item in _refBgmGrid.SelectedItems)
                        items.Add(item);
                if (items.Contains(bgmEntry))
                {
                    _whenNewRequestToMoveToOtherMod.OnNext(items);
                    return;
                }
            }

            _whenNewRequestToMoveToOtherMod.OnNext(new List<BgmDbRootEntryViewModel>() { bgmEntry });
        }
        #endregion

        #region REORDER
        public void InitializeDragAndDropHandlers(DataGrid grid)
        {
            grid.AddHandler(DragDrop.DropEvent, Drop);
            grid.AddHandler(DragDrop.DragOverEvent, DragOver);
            grid.AddHandler(DataGrid.KeyDownEvent, DeleteBgmEntries);
            _refBgmGrid = grid;
            SetColumnsVisibility(_config.CurrentValue);
        }

        public async Task ReorderBgm(DataGridCellPointerPressedEventArgs e)
        {
            if (e.Column.DisplayIndex != 0)
                return;

            var dataGrid = VisualTreeHelper.GetControlParent<DataGrid>(e.Row);
            if (e.Cell.DataContext is BgmDbRootEntryViewModel sourceObj)
            {
                var dragData = new DataObject();
                var syncCheck = dataGrid.SelectedItems.Contains(sourceObj);

                var leftClick = VisualTreeHelper.IsLeftButtonClicked(dataGrid, e.PointerPressedEventArgs);
                if (dataGrid.SelectedItems.Count == 1 || !syncCheck || leftClick)
                {
                    dragData.Set(Constants.DragAndDropDataFormats.DATAOBJECT_FORMAT_BGM, new List<BgmDbRootEntryViewModel>() { sourceObj });
                    VisualTreeHelper.AddClassStyle<DataGrid>(dataGrid, VisualTreeHelper.STYLES_CLASS_IS_DRAGGING);
                    VMBgmProperties.AddTreeDragDrop();
                    await DragDrop.DoDragDrop(e.PointerPressedEventArgs, dragData, DragDropEffects.Move);
                    VisualTreeHelper.RemoveClassStyle<DataGrid>(dataGrid, VisualTreeHelper.STYLES_CLASS_IS_DRAGGING);
                    VMBgmProperties.RemoveTreeDragDrop();
                }
                else if (dataGrid.SelectedItems.Count > 1)
                {
                    var items = new List<BgmDbRootEntryViewModel>();
                    foreach (BgmDbRootEntryViewModel item in dataGrid.SelectedItems)
                    {
                        if (!item.HiddenInSoundTest)
                            items.Add(item);
                    }
                    if (items.Count > 0)
                    {
                        dragData.Set(Constants.DragAndDropDataFormats.DATAOBJECT_FORMAT_BGM, items);
                        VisualTreeHelper.AddClassStyle<DataGrid>(dataGrid, VisualTreeHelper.STYLES_CLASS_IS_DRAGGING);
                        VMBgmProperties.AddTreeDragDrop();
                        await DragDrop.DoDragDrop(e.PointerPressedEventArgs, dragData, DragDropEffects.Move);
                        VisualTreeHelper.RemoveClassStyle<DataGrid>(dataGrid, VisualTreeHelper.STYLES_CLASS_IS_DRAGGING);
                        VMBgmProperties.RemoveTreeDragDrop();
                    }
                }
            }
        }

        public void DragOver(object sender, DragEventArgs e)
        {
            e.DragEffects &= DragDropEffects.Move;
            if (!e.Data.Contains(Constants.DragAndDropDataFormats.DATAOBJECT_FORMAT_BGM))
                e.DragEffects = DragDropEffects.None;

            if (((Control)e.Source).DataContext is BgmDbRootEntryViewModel destinationObj && destinationObj.HiddenInSoundTest)
                e.DragEffects = DragDropEffects.None;
        }

        public void Drop(object sender, DragEventArgs e)
        {
            var dataGrid = VisualTreeHelper.GetControlParent<DataGrid>(e.Source);
            VisualTreeHelper.RemoveClassStyle<DataGrid>(dataGrid, VisualTreeHelper.STYLES_CLASS_IS_DRAGGING);
            var dataGridRow = VisualTreeHelper.GetControlParent<DataGridRow>(e.Source);
            if (dataGrid == null || dataGridRow == null)
                return;

            if (((Control)e.Source).DataContext is BgmDbRootEntryViewModel destinationObj)
            {
                var selectedItems = e.Data.Get(Constants.DragAndDropDataFormats.DATAOBJECT_FORMAT_BGM) as List<BgmDbRootEntryViewModel>;
                if (selectedItems != null && selectedItems.Count > 0 && !destinationObj.HiddenInSoundTest)
                {
                    if (selectedItems[0] != destinationObj)
                    {
                        var position = destinationObj.TestDispOrder;
                        var point = e.GetPosition(dataGridRow);
                        if (point.Y >= dataGridRow.Bounds.Height / 2)
                            position += 1;

                        _whenNewRequestToReorderBgmEntry.OnNext(new Tuple<IEnumerable<string>, short>(selectedItems.Select(p => p.UiBgmId), position));
                        _postReorderSelection = () =>
                        {
                            dataGrid.SelectedItems.Clear();
                            selectedItems.ForEach(o => { try { dataGrid.SelectedItems.Add(o); } catch { } });
                        };
                    }
                }
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

        private void SetColumnsVisibility(ApplicationSettings settings)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                _refBgmGrid.Columns[2].IsVisible = !settings.Sma5hMusicGUI.HideIndexColumn;
                _refBgmGrid.Columns[3].IsVisible = !settings.Sma5hMusicGUI.HideSeriesColumn;
                _refBgmGrid.Columns[6].IsVisible = !settings.Sma5hMusicGUI.HideRecordColumn;
                _refBgmGrid.Columns[7].IsVisible = !settings.Sma5hMusicGUI.HideModColumn;
            }, DispatcherPriority.Background);
        }
        #endregion

        public void Dispose()
        {
            if (_whenNewRequestToEditBgmEntry != null)
            {
                _whenNewRequestToEditBgmEntry?.OnCompleted();
                _whenNewRequestToEditBgmEntry?.Dispose();
            }
            if (_whenNewRequestToReorderBgmEntry != null)
            {
                _whenNewRequestToReorderBgmEntry?.OnCompleted();
                _whenNewRequestToReorderBgmEntry?.Dispose();
            }
            if (_whenNewRequestToDeleteBgmEntry != null)
            {
                _whenNewRequestToDeleteBgmEntry?.OnCompleted();
                _whenNewRequestToDeleteBgmEntry?.Dispose();
            }
            if (_whenNewRequestToRenameToneId != null)
            {
                _whenNewRequestToRenameToneId?.OnCompleted();
                _whenNewRequestToRenameToneId?.Dispose();
            }
            if (_whenNewRequestToMoveToOtherMod != null)
            {
                _whenNewRequestToMoveToOtherMod?.OnCompleted();
                _whenNewRequestToMoveToOtherMod?.Dispose();
            }
        }
    }
}
