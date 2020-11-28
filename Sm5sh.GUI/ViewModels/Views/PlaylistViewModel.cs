using DynamicData;
using Microsoft.Extensions.Logging;
using ReactiveUI.Fody.Helpers;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI;
using Avalonia.Controls;
using System.Reactive;
using System.Reactive.Subjects;
using DynamicData.Binding;
using System.Reactive.Linq;
using Avalonia.Input;
using System.Threading.Tasks;
using Sm5sh.GUI.Models;

namespace Sm5sh.GUI.ViewModels
{
    public class PlaylistViewModel : ViewModelBase
    {
        private readonly ILogger _logging;
        private readonly ReadOnlyObservableCollection<BgmEntryViewModel> _bgms;
        private readonly ReadOnlyObservableCollection<PlaylistEntryViewModel> _playlists;
        private readonly BehaviorSubject<PlaylistEntryViewModel> _whenPlaylistSelected;
        private readonly BehaviorSubject<ComboItem> _whenPlaylistOrderSelected;
        private readonly Subject<Unit> _whenNewRequestToReorderBgmEntries;
        private const string DATAOBJECT_FORMAT_BGM = "BGM";
        private const string DATAOBJECT_FORMAT_PLAYLIST = "PLAYLIST";
        private readonly List<ComboItem> _orderMenu;
        private Action _postReorderSelection;
        private Grid _refGrid;

        public ContextMenuViewModel VMContextMenu { get; }

        public IObservable<Unit> WhenNewRequestToReorderBgmEntries { get { return _whenNewRequestToReorderBgmEntries; } }

        public ReadOnlyObservableCollection<BgmEntryViewModel> Bgms { get { return _bgms; } }
        public ReadOnlyObservableCollection<PlaylistEntryViewModel> Playlists { get { return _playlists; } }

        [Reactive]
        public BgmEntryViewModel SelectedBgmEntry { get; private set; }

        [Reactive]
        public PlaylistEntryViewModel SelectedPlaylistEntry { get; private set; }
        [Reactive]
        public PlaylistEntryValueViewModel SelectedPlaylistValueEntry { get; private set; }
        [Reactive]
        public short SelectedPlaylistOrder { get; private set; }
        [Reactive]
        public ObservableCollection<PlaylistEntryValueViewModel> SelectedPlaylistOrderedEntry { get; private set; }


        public ReactiveCommand<DataGridCellPointerPressedEventArgs, Unit> ActionReorderPlaylist { get; }
        public ReactiveCommand<DataGridCellPointerPressedEventArgs, Unit> ActionSendToPlaylist { get; }
        public ReactiveCommand<Grid, Unit> ActionInitializeDragAndDrop { get; }
        public ReactiveCommand<PlaylistEntryViewModel, Unit> ActionSelectPlaylist { get; }
        public ReactiveCommand<ComboItem, Unit> ActionSelectPlaylistOrder { get; }

        public IEnumerable<ComboItem> OrderMenu { get { return _orderMenu; } }
        public IObservable<PlaylistEntryViewModel> WhenPlaylistSelected { get { return _whenPlaylistSelected; } }
        public IObservable<ComboItem> WhenPlaylistOrderSelected { get { return _whenPlaylistOrderSelected; } }


        public PlaylistViewModel(ILogger<PlaylistViewModel> logging, IObservable<IChangeSet<BgmEntryViewModel, string>> observableBgmEntries,
            IObservable<IChangeSet<PlaylistEntryViewModel, string>> observablePlaylistEntries, ContextMenuViewModel vmContextMenu)
        {
            _logging = logging;
            VMContextMenu = vmContextMenu;
            _orderMenu = GetOrderList();
            _whenNewRequestToReorderBgmEntries = new Subject<Unit>();

            //Bgms
            observableBgmEntries
                .AutoRefresh(p => p.DbRoot.TestDispOrder, TimeSpan.FromMilliseconds(1))
                .Sort(SortExpressionComparer<BgmEntryViewModel>.Ascending(p => p.HiddenInSoundTest).ThenByAscending(p => p.DbRoot.TestDispOrder), SortOptimisations.ComparesImmutableValuesOnly, 8000)
                .TreatMovesAsRemoveAdd()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _bgms)
                .DisposeMany()
                .Subscribe((o) => FocusAfterMove());

            //Playlists
            observablePlaylistEntries
                .Sort(SortExpressionComparer<PlaylistEntryViewModel>.Ascending(p => p.Title), SortOptimisations.ComparesImmutableValuesOnly, 8000)
                .TreatMovesAsRemoveAdd()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _playlists)
                .DisposeMany()
                .Subscribe();

            //Trigger behavior subjets
            _whenPlaylistSelected = new BehaviorSubject<PlaylistEntryViewModel>(_playlists.FirstOrDefault());
            _whenPlaylistOrderSelected = new BehaviorSubject<ComboItem>(_orderMenu.FirstOrDefault());

