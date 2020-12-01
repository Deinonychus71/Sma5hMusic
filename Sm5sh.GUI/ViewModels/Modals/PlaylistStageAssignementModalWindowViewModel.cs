using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sm5sh.GUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Sm5sh.GUI.ViewModels
{
    public class PlaylistStageAssignementModalWindowViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private readonly IEnumerable<StageEntryViewModel> _stages;
        private readonly IEnumerable<ComboItem> _orders;
        private readonly ReadOnlyObservableCollection<PlaylistEntryViewModel> _playlists;
        private readonly ReadOnlyObservableCollection<PlaylistEntryValueViewModel> _tracks;

        public ReadOnlyObservableCollection<PlaylistEntryViewModel> Playlists { get { return _playlists; } }
        public ReadOnlyObservableCollection<PlaylistEntryValueViewModel> Tracks { get { return _tracks; } }

        public IEnumerable<StageEntryViewModel> Stages { get; private set; }
        public IEnumerable<ComboItem> Orders { get { return _orders; } }

        [Reactive]
        public StageEntryViewModel SelectedStageEntry { get; set; }

        [Reactive]
        public PlaylistEntryViewModel SelectedPlaylistEntry { get; set; }

        [Reactive]
        public ComboItem SelectedOrderId { get; set; }

        public ReactiveCommand<Window, Unit> ActionCancel { get; }
        public ReactiveCommand<Window, Unit> ActionOK { get; }


        public PlaylistStageAssignementModalWindowViewModel(ILogger<ModPickerModalWindowViewModel> logger, IObservable<IChangeSet<PlaylistEntryViewModel, string>> observablePlaylists,
            List<StageEntryViewModel> stages)
        {
            _logger = logger;
            _stages = stages;
            _orders = GetOrderList();

            //Bind observables
            observablePlaylists
               .Sort(SortExpressionComparer<PlaylistEntryViewModel>.Ascending(p => p.Title), SortOptimisations.ComparesImmutableValuesOnly, 8000)
               .TreatMovesAsRemoveAdd()
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _playlists)
               .DisposeMany()
               .Subscribe();
            observablePlaylists
                .AutoRefreshOnObservable(p => this.WhenAnyValue(p => p.SelectedPlaylistEntry))
                .Filter(p => this.SelectedPlaylistEntry != null && this.SelectedPlaylistEntry.Id == p.Id)
                .TransformMany(p => p.Tracks, p => p.Value)
                .AutoRefreshOnObservable(p => this.WhenAnyValue(p => p.SelectedOrderId))
                .Filter(p => this.SelectedOrderId != null && p.Key == short.Parse(this.SelectedOrderId.Id))
                .TransformMany(p => p.Value, p => p.UniqueId)
                .Sort(SortExpressionComparer<PlaylistEntryValueViewModel>.Ascending(p => p.Order), SortOptimisations.None, 8000)
                .TreatMovesAsRemoveAdd()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _tracks)
                .DisposeMany()
                .Subscribe();

            this.WhenAnyValue(p => p.SelectedStageEntry).Subscribe(p => SelectStage(p));
            this.WhenAnyValue(p => p.SelectedPlaylistEntry).Subscribe(p => SavePlaylistEntry(p));
            this.WhenAnyValue(p => p.SelectedOrderId).Subscribe(p => SaveOrderId(p));

            ActionCancel = ReactiveCommand.Create<Window>(Cancel);
            ActionOK = ReactiveCommand.Create<Window>(Save);
        }

        public void LoadControl()
        {
            Stages = null;
            SelectedStageEntry = null;
            Stages = _stages.Select(p => new StageEntryViewModel(p.GetStageEntryReference())).ToList();
            SelectedStageEntry = Stages.FirstOrDefault();
        }

        private void SelectStage(StageEntryViewModel vmStage)
        {
            if (vmStage != null)
            {
                string orderId = vmStage.OrderId.ToString();
                SelectedPlaylistEntry = _playlists.FirstOrDefault(p => p.Id == vmStage.PlaylistId);
                SelectedOrderId = _orders.FirstOrDefault(p => p.Id == orderId);
            }
        }

        private void SavePlaylistEntry(PlaylistEntryViewModel vmPlaylist)
        {
            if (SelectedStageEntry != null && vmPlaylist != null)
            {
                SelectedStageEntry.PlaylistId = vmPlaylist.Id;
            }
        }

        private void SaveOrderId(ComboItem order)
        {
            if (SelectedStageEntry != null && order != null)
            {
                SelectedStageEntry.OrderId = byte.Parse(order.Id);
            }
        }

        private void Cancel(Window w)
        {
            Stages = _stages;
            w.Close();
        }

        private void Save(Window window)
        {
            window.Close(window);
        }

        private List<ComboItem> GetOrderList()
        {
            var output = new List<ComboItem>();
            for (int i = 0; i < 16; i++)
                output.Add(new ComboItem(i.ToString(), $"Order {i}"));
            return output;
        }
    }
}
