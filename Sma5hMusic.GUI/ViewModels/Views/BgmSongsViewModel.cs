using Avalonia.Controls;
using Avalonia.Input;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
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
        private readonly Subject<Unit> _whenNewRequestToReorderBgmEntries;
        private const string DATAOBJECT_FORMAT = "BGM";
        private Action _postReorderSelection;

        public ContextMenuViewModel VMContextMenu { get; }

        public IObservable<BgmDbRootEntryViewModel> WhenNewRequestToEditBgmEntry { get { return _whenNewRequestToEditBgmEntry; } }
        public IObservable<BgmDbRootEntryViewModel> WhenNewRequestToDeleteBgmEntry { get { return _whenNewRequestToDeleteBgmEntry; } }
        public IObservable<BgmDbRootEntryViewModel> WhenNewRequestToRenameToneId { get { return _whenNewRequestToRenameToneId; } }
        public IObservable<BgmDbRootEntryViewModel> WhenNewRequestToMoveToOtherMod { get { return _whenNewRequestToMoveToOtherMod; } }
        public IObservable<Unit> WhenNewRequestToReorderBgmEntries { get { return _whenNewRequestToReorderBgmEntries; } }

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
            IObservable<IChangeSet<BgmDbRootEntryViewModel, string>> observableBgmEntriesList, ContextMenuViewModel vmContextMenu)
        {
            _logger = logger;
            VMContextMenu = vmContextMenu;

            //Initialize list
            _whenNewRequestToEditBgmEntry = new Subject<BgmDbRootEntryViewModel>();
            _whenNewRequestToDeleteBgmEntry = new Subject<BgmDbRootEntryViewModel>();
            _whenNewRequestToReorderBgmEntries = new Subject<Unit>();
            _whenNewRequestToRenameToneId = new Subject<BgmDbRootEntryViewModel>();
            _whenNewRequestToMoveToOtherMod = new Subject<BgmDbRootEntryViewModel>();

            observableBgmEntriesList
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
            VMBgmProperties = ActivatorUtilities.CreateInstance<BgmPropertiesViewModel>(serviceProvider, whenSelectedBgmEntryChanged);
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
        public void TestSend(object sender, PointerPressedEventArgs e)
        {
            Console.WriteLine("test");
        }

        public void InitializeDragAndDropHandlers(UserControl userControl)
        {
            userControl.AddHandler(DragDrop.DropEvent, Drop);
            userControl.AddHandler(DragDrop.DragOverEvent, DragOver);
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
            if (_whenNewRequestToEditBgmEntry != null)
            {
                _whenNewRequestToEditBgmEntry?.OnCompleted();
                _whenNewRequestToEditBgmEntry?.Dispose();
            }
            if (_whenNewRequestToReorderBgmEntries != null)
            {
                _whenNewRequestToReorderBgmEntries?.OnCompleted();
                _whenNewRequestToReorderBgmEntries?.Dispose();
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
