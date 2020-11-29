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
using System.Reactive.Linq;
using Avalonia.Input;
using System.Threading.Tasks;
using Sm5sh.GUI.Models;
using DynamicData.Binding;
using DynamicData;
using Microsoft.Extensions.DependencyInjection;

namespace Sm5sh.GUI.ViewModels
{
    public class PlaylistViewModel : ViewModelBase
    {
        private readonly ILogger _logging;
        private readonly ReadOnlyObservableCollection<BgmEntryViewModel> _bgms;
        private readonly ReadOnlyObservableCollection<PlaylistEntryViewModel> _playlists;
        private readonly ReadOnlyObservableCollection<PlaylistEntryValueViewModel> _selectedPlaylistOrderedEntry;
        private readonly BehaviorSubject<PlaylistEntryViewModel> _whenPlaylistSelected;
        private readonly BehaviorSubject<ComboItem> _whenPlaylistOrderSelected;
        private readonly Subject<Unit> _whenNewRequestToUpdatePlaylistsInternal;
        private readonly Subject<Unit> _whenNewRequestToUpdatePlaylists;
        private const string DATAOBJECT_FORMAT_BGM = "BGM";
        private const string DATAOBJECT_FORMAT_PLAYLIST = "PLAYLIST";
        private readonly List<ComboItem> _orderMenu;
        private Action _postReorderSelection;
        private DataGrid _refGrid;

        public IEnumerable<ComboItem> OrderMenu { get { return _orderMenu; } }
        public ContextMenuViewModel VMContextMenu { get; }

        public IObservable<PlaylistEntryViewModel> WhenPlaylistSelected { get { return _whenPlaylistSelected; } }
        public IObservable<ComboItem> WhenPlaylistOrderSelected { get { return _whenPlaylistOrderSelected; } }
        private IObservable<Unit> WhenNewRequestToUpdatePlaylistsInternal { get { return _whenNewRequestToUpdatePlaylistsInternal; } }
        public IObservable<Unit> WhenNewRequestToUpdatePlaylists { get { return _whenNewRequestToUpdatePlaylists; } }
        public ReadOnlyObservableCollection<BgmEntryViewModel> Bgms { get { return _bgms; } }
        public ReadOnlyObservableCollection<PlaylistEntryViewModel> Playlists { get { return _playlists; } }
        public ReadOnlyObservableCollection<PlaylistEntryValueViewModel> SelectedPlaylistOrderedEntry { get { return _selectedPlaylistOrderedEntry; } }

        [Reactive]
        public PlaylistEntryViewModel SelectedPlaylistEntry { get; private set; }
        [Reactive]
        public PlaylistEntryValueViewModel SelectedPlaylistValueEntry { get; set; }
        [Reactive]
        public short SelectedPlaylistOrder { get; private set; }


        public ReactiveCommand<DataGridCellPointerPressedEventArgs, Unit> ActionReorderPlaylist { get; }
        public ReactiveCommand<DataGridCellPointerPressedEventArgs, Unit> ActionSendToPlaylist { get; }
        public ReactiveCommand<DataGrid, Unit> ActionInitializeDragAndDrop { get; }
        public ReactiveCommand<PlaylistEntryViewModel, Unit> ActionSelectPlaylist { get; }
        public ReactiveCommand<PlaylistEntryValueViewModel, Unit> ActionDeletePlaylistItem { get; }
        public ReactiveCommand<ComboItem, Unit> ActionSelectPlaylistOrder { get; }


        public PlaylistViewModel(ILogger<PlaylistViewModel> logging, IObservable<IChangeSet<BgmEntryViewModel, string>> observableBgmEntries,
            IObservable<IChangeSet<PlaylistEntryViewModel, string>> observablePlaylistEntries, ContextMenuViewModel vmContextMenu)
        {
            _logging = logging;
            VMContextMenu = vmContextMenu;
            _orderMenu = GetOrderList();
            _whenNewRequestToUpdatePlaylistsInternal = new Subject<Unit>();
            _whenNewRequestToUpdatePlaylists = new Subject<Unit>();

            //Bgms
            observableBgmEntries
                .AutoRefresh(p => p.DbRoot.TestDispOrder, TimeSpan.FromMilliseconds(1))
                .Sort(SortExpressionComparer<BgmEntryViewModel>.Ascending(p => p.HiddenInSoundTest).ThenByAscending(p => p.DbRoot.TestDispOrder), SortOptimisations.ComparesImmutableValuesOnly, 8000)
                .TreatMovesAsRemoveAdd()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _bgms)
                .DisposeMany()
                .Subscribe();

            //Playlists
            observablePlaylistEntries
                .Sort(SortExpressionComparer<PlaylistEntryViewModel>.Ascending(p => p.Title), SortOptimisations.ComparesImmutableValuesOnly, 8000)
                .TreatMovesAsRemoveAdd()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _playlists)
                .DisposeMany()
                .Subscribe();

