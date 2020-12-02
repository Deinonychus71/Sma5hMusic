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
    public class ModPickerModalWindowViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private readonly ReadOnlyObservableCollection<ModEntryViewModel> _mods;

        public ReadOnlyObservableCollection<ModEntryViewModel> Mods { get { return _mods; } }

        public ReactiveCommand<Window, Unit> ActionCancel { get; }
        public ReactiveCommand<Window, Unit> ActionSelect { get; }

        [Reactive]
        public ModEntryViewModel SelectedModEntry { get; private set; }


        public ModPickerModalWindowViewModel(ILogger<ModPickerModalWindowViewModel> logger, IObservable<IChangeSet<ModEntryViewModel, string>> observableMods)
        {
            _logger = logger;

            //Bind observables
            observableMods
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _mods)
               .DisposeMany()
               .Subscribe();

            var canExecute = this.WhenAnyValue(x => x.SelectedModEntry, x => x != null && !string.IsNullOrEmpty(x.ModName));
            ActionCancel = ReactiveCommand.Create<Window>(Cancel);
            ActionSelect = ReactiveCommand.Create<Window>(Select, canExecute);
        }

        private void Cancel(Window w)
        {
            SelectedModEntry = null;
            w.Close();
        }

        private void Select(Window window)
        {
            window.Close(window);
        }
    }
}
