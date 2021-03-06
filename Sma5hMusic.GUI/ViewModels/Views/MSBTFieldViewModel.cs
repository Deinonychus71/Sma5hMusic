using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sma5hMusic.GUI.Helpers;
using Sma5hMusic.GUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Sma5hMusic.GUI.ViewModels
{
    public class MSBTFieldViewModel : ViewModelBase
    {
        private Dictionary<string, string> _msbtValues;
        private const string COPY_ACTION_ALL = "all";
        private const string COPY_ACTION_EMPTY = "empty";
        private readonly Dictionary<string, List<string>> _useRecentDict;
        private readonly IEnumerable<ComboItem> _copyActions;
        private readonly IEnumerable<ComboItem> _locales;

        public IEnumerable<ComboItem> Locales { get { return _locales; } }

        public bool AcceptsReturn { get; set; }

        public IEnumerable<ComboItem> CopyActions { get { return _copyActions; } }

        [Reactive]
        public IEnumerable<string> UseRecent { get; set; }

        [Reactive]
        public ComboItem SelectedLocale { get; set; }

        public Dictionary<string, string> MSBTValues
        {
            get => _msbtValues;
            set
            {
                this.RaiseAndSetIfChanged(ref _msbtValues, value);
                InitMsbtArray();
                SetCurrentLocalizedValue();
            }
        }

        [Reactive]
        public string CurrentLocalizedValue { get; set; }

        public bool HasValue()
        {
            if (MSBTValues != null)
            {
                return MSBTValues.Values.Any(p => !string.IsNullOrEmpty(p));
            }
            return false;
        }

        [Reactive]
        public bool DisplayRecents { get; set; }

        [Reactive]
        public ComboItem SelectedCopyAction { get; set; }
        [Reactive]
        public string SelectedRecentAction { get; set; }
        public ReactiveCommand<ComboItem, Unit> ActionChangeLocale { get; }
        public ReactiveCommand<Unit, Unit> ActionCopyToAll { get; }
        public ReactiveCommand<Unit, Unit> ActionCopyToEmptyLanguages { get; }

        public MSBTFieldViewModel()
        {
            _locales = Constants.CONVERTER_LOCALE.Select(p => new ComboItem(p.Key, p.Value));
            _copyActions = GetCopyActions();
            _useRecentDict = Constants.CONVERTER_LOCALE.ToDictionary(p => p.Key, p => new List<string>());
            ActionChangeLocale = ReactiveCommand.Create<ComboItem>(ChangeLocale);
            ActionCopyToAll = ReactiveCommand.Create(CopyToAllLanguages);
            ActionCopyToEmptyLanguages = ReactiveCommand.Create(CopyToEmptyLanguages);

            this.WhenAnyValue(p => p.SelectedRecentAction).Subscribe(o => HandleRecentAction(o));
            this.WhenAnyValue(p => p.SelectedCopyAction).Subscribe(o => HandleCopyAction(o));
            this.WhenAnyValue(p => p.SelectedLocale).Subscribe(o => ChangeLocale(o));
            this.WhenAnyValue(p => p.CurrentLocalizedValue).Subscribe((p) => { SaveValueToCurrentLocale(); });
        }

        private void ChangeLocale(ComboItem locale)
        {
            if (MSBTValues != null && SelectedLocale != null)
            {
                SetCurrentLocalizedValue();
            }
        }

        private void HandleRecentAction(string o)
        {
            if (o == null)
                return;

            CurrentLocalizedValue = o;

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                SelectedRecentAction = null;
            }, DispatcherPriority.Background);
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

        private void InitMsbtArray()
        {
            if (_msbtValues != null)
            {
                foreach (var locale in Constants.CONVERTER_LOCALE.Keys)
                {
                    if (!_msbtValues.ContainsKey(locale))
                        _msbtValues.Add(locale, string.Empty);
                }
            }
        }

        private void SetCurrentLocalizedValue()
        {
            if (_msbtValues != null && SelectedLocale != null)
            {
                if (!_msbtValues.ContainsKey(SelectedLocale.Id))
                    _msbtValues.Add(SelectedLocale.Id, string.Empty);
                CurrentLocalizedValue = _msbtValues[SelectedLocale.Id];
                UseRecent = _useRecentDict[SelectedLocale.Id];
                DisplayRecents = UseRecent.Count() > 0;
            }
        }

        public void SaveValueToRecent()
        {
            foreach(var msbtValue in _msbtValues)
            {
                var list = _useRecentDict[msbtValue.Key];
                if (!list.Contains(msbtValue.Value) && !string.IsNullOrEmpty(msbtValue.Value))
                {
                    if(list.Count > 9)
                        list.RemoveAt(list.Count - 1);
                    list.Insert(0, msbtValue.Value);
                }
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
                new ComboItem(COPY_ACTION_ALL, "To all other languages"),
                new ComboItem(COPY_ACTION_EMPTY, "To all languages with empty values")
            };
        }
    }
}
