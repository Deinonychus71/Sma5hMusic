using Avalonia.Controls;
using Avalonia.Input;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sma5h.Mods.Music;
using Sma5hMusic.GUI.Interfaces;
using Sma5hMusic.GUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Sma5hMusic.GUI.ViewModels
{
    public class PlaylistViewModel : ViewModelBase
    {
        private readonly IOptions<ApplicationSettings> _config;
        private readonly IMessageDialog _messageDialog;
        private readonly ReadOnlyObservableCollection<BgmDbRootEntryViewModel> _bgms;
        private readonly ReadOnlyObservableCollection<PlaylistEntryViewModel> _playlists;
        private readonly ReadOnlyObservableCollection<PlaylistEntryValueViewModel> _selectedPlaylistOrderedEntry;
        private readonly BehaviorSubject<PlaylistEntryViewModel> _whenPlaylistSelected;
        private readonly BehaviorSubject<ComboItem> _whenPlaylistOrderSelected;
        private readonly Subject<Unit> _whenNewRequestToUpdatePlaylistsInternal;
        private readonly Subject<Unit> _whenNewRequestToUpdatePlaylists;
        private readonly Subject<Unit> _whenNewRequestToCreatePlaylist;
        private readonly Subject<Unit> _whenNewRequestToEditPlaylist;
        private readonly Subject<Unit> _whenNewRequestToDeletePlaylist;
        private readonly Subject<Unit> _whenNewRequestToAssignPlaylistToStage;
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
        public IObservable<Unit> WhenNewRequestToCreatePlaylist { get { return _whenNewRequestToCreatePlaylist; } }
        public IObservable<Unit> WhenNewRequestToEditPlaylist { get { return _whenNewRequestToEditPlaylist; } }
        public IObservable<Unit> WhenNewRequestToDeletePlaylist { get { return _whenNewRequestToDeletePlaylist; } }
        public IObservable<Unit> WhenNewRequestToAssignPlaylistToStage { get { return _whenNewRequestToAssignPlaylistToStage; } }
        public ReadOnlyObservableCollection<BgmDbRootEntryViewModel> Bgms { get { return _bgms; } }
        public ReadOnlyObservableCollection<PlaylistEntryViewModel> Playlists { get { return _playlists; } }
        public ReadOnlyObservableCollection<PlaylistEntryValueViewModel> SelectedPlaylistOrderedEntry { get { return _selectedPlaylistOrderedEntry; } }

        [Reactive]
        public PlaylistEntryViewModel SelectedPlaylistEntry { get; private set; }
        [Reactive]
        public PlaylistEntryValueViewModel SelectedPlaylistValueEntry { get; set; }
        [Reactive]
        public short SelectedPlaylistOrder { get; private set; }

        [Reactive]
        public string NbrBgmsPlaylist { get; private set; }

        public ReactiveCommand<DataGridCellPointerPressedEventArgs, Unit> ActionReorderPlaylist { get; }
        public ReactiveCommand<DataGridCellPointerPressedEventArgs, Unit> ActionSendToPlaylist { get; }
        public ReactiveCommand<DataGrid, Unit> ActionInitializeDragAndDrop { get; }
        public ReactiveCommand<PlaylistEntryViewModel, Unit> ActionSelectPlaylist { get; }
        public ReactiveCommand<PlaylistEntryValueViewModel, Unit> ActionDeletePlaylistItem { get; }
        public ReactiveCommand<PlaylistEntryValueViewModel, Unit> ActionHidePlaylistItem { get; }
        public ReactiveCommand<ComboItem, Unit> ActionSelectPlaylistOrder { get; }
        public ReactiveCommand<Unit, Unit> ActionCreatePlaylist { get; }
        public ReactiveCommand<Unit, Unit> ActionEditPlaylist { get; }
        public ReactiveCommand<Unit, Unit> ActionDeletePlaylist { get; }
        public ReactiveCommand<Unit, Unit> ActionAssignPlaylistToStage { get; }

        public PlaylistViewModel(IOptions<ApplicationSettings> config, IMessageDialog messageDialog, IObservable<IChangeSet<BgmDbRootEntryViewModel, string>> observableBgmEntries,
            IObservable<IChangeSet<PlaylistEntryViewModel, string>> observablePlaylistEntries, ContextMenuViewModel vmContextMenu)
        {
            _config = config;
            _messageDialog = messageDialog;
            VMContextMenu = vmContextMenu;
            _orderMenu = GetOrderList();
            _whenNewRequestToUpdatePlaylistsInternal = new Subject<Unit>();
            _whenNewRequestToUpdatePlaylists = new Subject<Unit>();
            _whenNewRequestToCreatePlaylist = new Subject<Unit>();
            _whenNewRequestToEditPlaylist = new Subject<Unit>();
            _whenNewRequestToDeletePlaylist = new Subject<Unit>();
            _whenNewRequestToAssignPlaylistToStage = new Subject<Unit>();

            //Bgms
            observableBgmEntries
                .Sort(SortExpressionComparer<BgmDbRootEntryViewModel>.Ascending(p => p.HiddenInSoundTest).ThenByAscending(p => p.TestDispOrder), SortOptimisations.ComparesImmutableValuesOnly, 8000)
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
                .Subscribe((o) =>
                {
                    FocusAfterMove();
                    NbrBgmsPlaylist = $"{SelectedPlaylistEntry.Tracks.Count} songs ({SelectedPlaylistEntry.AllModTracks.Count} mods)";
                });
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
            ActionDeletePlaylistItem = ReactiveCommand.CreateFromTask<PlaylistEntryValueViewModel>(RemoveItem);
            ActionHidePlaylistItem = ReactiveCommand.Create<PlaylistEntryValueViewModel>(HideItem);
            ActionCreatePlaylist = ReactiveCommand.Create(() => AddNewPlaylist());
            ActionEditPlaylist = ReactiveCommand.Create(() => EditPlaylist());
            ActionDeletePlaylist = ReactiveCommand.Create(() => DeletePlaylist());
            ActionAssignPlaylistToStage = ReactiveCommand.Create(() => AssignPlaylistToStage());

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
            if (e.Column.DisplayIndex != 0)
                return;

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

            if (e.Cell.DataContext is BgmDbRootEntryViewModel vmBgmEntry)
            {
                dragData.Set(DATAOBJECT_FORMAT_BGM, vmBgmEntry);
                await DragDrop.DoDragDrop(e.PointerPressedEventArgs, dragData, DragDropEffects.Move);
            }
        }

        public void DragOver(object sender, DragEventArgs e)
        {
            e.DragEffects &= DragDropEffects.Move;
            if (SelectedPlaylistEntry == null || !e.Data.Contains(DATAOBJECT_FORMAT_BGM) && !e.Data.Contains(DATAOBJECT_FORMAT_PLAYLIST))
                e.DragEffects = DragDropEffects.None;
        }

        public void Drop(object sender, DragEventArgs e)
        {
            var destinationObj = ((Control)e.Source).DataContext as PlaylistEntryValueViewModel;
            if (destinationObj != null)
            {
                if (e.Data.Get(DATAOBJECT_FORMAT_PLAYLIST) is PlaylistEntryValueViewModel sourcePlaylistObj)
                {
                    ReorderPlaylist(sourcePlaylistObj, destinationObj);
                }
            }
            if (SelectedPlaylistEntry != null && e.Data.Get(DATAOBJECT_FORMAT_BGM) is BgmDbRootEntryViewModel sourceBgmObj)
            {
                AddToPlaylist(sourceBgmObj, destinationObj, _config.Value.Sma5hMusicGUI.PlaylistIncidenceDefault);
            }
        }
        #endregion

        #region Playlist manipulation
        public void AssignPlaylistToStage()
        {
            _whenNewRequestToAssignPlaylistToStage.OnNext(Unit.Default);
        }

        public void AddNewPlaylist()
        {
            _whenNewRequestToCreatePlaylist.OnNext(Unit.Default);
        }

        public void EditPlaylist()
        {
            _whenNewRequestToEditPlaylist.OnNext(Unit.Default);
        }

        public void DeletePlaylist()
        {
            _whenNewRequestToDeletePlaylist.OnNext(Unit.Default);
        }

        public void AddToPlaylist(BgmDbRootEntryViewModel sourceObj, PlaylistEntryValueViewModel destinationObj, ushort incidence)
        {
            var order = destinationObj != null ? (short)(destinationObj.Order + 1) : (short)999;
            var newEntry = SelectedPlaylistEntry.AddSong(sourceObj, SelectedPlaylistOrder, order, incidence);
            _postReorderSelection = () => _refGrid.SelectedItem = newEntry;
            SelectedPlaylistEntry.ReorderSongs(SelectedPlaylistOrder);
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
            sourceObj.Hidden = false;
            sourceObj.Parent.ReorderSongs(SelectedPlaylistOrder);
            _whenNewRequestToUpdatePlaylistsInternal.OnNext(Unit.Default);
        }

        public async Task RemoveItem(PlaylistEntryValueViewModel sourceObj)
        {
            if (await _messageDialog.ShowWarningConfirm($"Delete '{sourceObj.BgmReference?.Title}'?", "Do you really want to remove this song from the playlist? Deleting the song in the playlist does not remove it from the game."))
            {
                sourceObj.Parent.RemoveSong(sourceObj.UiBgmId);
                _whenNewRequestToUpdatePlaylistsInternal.OnNext(Unit.Default);
            }
        }

        public void HideItem(PlaylistEntryValueViewModel sourceObj)
        {
            sourceObj.Hidden = true;
            sourceObj.Order = -1;
            sourceObj.Parent.ReorderSongs(SelectedPlaylistOrder);
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
            if (_whenNewRequestToUpdatePlaylistsInternal != null)
            {
                _whenNewRequestToUpdatePlaylistsInternal?.OnCompleted();
                _whenNewRequestToUpdatePlaylistsInternal?.Dispose();
            }
            if (_whenNewRequestToUpdatePlaylistsInternal != null)
            {
                _whenNewRequestToUpdatePlaylistsInternal?.OnCompleted();
                _whenNewRequestToUpdatePlaylistsInternal?.Dispose();
            }
            if (_whenPlaylistSelected != null)
            {
                _whenPlaylistSelected?.OnCompleted();
                _whenPlaylistSelected?.Dispose();
            }
            if (_whenPlaylistOrderSelected != null)
            {
                _whenPlaylistOrderSelected?.OnCompleted();
                _whenPlaylistOrderSelected?.Dispose();
            }
            if (_whenNewRequestToCreatePlaylist != null)
            {
                _whenNewRequestToCreatePlaylist?.OnCompleted();
                _whenNewRequestToCreatePlaylist?.Dispose();
            }
            if (_whenNewRequestToEditPlaylist != null)
            {
                _whenNewRequestToEditPlaylist?.OnCompleted();
                _whenNewRequestToEditPlaylist?.Dispose();
            }
            if (_whenNewRequestToDeletePlaylist != null)
            {
                _whenNewRequestToDeletePlaylist?.OnCompleted();
                _whenNewRequestToDeletePlaylist?.Dispose();
            }
            if (_whenNewRequestToAssignPlaylistToStage != null)
            {
                _whenNewRequestToAssignPlaylistToStage?.OnCompleted();
                _whenNewRequestToAssignPlaylistToStage?.Dispose();
            }
        }
    }

}
