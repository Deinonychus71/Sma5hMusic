using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sma5h.Mods.Music;
using Sma5hMusic.GUI.Helpers;
using Sma5hMusic.GUI.Interfaces;
using Sma5hMusic.GUI.Models;
using Sma5hMusic.GUI.Views;
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
        private readonly IDialogWindow _rootDialog;
        private readonly IMessageDialog _messageDialog;
        private readonly ReadOnlyObservableCollection<BgmDbRootEntryViewModel> _bgms;
        private readonly ReadOnlyObservableCollection<PlaylistEntryViewModel> _playlists;
        private readonly ReadOnlyObservableCollection<StageEntryViewModel> _stages;
        private readonly ReadOnlyObservableCollection<PlaylistEntryValueViewModel> _selectedPlaylistOrderedEntry;
        private readonly BehaviorSubject<PlaylistEntryViewModel> _whenPlaylistSelected;
        private readonly BehaviorSubject<StageEntryViewModel> _whenStageSelected;
        private readonly BehaviorSubject<ComboItem> _whenPlaylistOrderSelected;
        private readonly Subject<Unit> _whenNewRequestToUpdatePlaylistsInternal;
        private readonly Subject<Unit> _whenNewRequestToUpdatePlaylists;
        private readonly Subject<Unit> _whenNewRequestToCreatePlaylist;
        private readonly Subject<Unit> _whenNewRequestToEditPlaylist;
        private readonly Subject<Unit> _whenNewRequestToDeletePlaylist;
        private readonly Subject<Unit> _whenNewRequestToAssignPlaylistToStage;
        private readonly Subject<Unit> _whenUIUpdateWaitForFocus;
        private readonly List<ComboItem> _orderMenu;
        private ushort _incidenceCopy = ushort.MaxValue;
        private Action _postReorderSelection;
        private DataGrid _refGrid;
        private DataGrid _refBgmGrid;

        public IEnumerable<ComboItem> OrderMenu { get { return _orderMenu; } }
        public ContextMenuViewModel VMContextMenu { get; }
        [Reactive]
        public bool CopyValueExists { get; private set; }

        public IObservable<PlaylistEntryViewModel> WhenPlaylistSelected { get { return _whenPlaylistSelected; } }
        public IObservable<StageEntryViewModel> WhenStageSelected { get { return _whenStageSelected; } }
        public IObservable<ComboItem> WhenPlaylistOrderSelected { get { return _whenPlaylistOrderSelected; } }
        private IObservable<Unit> WhenNewRequestToUpdatePlaylistsInternal { get { return _whenNewRequestToUpdatePlaylistsInternal; } }
        public IObservable<Unit> WhenNewRequestToUpdatePlaylists { get { return _whenNewRequestToUpdatePlaylists; } }
        public IObservable<Unit> WhenNewRequestToCreatePlaylist { get { return _whenNewRequestToCreatePlaylist; } }
        public IObservable<Unit> WhenNewRequestToEditPlaylist { get { return _whenNewRequestToEditPlaylist; } }
        public IObservable<Unit> WhenNewRequestToDeletePlaylist { get { return _whenNewRequestToDeletePlaylist; } }
        public IObservable<Unit> WhenNewRequestToAssignPlaylistToStage { get { return _whenNewRequestToAssignPlaylistToStage; } }
        public ReadOnlyObservableCollection<BgmDbRootEntryViewModel> Bgms { get { return _bgms; } }
        public ReadOnlyObservableCollection<PlaylistEntryViewModel> Playlists { get { return _playlists; } }
        public ReadOnlyObservableCollection<StageEntryViewModel> Stages { get { return _stages; } }
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
        public ReactiveCommand<DataGrid, Unit> ActionInitializeDragAndDropBgm { get; }
        public ReactiveCommand<PlaylistEntryViewModel, Unit> ActionSelectPlaylist { get; }
        public ReactiveCommand<StageEntryViewModel, Unit> ActionSelectStage { get; }
        public ReactiveCommand<PlaylistEntryValueViewModel, Unit> ActionDeletePlaylistItem { get; }
        public ReactiveCommand<PlaylistEntryValueViewModel, Unit> ActionHidePlaylistItem { get; }
        public ReactiveCommand<ComboItem, Unit> ActionSelectPlaylistOrder { get; }
        public ReactiveCommand<Unit, Unit> ActionCreatePlaylist { get; }
        public ReactiveCommand<Unit, Unit> ActionEditPlaylist { get; }
        public ReactiveCommand<Unit, Unit> ActionDeletePlaylist { get; }
        public ReactiveCommand<Unit, Unit> ActionAssignPlaylistToStage { get; }
        public ReactiveCommand<PlaylistEntryValueViewModel, Unit> ActionSetIncidence { get; }
        public ReactiveCommand<PlaylistEntryValueViewModel, Unit> ActionCopyIncidence { get; }
        public ReactiveCommand<PlaylistEntryValueViewModel, Unit> ActionPasteIncidence { get; }
        public ReactiveCommand<PlaylistEntryValueViewModel, Unit> ActionPasteIncidenceAll { get; }

        public PlaylistViewModel(IOptions<ApplicationSettings> config, IDialogWindow rootDialog, IMessageDialog messageDialog, IViewModelManager viewModelManager,
            IObservable<IChangeSet<BgmDbRootEntryViewModel, string>> observableBgmEntries, ContextMenuViewModel vmContextMenu)
        {
            _config = config;
            _rootDialog = rootDialog;
            _messageDialog = messageDialog;
            VMContextMenu = vmContextMenu;
            _orderMenu = GetOrderList();
            _whenNewRequestToUpdatePlaylistsInternal = new Subject<Unit>();
            _whenNewRequestToUpdatePlaylists = new Subject<Unit>();
            _whenNewRequestToCreatePlaylist = new Subject<Unit>();
            _whenNewRequestToEditPlaylist = new Subject<Unit>();
            _whenNewRequestToDeletePlaylist = new Subject<Unit>();
            _whenNewRequestToAssignPlaylistToStage = new Subject<Unit>();
            _whenUIUpdateWaitForFocus = new Subject<Unit>();
            var observablePlaylistEntries = viewModelManager.ObservablePlaylistsEntries.Connect();

            //Bgms
            observableBgmEntries
                .Sort(SortExpressionComparer<BgmDbRootEntryViewModel>.Ascending(p => p.HiddenInSoundTest).ThenByAscending(p => p.TestDispOrder), SortOptimisations.ComparesImmutableValuesOnly, 8000)
                .TreatMovesAsRemoveAdd()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _bgms)
                .DisposeMany()
                .Subscribe();

            //Playlists
            viewModelManager.ObservableStagesEntries.Connect()
                .Sort(SortExpressionComparer<StageEntryViewModel>.Ascending(p => p.Hidden)
                .ThenByAscending(p => p.TitleHidden).ThenByAscending(p => p.DispOrder), SortOptimisations.ComparesImmutableValuesOnly, 8000)
                .TreatMovesAsRemoveAdd()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _stages)
                .DisposeMany()
                .Subscribe();
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
                    _whenUIUpdateWaitForFocus.OnNext(Unit.Default);
                    NbrBgmsPlaylist = $"{SelectedPlaylistEntry.Tracks.First().Value.Count} songs ({SelectedPlaylistEntry.AllModTracks.Count} mods)";
                });

            //Throttle changes
            baseObs
               .AutoRefresh(p => p.Incidence)
               .Throttle(TimeSpan.FromSeconds(1))
               .Subscribe((o) => _whenNewRequestToUpdatePlaylistsInternal.OnNext(Unit.Default));
            this.WhenAnyObservable(p => p.WhenNewRequestToUpdatePlaylistsInternal)
                .Throttle(TimeSpan.FromSeconds(2))
                .Subscribe((o) =>
                {
                    _whenNewRequestToUpdatePlaylists.OnNext(Unit.Default);
                });
            this.WhenAnyObservable(p => p._whenUIUpdateWaitForFocus)
                .Throttle(TimeSpan.FromMilliseconds(50))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe((o) =>
                {
                    FocusAfterMove();
                });

            ActionReorderPlaylist = ReactiveCommand.CreateFromTask<DataGridCellPointerPressedEventArgs>(ReorderPlaylist);
            ActionSendToPlaylist = ReactiveCommand.CreateFromTask<DataGridCellPointerPressedEventArgs>(SendToPlaylist);
            ActionInitializeDragAndDrop = ReactiveCommand.Create<DataGrid>(InitializeDragAndDropHandlers);
            ActionInitializeDragAndDropBgm = ReactiveCommand.Create<DataGrid>(InitializeDragAndDropBgmHandlers);
            ActionSelectPlaylist = ReactiveCommand.Create<PlaylistEntryViewModel>((o) => SelectPlaylistId(o, true));
            ActionSelectStage = ReactiveCommand.Create<StageEntryViewModel>((o) => SelectStageId(o, true));
            ActionSelectPlaylistOrder = ReactiveCommand.Create<ComboItem>((o) => SelectPlaylistOrder(o, true));
            ActionDeletePlaylistItem = ReactiveCommand.CreateFromTask<PlaylistEntryValueViewModel>(RemoveItem);
            ActionHidePlaylistItem = ReactiveCommand.Create<PlaylistEntryValueViewModel>(HideItem);
            ActionCreatePlaylist = ReactiveCommand.Create(() => AddNewPlaylist());
            ActionEditPlaylist = ReactiveCommand.Create(() => EditPlaylist());
            ActionDeletePlaylist = ReactiveCommand.Create(() => DeletePlaylist());
            ActionAssignPlaylistToStage = ReactiveCommand.Create(() => AssignPlaylistToStage());
            ActionSetIncidence = ReactiveCommand.CreateFromTask<PlaylistEntryValueViewModel>(SetIncidenceValue);
            ActionCopyIncidence = ReactiveCommand.Create<PlaylistEntryValueViewModel>(CopyIncidenceValue);
            ActionPasteIncidence = ReactiveCommand.Create<PlaylistEntryValueViewModel>(PasteIncidenceValue);
            ActionPasteIncidenceAll = ReactiveCommand.Create<PlaylistEntryValueViewModel>(PasteIncidenceValueToAllOrderIds);

            //Trigger behavior subjets
            _whenStageSelected = new BehaviorSubject<StageEntryViewModel>(_stages.FirstOrDefault());
            _whenPlaylistSelected = new BehaviorSubject<PlaylistEntryViewModel>(_playlists.FirstOrDefault());
            _whenPlaylistOrderSelected = new BehaviorSubject<ComboItem>(_orderMenu.FirstOrDefault());
        }

        #region Drag & Drop
        public void InitializeDragAndDropHandlers(DataGrid grid)
        {
            _refGrid = grid;
            grid.AddHandler(DragDrop.DropEvent, Drop);
            grid.AddHandler(DragDrop.DragOverEvent, DragOver);
            grid.AddHandler(DataGrid.KeyDownEvent, RemoveItems);
        }
        public void InitializeDragAndDropBgmHandlers(DataGrid grid)
        {
            _refBgmGrid = grid;
        }

        public async Task ReorderPlaylist(DataGridCellPointerPressedEventArgs e)
        {
            if (e.Column.DisplayIndex != 0)
                return;

            var dataGrid = VisualTreeHelper.GetControlParent<DataGrid>(e.Row);
            if (e.Cell.DataContext is PlaylistEntryValueViewModel sourceObj && !sourceObj.Hidden)
            {
                var dragData = new DataObject();
                var syncCheck = dataGrid.SelectedItems.Contains(sourceObj);

                var leftClick = VisualTreeHelper.IsLeftButtonClicked(dataGrid, e.PointerPressedEventArgs);
                if (dataGrid.SelectedItems.Count == 1 || !syncCheck || leftClick)
                {
                    dragData.Set(Constants.DragAndDropDataFormats.DATAOBJECT_FORMAT_PLAYLIST, new List<PlaylistEntryValueViewModel>() { sourceObj });
                    VisualTreeHelper.AddClassStyle<DataGrid>(dataGrid, VisualTreeHelper.STYLES_CLASS_IS_DRAGGING);
                    await DragDrop.DoDragDrop(e.PointerPressedEventArgs, dragData, DragDropEffects.Move);
                    VisualTreeHelper.RemoveClassStyle<DataGrid>(dataGrid, VisualTreeHelper.STYLES_CLASS_IS_DRAGGING);
                }
                else if (dataGrid.SelectedItems.Count > 1)
                {
                    var items = new List<PlaylistEntryValueViewModel>();
                    foreach (PlaylistEntryValueViewModel item in dataGrid.SelectedItems)
                    {
                        if (item.Hidden)
                            return;
                        items.Add(item);
                    }
                    if (items.Count > 0)
                    {
                        dragData.Set(Constants.DragAndDropDataFormats.DATAOBJECT_FORMAT_PLAYLIST, items);
                        VisualTreeHelper.AddClassStyle<DataGrid>(dataGrid, VisualTreeHelper.STYLES_CLASS_IS_DRAGGING);
                        await DragDrop.DoDragDrop(e.PointerPressedEventArgs, dragData, DragDropEffects.Move);
                        VisualTreeHelper.RemoveClassStyle<DataGrid>(dataGrid, VisualTreeHelper.STYLES_CLASS_IS_DRAGGING);
                    }
                }
            }
        }

        public async Task SendToPlaylist(DataGridCellPointerPressedEventArgs e)
        {
            var dataGrid = VisualTreeHelper.GetControlParent<DataGrid>(e.Row);
            if (e.Cell.DataContext is BgmDbRootEntryViewModel vmBgmEntry)
            {
                var dragData = new DataObject();
                var syncCheck = dataGrid.SelectedItems.Contains(vmBgmEntry);

                var leftClick = VisualTreeHelper.IsLeftButtonClicked(dataGrid, e.PointerPressedEventArgs);
                if (dataGrid.SelectedItems.Count == 1 || !syncCheck || leftClick)
                {
                    dragData.Set(Constants.DragAndDropDataFormats.DATAOBJECT_FORMAT_BGM, new List<BgmDbRootEntryViewModel>() { vmBgmEntry });
                    VisualTreeHelper.AddClassStyle<DataGrid>(_refGrid, VisualTreeHelper.STYLES_CLASS_IS_DRAGGING);
                    await DragDrop.DoDragDrop(e.PointerPressedEventArgs, dragData, DragDropEffects.Move);
                    VisualTreeHelper.RemoveClassStyle<DataGrid>(_refGrid, VisualTreeHelper.STYLES_CLASS_IS_DRAGGING);
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
                        VisualTreeHelper.AddClassStyle<DataGrid>(_refGrid, VisualTreeHelper.STYLES_CLASS_IS_DRAGGING);
                        await DragDrop.DoDragDrop(e.PointerPressedEventArgs, dragData, DragDropEffects.Move);
                        VisualTreeHelper.RemoveClassStyle<DataGrid>(_refGrid, VisualTreeHelper.STYLES_CLASS_IS_DRAGGING);
                    }
                }
            }
        }

        public void DragOver(object sender, DragEventArgs e)
        {
            e.DragEffects &= DragDropEffects.Move;
            if (SelectedPlaylistEntry == null || !e.Data.Contains(Constants.DragAndDropDataFormats.DATAOBJECT_FORMAT_BGM) && !e.Data.Contains(Constants.DragAndDropDataFormats.DATAOBJECT_FORMAT_PLAYLIST))
                e.DragEffects = DragDropEffects.None;
        }

        public void Drop(object sender, DragEventArgs e)
        {
            var dataGrid = VisualTreeHelper.GetControlParent<DataGrid>(e.Source);
            VisualTreeHelper.RemoveClassStyle<DataGrid>(dataGrid, VisualTreeHelper.STYLES_CLASS_IS_DRAGGING);
            var dataGridRow = VisualTreeHelper.GetControlParent<DataGridRow>(e.Source);
            if (dataGrid == null)
                return;

            if (dataGridRow != null)
            {
                var destinationObj = ((Control)e.Source).DataContext as PlaylistEntryValueViewModel;
                if (destinationObj != null)
                {
                    var position = destinationObj.Order;
                    var point = e.GetPosition(dataGridRow);
                    if (point.Y >= dataGridRow.Bounds.Height / 2)
                        position += 1;

                    if (e.Data.Get(Constants.DragAndDropDataFormats.DATAOBJECT_FORMAT_PLAYLIST) is List<PlaylistEntryValueViewModel> sourcePlaylistObjs && sourcePlaylistObjs.Count > 0)
                    {
                        ReorderPlaylist(sourcePlaylistObjs, destinationObj, dataGrid, position);
                    }
                    else if (SelectedPlaylistEntry != null && e.Data.Get(Constants.DragAndDropDataFormats.DATAOBJECT_FORMAT_BGM) is List<BgmDbRootEntryViewModel> sourceBgmObjs)
                    {
                        AddToPlaylist(sourceBgmObjs, dataGrid, position, _config.Value.Sma5hMusicGUI.PlaylistIncidenceDefault);
                    }
                }
            }
            else if (SelectedPlaylistEntry != null && e.Data.Get(Constants.DragAndDropDataFormats.DATAOBJECT_FORMAT_BGM) is List<BgmDbRootEntryViewModel> sourceBgmObjs)
            {
                AddToPlaylist(sourceBgmObjs, dataGrid, 9999, _config.Value.Sma5hMusicGUI.PlaylistIncidenceDefault);
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

        public void AddToPlaylist(List<BgmDbRootEntryViewModel> sourceObjs, DataGrid playlistDatagrid, short order, ushort incidence)
        {
            if (order < 0)
                order = 0;
            var newEntries = SelectedPlaylistEntry.AddSongs(sourceObjs, SelectedPlaylistOrder, order, incidence);
            _postReorderSelection = () =>
            {
                _refBgmGrid.SelectedItems.Clear();
                playlistDatagrid.SelectedItems.Clear();
                newEntries.ForEach(o => playlistDatagrid.SelectedItems.Add(o));
            };
            SelectedPlaylistEntry.ReorderSongs(SelectedPlaylistOrder);
            _whenNewRequestToUpdatePlaylistsInternal.OnNext(Unit.Default);
        }

        public void ReorderPlaylist(List<PlaylistEntryValueViewModel> sourceObjs, PlaylistEntryValueViewModel destinationObj, DataGrid datagrid, short newPosition)
        {
            if (sourceObjs.Count > 0 && sourceObjs[0] != destinationObj)
            {
                if (newPosition < 0)
                    newPosition = 0;

                _postReorderSelection = () =>
                {
                    datagrid.SelectedItems.Clear();
                    sourceObjs.ForEach(o => datagrid.SelectedItems.Add(o));
                };
                sourceObjs[0].Parent.ReorderSongs(SelectedPlaylistOrder, new List<PlaylistEntryValueViewModel>() { sourceObjs }, newPosition);
                _whenNewRequestToUpdatePlaylistsInternal.OnNext(Unit.Default);
            }
        }

        public async void RemoveItems(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Delete && _refGrid.SelectedItems.Count > 0)
            {
                if (await _messageDialog.ShowWarningConfirm($"Delete '{_refGrid.SelectedItems.Count}' tracks?", "Do you really want to remove the selected songs from the playlist? Deleting the song in the playlist does not remove it from the game."))
                {
                    var items = new List<PlaylistEntryValueViewModel>();
                    foreach (PlaylistEntryValueViewModel item in _refGrid.SelectedItems)
                        items.Add(item);
                    foreach (PlaylistEntryValueViewModel item in items)
                        item.Parent.RemoveSong(item.UiBgmId);
                    _whenNewRequestToUpdatePlaylistsInternal.OnNext(Unit.Default);
                }
            }
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
            if (!sourceObj.Hidden)
            {
                sourceObj.Hidden = true;
                sourceObj.Order = -1;
                sourceObj.Parent.ReorderSongs(SelectedPlaylistOrder);
            }
            else
            {
                sourceObj.Hidden = false;
                sourceObj.Parent.ReorderAndUnhideSong(SelectedPlaylistOrder, sourceObj);
            }
            _whenNewRequestToUpdatePlaylistsInternal.OnNext(Unit.Default);
        }

        public void FocusAfterMove()
        {
            if (_postReorderSelection != null)
                _postReorderSelection();
            _postReorderSelection = null;
        }
        #endregion

        #region Playlists
        private void SelectStageId(StageEntryViewModel vmStage, bool triggerChanges = true)
        {
            if (triggerChanges && vmStage != null)
            {
                var playlist = _playlists.FirstOrDefault(p => p.Id == vmStage.PlaylistId);
                SelectPlaylistId(playlist, false);
                var order = _orderMenu.FirstOrDefault(p => short.Parse(p.Id) == vmStage.OrderId);
                if (order != null)
                    SelectPlaylistOrder(order, false);
            }
            _whenStageSelected.OnNext(vmStage);
        }

        private void SelectPlaylistId(PlaylistEntryViewModel vmPlaylist, bool triggerChanges = true)
        {
            SelectedPlaylistEntry = vmPlaylist;
            _whenPlaylistSelected.OnNext(vmPlaylist);
            if (triggerChanges)
            {
                var playlist = _stages.FirstOrDefault(p => p.PlaylistId == vmPlaylist.Id && p.OrderId == SelectedPlaylistOrder);
                SelectStageId(playlist, false);
            }
        }

        private void SelectPlaylistOrder(ComboItem orderItem, bool triggerChanges = true)
        {
            SelectedPlaylistOrder = short.Parse(orderItem.Id);
            _whenPlaylistOrderSelected.OnNext(orderItem);
            if (triggerChanges)
            {
                var playlist = _stages.FirstOrDefault(p => p.PlaylistId == SelectedPlaylistEntry.Id && p.OrderId == SelectedPlaylistOrder);
                SelectStageId(playlist, false);
            }
        }

        private List<ComboItem> GetOrderList()
        {
            var output = new List<ComboItem>();
            for (int i = 0; i < 16; i++)
                output.Add(new ComboItem(i.ToString(), $"Order {i}"));
            return output;
        }
        #endregion

        #region Incidence Context Menu
        private async Task SetIncidenceValue(PlaylistEntryValueViewModel vmPlaylistEntryValue)
        {
            var vmIncidenceModalPicker = new IncidencePickerModalWindowViewModel() { Incidence = vmPlaylistEntryValue.Incidence };
            var incidenceModal = new IncidencePickerModalWindow() { DataContext = vmIncidenceModalPicker };
            var result = await incidenceModal.ShowDialog<IncidencePickerModalWindow>(_rootDialog.Window);
            if (result != null && vmIncidenceModalPicker.Incidence != ushort.MaxValue)
            {
                vmPlaylistEntryValue.Incidence = vmIncidenceModalPicker.Incidence;
            }
        }

        private void CopyIncidenceValue(PlaylistEntryValueViewModel vmPlaylistEntryValue)
        {
            _incidenceCopy = vmPlaylistEntryValue.Incidence;
            CopyValueExists = true;
        }

        private void PasteIncidenceValue(PlaylistEntryValueViewModel vmPlaylistEntryValue)
        {
            vmPlaylistEntryValue.Incidence = _incidenceCopy;
        }

        private void PasteIncidenceValueToAllOrderIds(PlaylistEntryValueViewModel vmPlaylistEntryValue)
        {
            if (vmPlaylistEntryValue.Parent == null)
                return;

            var parent = vmPlaylistEntryValue.Parent;
            for (short i = 0; i < 16; i++)
            {
                var vmPlaylistEntryValueOrder = parent.Tracks[i].FirstOrDefault(p => p.UiBgmId == vmPlaylistEntryValue.UiBgmId);
                PasteIncidenceValue(vmPlaylistEntryValueOrder);
            }
        }
        #endregion

        public void Dispose()
        {
            if (_whenUIUpdateWaitForFocus != null)
            {
                _whenUIUpdateWaitForFocus?.OnCompleted();
                _whenUIUpdateWaitForFocus?.Dispose();
            }
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
            if (_whenStageSelected != null)
            {
                _whenStageSelected?.OnCompleted();
                _whenStageSelected?.Dispose();
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
