using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sm5sh.GUI.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;

namespace Sm5sh.GUI.ViewModels
{
    public class MSBTFieldViewModel : ViewModelBase
    {
        private Dictionary<string, string> _msbtValues;

        public ReadOnlyObservableCollection<LocaleViewModel> Locales { get; set; }

        public bool AcceptsReturn { get; set; }

        [Reactive]
        public LocaleViewModel SelectedLocale { get; set; }

        public Dictionary<string, string> MSBTValues
        {
            get => _msbtValues;
            set
            {
                this.RaiseAndSetIfChanged(ref _msbtValues, value);
                SetCurrentLocalizedValue();
            }
        }

        [Reactive]
        public string CurrentLocalizedValue { get; set; }

        public ReactiveCommand<LocaleViewModel, Unit> ActionChangeLocale { get; }
        public ReactiveCommand<Unit, Unit> ActionCopyToAll { get; }
        public ReactiveCommand<Unit, Unit> ActionCopyToEmptyLanguages { get; }

        public MSBTFieldViewModel()
        {
            ActionChangeLocale = ReactiveCommand.Create<LocaleViewModel>(ChangeLocale);
            ActionCopyToAll = ReactiveCommand.Create(CopyToAllLanguages);
            ActionCopyToEmptyLanguages = ReactiveCommand.Create(CopyToEmptyLanguages);
        }

        private void ChangeLocale(LocaleViewModel locale)
        {
            MSBTValues[SelectedLocale.Id] = CurrentLocalizedValue;
            SelectedLocale = locale;
            SetCurrentLocalizedValue();
        }

        private void CopyToEmptyLanguages()
        {
            foreach(var locale in Locales)
            {
                if (locale.Id == SelectedLocale.Id)
                    continue;
                if (!_msbtValues.ContainsKey(locale.Id))
                    _msbtValues.Add(locale.Id, string.Empty);
                if(string.IsNullOrEmpty(MSBTValues[locale.Id]))
                    MSBTValues[locale.Id] = CurrentLocalizedValue;
            }
        }

        private void CopyToAllLanguages()
        {
            foreach (var locale in Locales)
            {
                if (locale.Id == SelectedLocale.Id)
                    continue;
                if (!_msbtValues.ContainsKey(locale.Id))
                    _msbtValues.Add(locale.Id, string.Empty);
                MSBTValues[locale.Id] = CurrentLocalizedValue;
            }
        }

        private void SetCurrentLocalizedValue()
        {
            if(_msbtValues != null && SelectedLocale != null)
            {
                if (!_msbtValues.ContainsKey(SelectedLocale.Id))
                    _msbtValues.Add(SelectedLocale.Id, string.Empty);
                CurrentLocalizedValue = _msbtValues[SelectedLocale.Id];
            }
        }
    }
}
