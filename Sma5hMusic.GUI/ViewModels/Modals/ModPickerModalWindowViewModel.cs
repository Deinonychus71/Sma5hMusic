using DynamicData;
using ReactiveUI;
using Sma5hMusic.GUI.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace Sma5hMusic.GUI.ViewModels
{
    public class ModPickerModalWindowViewModel : ModalBaseViewModel<ModEntryViewModel>
    {
        private readonly ReadOnlyObservableCollection<ModEntryViewModel> _mods;

        public ReadOnlyObservableCollection<ModEntryViewModel> Mods { get { return _mods; } }

        public ModPickerModalWindowViewModel(IViewModelManager viewModelManager)
        {
            //Bind observables
            viewModelManager.ObservableModsEntries.Connect()
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