            //How
            var baseObs = observablePlaylistEntries
                .AutoRefreshOnObservable(p => this.WhenPlaylistSelected)
                .Filter(p => this.SelectedPlaylistEntry != null && this.SelectedPlaylistEntry.Id == p.Id)
                .TransformMany(p => p.Tracks, p => p.Value)
                .AutoRefreshOnObservable(p => this.WhenPlaylistOrderSelected)
                .Filter(p => p.Key == this.SelectedPlaylistOrder)
                .TransformMany(p => p.Value, p => p.UniqueId);
            baseObs
                .AutoRefresh(p => p.Order)
                .Sort(SortExpressionComparer<PlaylistEntryValueViewModel>.Ascending(p => p.Order), SortOptimisations.None, 8000)
                .TreatMovesAsRemoveAdd()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _selectedPlaylistOrderedEntry)
                .DisposeMany()
                .Subscribe((o) => FocusAfterMove());
            baseObs
                .AutoRefresh(p => p.Incidence)
                .Throttle(TimeSpan.FromSeconds(1))
                .Subscribe((o) => _whenNewRequestToUpdatePlaylistsInternal.OnNext(Unit.Default));

            //Throttle changes
            this.WhenAnyObservable(p => p.WhenNewRequestToUpdatePlaylistsInternal)
                .Throttle(TimeSpan.FromSeconds(2))
                .Subscribe((o) => _whenNewRequestToUpdatePlaylists.OnNext(Unit.Default));

            ActionReorderPlaylist = ReactiveCommand.CreateFromTask<DataGridCellPointerPressedEventArgs>(ReorderPlaylist);
            ActionSendToPlaylist = ReactiveCommand.CreateFromTask<DataGridCellPointerPressedEventArgs>(SendToPlaylist);
            ActionInitializeDragAndDrop = ReactiveCommand.Create<DataGrid>(InitializeDragAndDropHandlers);
            ActionSelectPlaylist = ReactiveCommand.Create<PlaylistEntryViewModel>(SelectPlaylistId);
            ActionSelectPlaylistOrder = ReactiveCommand.Create<ComboItem>(SelectPlaylistOrder);
            ActionDeletePlaylistItem = ReactiveCommand.Create<PlaylistEntryValueViewModel>(RemoveItem);

            //Trigger behavior subjets
            _whenPlaylistSelected = new BehaviorSubject<PlaylistEntryViewModel>(_playlists.FirstOrDefault());
            _whenPlaylistOrderSelected = new BehaviorSubject<ComboItem>(_orderMenu.FirstOrDefault());
        }

        #region Drag & Drop
        public void InitializeDragAndDropHandlers(DataGrid grid)
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
                if (e.Data.Get(DATAOBJECT_FORMAT_BGM) is BgmEntryViewModel sourceBgmObj)
                {
                    AddToPlaylist(sourceBgmObj, destinationObj);
                }
                else if (e.Data.Get(DATAOBJECT_FORMAT_PLAYLIST) is PlaylistEntryValueViewModel sourcePlaylistObj)
                {
                    ReorderPlaylist(sourcePlaylistObj, destinationObj);
                }
            }
        }
        #endregion

        #region Playlist manipulation
        public void AddToPlaylist(BgmEntryViewModel sourceObj, PlaylistEntryValueViewModel destinationObj)
        {
            var newEntry = destinationObj.Parent.AddSong(sourceObj, SelectedPlaylistOrder, (short)(destinationObj.Order + 1));
            _postReorderSelection = () => _refGrid.SelectedItem = newEntry;
            destinationObj.Parent.ReorderSongs(SelectedPlaylistOrder);
            _whenNewRequestToUpdatePlaylistsInternal.OnNext(Unit.Default);
        }

        public void ReorderPlaylist(PlaylistEntryValueViewModel sourceObj, PlaylistEntryValueViewModel destinationObj)
        {
            _postReorderSelection = () => _refGrid.SelectedItem = sourceObj;
            var isHigherThanDest = sourceObj.Order > destinationObj.Order;
            if (isHigherThanDest)
                sourceObj.Order = (short)(destinationObj.Order - 1);
            else
                sourceObj.Order = (short)(destinationObj.Order + 1);
            sourceObj.Parent.ReorderSongs(SelectedPlaylistOrder);
            _whenNewRequestToUpdatePlaylistsInternal.OnNext(Unit.Default);
        }

        public void RemoveItem(PlaylistEntryValueViewModel sourceObj)
        {
            sourceObj.Parent.RemoveSong(sourceObj.UiBgmId);
            _whenNewRequestToUpdatePlaylistsInternal.OnNext(Unit.Default);
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
            SelectedPlaylistEntry = vmPlaylist;
            _whenPlaylistSelected.OnNext(vmPlaylist);
        }

        private void SelectPlaylistOrder(ComboItem orderItem)
        {
            SelectedPlaylistOrder = short.Parse(orderItem.Id);
            _whenPlaylistOrderSelected.OnNext(orderItem);
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
            if (_whenNewRequestToUpdatePlaylists != null)
            {
                _whenNewRequestToUpdatePlaylists?.OnCompleted();
                _whenNewRequestToUpdatePlaylists?.Dispose();
            }
        }
    }
}
