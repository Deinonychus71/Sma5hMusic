using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly Subject<BgmDbRootEntryViewModel> _whenNewRequestToRenameToneId;
        private readonly Subject<BgmDbRootEntryViewModel> _whenNewRequestToMoveToOtherMod;
        private readonly Subject<Tuple<string, short>> _whenNewRequestToReorderBgmEntry;
        private const string DATAOBJECT_FORMAT = "BGM";
        private Action _postReorderSelection;

        public ContextMenuViewModel VMContextMenu { get; }

        [Reactive]
        public string Debug { get; set; }

        public IObservable<BgmDbRootEntryViewModel> WhenNewRequestToEditBgmEntry { get { return _whenNewRequestToEditBgmEntry; } }
        public IObservable<BgmDbRootEntryViewModel> WhenNewRequestToDeleteBgmEntry { get { return _whenNewRequestToDeleteBgmEntry; } }
        public IObservable<BgmDbRootEntryViewModel> WhenNewRequestToRenameToneId { get { return _whenNewRequestToRenameToneId; } }
        public IObservable<BgmDbRootEntryViewModel> WhenNewRequestToMoveToOtherMod { get { return _whenNewRequestToMoveToOtherMod; } }
        public IObservable<Tuple<string, short>> WhenNewRequestToReorderBgmEntry { get { return _whenNewRequestToReorderBgmEntry; } }
        public IObservable<Tuple<IEnumerable<string>, short>> WhenNewRequestToReorderBgmEntries { get; private set; }

        public ReadOnlyObservableCollection<BgmDbRootEntryViewModel> Items { get { return _items; } }

        [Reactive]
        public BgmDbRootEntryViewModel SelectedBgmEntry { get; private set; }

        public ReactiveCommand<DataGridCellPointerPressedEventArgs, Unit> ActionReorderBgm { get; }
        public ReactiveCommand<UserControl, Unit> ActionInitializeDragAndDrop { get; }
        public ReactiveCommand<BgmDbRootEntryViewModel, Unit> ActionEditBgm { get; }
        public ReactiveCommand<BgmDbRootEntryViewModel, Unit> ActionRenameToneId { get; }
        public ReactiveCommand<BgmDbRootEntryViewModel, Unit> ActionMoveToOtherMod { get; }
        public ReactiveCommand<BgmDbRootEntryViewModel, Unit> ActionDeleteBgm { get; }
        public BgmPropertiesViewModel VMBgmProperties { get; }

        public BgmSongsViewModel(IServiceProvider serviceProvider, ILogger<BgmSongsViewModel> logger,
            IObservable<IChangeSet<BgmDbRootEntryViewModel, string>> observableBgmEntriesNonFilteredList,
            IObservable<IChangeSet<BgmDbRootEntryViewModel, string>> observableBgmEntriesFilteredList, ContextMenuViewModel vmContextMenu)
        {
            _logger = logger;
            VMContextMenu = vmContextMenu;

            //Initialize list
            _whenNewRequestToEditBgmEntry = new Subject<BgmDbRootEntryViewModel>();
            _whenNewRequestToDeleteBgmEntry = new Subject<BgmDbRootEntryViewModel>();
            _whenNewRequestToReorderBgmEntry = new Subject<Tuple<string, short>>();
            _whenNewRequestToRenameToneId = new Subject<BgmDbRootEntryViewModel>();
            _whenNewRequestToMoveToOtherMod = new Subject<BgmDbRootEntryViewModel>();

            observableBgmEntriesFilteredList
                .Sort(SortExpressionComparer<BgmDbRootEntryViewModel>.Ascending(p => p.HiddenInSoundTest).ThenByAscending(p => p.TestDispOrder), SortOptimisations.ComparesImmutableValuesOnly, 8000)
                .TreatMovesAsRemoveAdd()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _items)
                .DisposeMany()
                .Subscribe((o) => FocusAfterMove());

            ActionReorderBgm = ReactiveCommand.CreateFromTask<DataGridCellPointerPressedEventArgs>(ReorderBgm);
            ActionInitializeDragAndDrop = ReactiveCommand.Create<UserControl>(InitializeDragAndDropHandlers);
            ActionEditBgm = ReactiveCommand.Create<BgmDbRootEntryViewModel>(EditBgmEntry);
            ActionDeleteBgm = ReactiveCommand.Create<BgmDbRootEntryViewModel>(DeleteBgmEntry);
            ActionRenameToneId = ReactiveCommand.Create<BgmDbRootEntryViewModel>(RenameToneId);
            ActionMoveToOtherMod = ReactiveCommand.Create<BgmDbRootEntryViewModel>(MoveToOtherMod);

            //Initialize properties
            var whenSelectedBgmEntryChanged = this.WhenAnyValue(p => p.SelectedBgmEntry);
            VMBgmProperties = ActivatorUtilities.CreateInstance<BgmPropertiesViewModel>(serviceProvider, observableBgmEntriesNonFilteredList, whenSelectedBgmEntryChanged);
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
        public void RenameToneId(BgmDbRootEntryViewModel bgmEntry)
        {
            _whenNewRequestToRenameToneId.OnNext(bgmEntry);
        }
        public void MoveToOtherMod(BgmDbRootEntryViewModel bgmEntry)
        {
            _whenNewRequestToMoveToOtherMod.OnNext(bgmEntry);
        }
        #endregion

        #region REORDER
        public void InitializeDragAndDropHandlers(UserControl userControl)
        {
            userControl.AddHandler(DragDrop.DropEvent, Drop);
            userControl.AddHandler(DragDrop.DragOverEvent, DragOver);
            userControl.AddHandler(DragDrop.DragLeaveEvent, DragLeaveEvent, RoutingStrategies.Bubble);
        }

        public void DragLeaveEvent(object sender, RoutedEventArgs e)
        {
            RemoveDragDropBorder(e.Source);
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
            AddDragDropBorder(e.Source, e);

            e.DragEffects &= DragDropEffects.Move;
            if (!e.Data.Contains(DATAOBJECT_FORMAT))
                e.DragEffects = DragDropEffects.None;

            if (((Control)e.Source).DataContext is BgmDbRootEntryViewModel destinationObj && destinationObj.HiddenInSoundTest)
                e.DragEffects = DragDropEffects.None;
        }

        public void Drop(object sender, DragEventArgs e)
        {
            RemoveDragDropBorder(e.Source);

            var source = e.Source;
            while (!(source is DataGrid) && source != null)
            {
                source = source.InteractiveParent;
            }
            if (source == null)
                return;
            var dataGrid = (DataGrid)source;

            if (((Control)e.Source).DataContext is BgmDbRootEntryViewModel destinationObj
                && e.Data.Get(DATAOBJECT_FORMAT) is BgmDbRootEntryViewModel sourceObj
                && !destinationObj.HiddenInSoundTest
                && sourceObj != destinationObj)
            {
                _whenNewRequestToReorderBgmEntry.OnNext(new Tuple<string, short>(sourceObj.UiBgmId, destinationObj.TestDispOrder));
                _postReorderSelection = () => dataGrid.SelectedItem = sourceObj;
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

        private void RemoveDragDropBorder(IInteractive control)
        {
            var source = control;
            while (!(source is DataGridRow) && source != null)
            {
                source = source.InteractiveParent;
            }
            if (source != null)
            {
                var dgRow = (DataGridRow)source;
                dgRow.Classes.Remove("insertBelow");
                dgRow.Classes.Remove("insertAbove");
            }
        }

        private void AddDragDropBorder(IInteractive control, DragEventArgs e)
        {
            var source = control;
            while (!(source is DataGridRow) && source != null)
            {
                source = source.InteractiveParent;
            }
            if (source != null)
            {
                var dgRow = (DataGridRow)source;
                var point = e.GetPosition(dgRow);
                if (point.Y <= 15)
                    dgRow.Classes.ReplaceOrAdd("insertBelow", "insertAbove");
                else
                    dgRow.Classes.ReplaceOrAdd("insertAbove", "insertBelow");
            }
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
