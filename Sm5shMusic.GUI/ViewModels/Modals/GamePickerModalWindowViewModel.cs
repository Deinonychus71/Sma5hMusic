using Avalonia.Controls;
using DynamicData;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;

namespace Sm5shMusic.GUI.ViewModels
{
    public class GamePickerModalWindowViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private readonly ReadOnlyObservableCollection<GameTitleEntryViewModel> _games;

        public ReadOnlyObservableCollection<GameTitleEntryViewModel> Games { get { return _games; } }

        public ReactiveCommand<Window, Unit> ActionCancel { get; }
        public ReactiveCommand<Window, Unit> ActionSelect { get; }

        [Reactive]
        public GameTitleEntryViewModel SelectedGameTitleEntry { get; private set; }


        public GamePickerModalWindowViewModel(ILogger<GamePickerModalWindowViewModel> logger, IObservable<IChangeSet<GameTitleEntryViewModel, string>> observableGames)
        {
            _logger = logger;

            //Bind observables
            observableGames
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _games)
               .DisposeMany()
               .Subscribe();

            var canExecute = this.WhenAnyValue(x => x.SelectedGameTitleEntry, x => x != null && !string.IsNullOrEmpty(x.UiSeriesId));
            ActionCancel = ReactiveCommand.Create<Window>(Cancel);
            ActionSelect = ReactiveCommand.Create<Window>(Select, canExecute);
        }

        private void Cancel(Window w)
        {
            SelectedGameTitleEntry = null;
            w.Close();
        }

        private void Select(Window window)
        {
            window.Close(window);
        }
    }
}