            ActionReorderPlaylist = ReactiveCommand.CreateFromTask<DataGridCellPointerPressedEventArgs>(ReorderPlaylist);
            ActionSendToPlaylist = ReactiveCommand.CreateFromTask<DataGridCellPointerPressedEventArgs>(SendToPlaylist);
            ActionInitializeDragAndDrop = ReactiveCommand.Create<Grid>(InitializeDragAndDropHandlers);
            ActionSelectPlaylist = ReactiveCommand.Create<PlaylistEntryViewModel>(SelectPlaylistId);
            ActionSelectPlaylistOrder = ReactiveCommand.Create<ComboItem>(SelectPlaylistOrder);
        }

        #region Drag & Drop
        public void InitializeDragAndDropHandlers(Grid grid)
        {
            _refGrid = grid;
            grid.AddHandler(DragDrop.DropEvent, Drop);
            grid.AddHandler(DragDrop.DragOverEvent, DragOver);
        }

        public async Task ReorderPlaylist(DataGridCellPointerPressedEventArgs e)
        {
            var dragData = new DataObject();

            if (e.Cell.DataContext is PlaylistEntryValueViewModel vmPlaylistValue)
            {
                dragData.Set(DATAOBJECT_FORMAT_PLAYLIST, vmPlaylistValue);
                await DragDrop.DoDragDrop(e.PointerPressedEventArgs, dragData, DragDropEffects.Move);
            }
        }

        public async Task SendToPlaylist(DataGridCellPointerPressedEventArgs e)
        {
            var dragData = new DataObject();

            if (e.Cell.DataContext is BgmEntryViewModel vmBgmEntry)
            {
                dragData.Set(DATAOBJECT_FORMAT_BGM, vmBgmEntry);
                await DragDrop.DoDragDrop(e.PointerPressedEventArgs, dragData, DragDropEffects.Move);
            }
        }

        public void DragOver(object sender, DragEventArgs e)
        {
            e.DragEffects &= DragDropEffects.Move;
            if (!e.Data.Contains(DATAOBJECT_FORMAT_BGM) && !e.Data.Contains(DATAOBJECT_FORMAT_PLAYLIST))
                e.DragEffects = DragDropEffects.None;
        }

        public void Drop(object sender, DragEventArgs e)
        {
            if (((Control)e.Source).DataContext is PlaylistEntryValueViewModel destinationObj)
            {
                if(e.Data.Get(DATAOBJECT_FORMAT_BGM) is BgmEntryViewModel sourceBgmObj)
                {
                    DropSendToPlaylist(sourceBgmObj, destinationObj);
                }
                else if (e.Data.Get(DATAOBJECT_FORMAT_PLAYLIST) is PlaylistEntryValueViewModel sourcePlaylistObj)
                {
                    DropReorder(sourcePlaylistObj, destinationObj);
                }
            }

            /*if (((Control)e.Source).DataContext is BgmEntryViewModel destinationObj
                && e.Data.Get(DATAOBJECT_FORMAT) is BgmEntryViewModel sourceObj
                && !destinationObj.HiddenInSoundTest
                && sourceObj != destinationObj)
            {
                var isHigherThanDest = sourceObj.DbRoot.TestDispOrder > destinationObj.DbRoot.TestDispOrder;
                sourceObj.DbRoot.TestDispOrder = destinationObj.DbRoot.TestDispOrder;
                if (isHigherThanDest)
                    destinationObj.DbRoot.TestDispOrder += 1;
                else
                    destinationObj.DbRoot.TestDispOrder -= 1;
                _postReorderSelection = () => dataGrid.SelectedItem = sourceObj;

                _whenNewRequestToReorderBgmEntries.OnNext(Unit.Default);
            }*/
        }

        public void DropSendToPlaylist(BgmEntryViewModel sourceObj, PlaylistEntryValueViewModel destinationObj)
        {
            var newEntry = new PlaylistEntryValueViewModel(sourceObj.DbRoot.UiBgmId, 999, 5000, sourceObj);
            SelectedPlaylistEntry.Tracks[SelectedPlaylistOrder].Add(newEntry);
        }

        public void DropReorder(PlaylistEntryValueViewModel sourceObj, PlaylistEntryValueViewModel destinationObj)
        {

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

        #region Playlists
        private void SelectPlaylistId(PlaylistEntryViewModel vmPlaylist)
        {
            _whenPlaylistSelected.OnNext(vmPlaylist);
            SelectedPlaylistEntry = vmPlaylist;
            SelectPlaylist();
        }

        private void SelectPlaylistOrder(ComboItem orderItem)
        {
            _whenPlaylistOrderSelected.OnNext(orderItem);
            SelectedPlaylistOrder = short.Parse(orderItem.Id);
            SelectPlaylist();
        }

        private void SelectPlaylist()
        {
            if (SelectedPlaylistEntry == null)
                return;

            if (SelectedPlaylistEntry.Tracks.ContainsKey(SelectedPlaylistOrder))
                SelectedPlaylistOrderedEntry = SelectedPlaylistEntry.Tracks[SelectedPlaylistOrder];
            else
                SelectedPlaylistOrderedEntry = null;
        }

        private List<ComboItem> GetOrderList()
        {
            var output = new List<ComboItem>();
            for (int i = 0; i < 16; i++)
                output.Add(new ComboItem(i.ToString(), $"Order {i}"));
            return output;
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
