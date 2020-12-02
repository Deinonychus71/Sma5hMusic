using Avalonia.Threading;
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
    public class MSBTFieldViewModel : ViewModelBase
    {
        private Dictionary<string, string> _msbtValues;
        private const string COPY_ACTION_ALL = "all";
        private const string COPY_ACTION_EMPTY = "empty";
        private IEnumerable<ComboItem> _copyActions;

        public ReadOnlyObservableCollection<LocaleViewModel> Locales { get; set; }

        public bool AcceptsReturn { get; set; }

        public IEnumerable<ComboItem> CopyActions { get { return _copyActions; } }

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

        public bool HasValue()
        {
            if(MSBTValues != null)
            {
                return MSBTValues.Values.Any(p => !string.IsNullOrEmpty(p));
            }
            return false;
        }

        [Reactive]
        public ComboItem SelectedCopyAction { get; set; }
        public ReactiveCommand<LocaleViewModel, Unit> ActionChangeLocale { get; }
        public ReactiveCommand<Unit, Unit> ActionCopyToAll { get; }
        public ReactiveCommand<Unit, Unit> ActionCopyToEmptyLanguages { get; }

        public MSBTFieldViewModel()
        {
            _copyActions = GetCopyActions();
            ActionChangeLocale = ReactiveCommand.Create<LocaleViewModel>(ChangeLocale);
            ActionCopyToAll = ReactiveCommand.Create(CopyToAllLanguages);
            ActionCopyToEmptyLanguages = ReactiveCommand.Create(CopyToEmptyLanguages);

            this.WhenAnyValue(p => p.SelectedCopyAction).Subscribe(o => HandleCopyAction(o));
            this.WhenAnyValue(p => p.SelectedLocale).Subscribe(o => ChangeLocale(o));
            this.WhenAnyValue(p => p.CurrentLocalizedValue).Subscribe((p) => { SaveValueToCurrentLocale(); });
        }

        private void ChangeLocale(LocaleViewModel locale)
        {
            if (MSBTValues != null && SelectedLocale != null)
            {
                SetCurrentLocalizedValue();
            }
        }

        private void HandleCopyAction(ComboItem o)
        {
            if (o == null)
                return;

            if (o.Id == COPY_ACTION_ALL)
                CopyToAllLanguages();
            else if (o.Id == COPY_ACTION_EMPTY)
                CopyToEmptyLanguages();

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                SelectedCopyAction = null;
            }, DispatcherPriority.Background);
        }

        private void CopyToEmptyLanguages()
        {
            foreach (var locale in Locales)
            {
                if (locale.Id == SelectedLocale.Id)
                    continue;
                if (!_msbtValues.ContainsKey(locale.Id))
                    _msbtValues.Add(locale.Id, string.Empty);
                if (string.IsNullOrEmpty(MSBTValues[locale.Id]))
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
            if (_msbtValues != null && SelectedLocale != null)
            {
                if (!_msbtValues.ContainsKey(SelectedLocale.Id))
                    _msbtValues.Add(SelectedLocale.Id, string.Empty);
                CurrentLocalizedValue = _msbtValues[SelectedLocale.Id];
            }
        }

        private void SaveValueToCurrentLocale()
        {
            if (_msbtValues != null)
            {
                if (!_msbtValues.ContainsKey(SelectedLocale.Id))
                    _msbtValues.Add(SelectedLocale.Id, string.Empty);
                MSBTValues[SelectedLocale.Id] = CurrentLocalizedValue;
            }
        }

        private IEnumerable<ComboItem> GetCopyActions()
        {
            return new List<ComboItem>()
            {
                new ComboItem(COPY_ACTION_ALL, "All other languages"),
                new ComboItem(COPY_ACTION_EMPTY, "All languages with empty values")
            };
        }
    }
}
