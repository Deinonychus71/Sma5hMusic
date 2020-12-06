using DynamicData;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace Sm5shMusic.GUI.ViewModels
{
    public class ModPickerModalWindowViewModel : ModalBaseViewModel<ModEntryViewModel>
    {
        private readonly ReadOnlyObservableCollection<ModEntryViewModel> _mods;

        public ReadOnlyObservableCollection<ModEntryViewModel> Mods { get { return _mods; } }

        public ModPickerModalWindowViewModel(IObservable<IChangeSet<ModEntryViewModel, string>> observableMods)
        {
            //Bind observables
            observableMods
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _mods)
               .DisposeMany()
               .Subscribe();
        }

        protected override IObservable<bool> GetValidationRule()
        {
            return this.WhenAnyValue(x => x.SelectedItem, x => x != null && !string.IsNullOrEmpty(x.Name));
        }
    }
}
