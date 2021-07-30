using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using Sma5h.Helpers;
using Sma5h.Mods.Music;
using Sma5h.Mods.Music.Helpers;
using Sma5h.Mods.Music.Interfaces;
using Sma5h.Mods.Music.Models;
using Sma5hMusic.GUI.Helpers;
using Sma5hMusic.GUI.Interfaces;
using Sma5hMusic.GUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sma5hMusic.GUI.ViewModels
{
    public class SeriesPropertiesModalWindowViewModel : ModalBaseViewModel<SeriesEntryViewModel>
    {
        private readonly IOptionsMonitor<ApplicationSettings> _config;
        private readonly ReadOnlyObservableCollection<SeriesEntryViewModel> _series;
        private const string REGEX_REPLACE = @"[^a-zA-Z0-9_]";
        private readonly string REGEX_VALIDATION = $"^{MusicConstants.InternalIds.SERIES_ID_PREFIX}[a-z0-9_]+$";
        private readonly ILogger _logger;
        private readonly IGUIStateManager _guiStateManager;
        private readonly IViewModelManager _viewModelManager;

        public IMusicMod ModManager { get; }

        public MSBTFieldViewModel MSBTTitleEditor { get; set; }

        [Reactive]
        public string UiSeriesId { get; set; }

        [Reactive]
        public string NameId { get; set; }

        [Reactive]
        public sbyte DispOrder { get; set; }

        [Reactive]
        public sbyte DispOrderSound { get; set; }

        [Reactive]
        public bool Unk1 { get; set; }

        [Reactive]
        public bool IsDlc { get; set; }

        [Reactive]
        public bool IsPatch { get; set; }

        [Reactive]
        public string DlcCharaId { get; set; }

        [Reactive]
        public bool IsUseAmiiboBg { get; set; }

        [Reactive]
        public bool IsEdit { get; set; }

        [Reactive]
        public bool IsInSoundTest { get; set; }

        public ReadOnlyObservableCollection<SeriesEntryViewModel> Series { get { return _series; } }

        public SeriesPropertiesModalWindowViewModel(IOptionsMonitor<ApplicationSettings> config, ILogger<SeriesPropertiesModalWindowViewModel> logger, IViewModelManager viewModelManager,
            IGUIStateManager guiStateManager)
        {
            _config = config;
            _logger = logger;
            _guiStateManager = guiStateManager;
            _viewModelManager = viewModelManager;

            //Bind observables
            viewModelManager.ObservableSeries.Connect()
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _series)
               .DisposeMany()
               .Subscribe();

            //Set up MSBT Fields
            var defaultLocale = _config.CurrentValue.Sma5hMusicGUI.DefaultGUILocale;
            var defaultLocaleItem = new ComboItem(defaultLocale, Constants.GetLocaleDisplayName(defaultLocale));
            MSBTTitleEditor = new MSBTFieldViewModel()
            {
                //Locales = Locales,
                SelectedLocale = defaultLocaleItem,
                CurrentLocalizedValue = string.Empty
            };

            //Validation
            this.ValidationRule(p => p.UiSeriesId,
                p => !string.IsNullOrEmpty(p) && Regex.IsMatch(p, REGEX_VALIDATION),
                $"The Series ID must start by '{MusicConstants.InternalIds.SERIES_ID_PREFIX}' and only contain lowercase letters, digits and underscore.");

            this.ValidationRule(p => p.UiSeriesId,
              p => p != null && p.Length <= MusicConstants.GameResources.SeriesMaximumSize,
              $"The Series ID is too long. Maximum is {MusicConstants.GameResources.SeriesMaximumSize}");

            this.ValidationRule(p => p.UiSeriesId,
             p => p != null && p.Length >= MusicConstants.GameResources.SeriesMinimumSize,
             $"The Series ID is too short. Minimum is {MusicConstants.GameResources.SeriesMinimumSize}");

            this.ValidationRule(p => p.UiSeriesId,
                p => (IsEdit || !_series.Select(p => p.UiSeriesId).Contains(p)),
                $"The Series ID already exists.");

            this.ValidationRule(p => p.MSBTTitleEditor.CurrentLocalizedValue,
                p => !string.IsNullOrEmpty(p),
                $"Please give a title to your series (in at least one language).");

            this.WhenAnyValue(p => p.MSBTTitleEditor.CurrentLocalizedValue).Subscribe((o) => { FormatSeriesId(o); });
        }

        private void FormatSeriesId(string seriesId)
        {
            if (!IsEdit)
            {
                if (string.IsNullOrEmpty(seriesId))
                {
                    UiSeriesId = MusicConstants.InternalIds.SERIES_ID_PREFIX;
                    NameId = string.Empty;
                }
                else
                {
                    NameId = Regex.Replace(seriesId.Replace(" ", "_"), REGEX_REPLACE, string.Empty).ToLower();
                    UiSeriesId = $"{MusicConstants.InternalIds.SERIES_ID_PREFIX}{NameId}";
                }
            }
        }

        protected override async Task<bool> SaveChanges()
        {
            _logger.LogDebug("Save Changes");

            if (!IsEdit)
            {
                var newSeriesEntry = new SeriesEntry(UiSeriesId, EntrySource.Mod);
                await _guiStateManager.CreateNewSeriesEntry(newSeriesEntry);
                _refSelectedItem = _viewModelManager.GetSeriesViewModel(UiSeriesId);
            }

            var soundTestValue = IsInSoundTest ? 0 : -1;
            _refSelectedItem.DispOrder = (sbyte)soundTestValue;
            if (_refSelectedItem.IsMod)
                _refSelectedItem.DispOrderSound = (sbyte)soundTestValue;

            _refSelectedItem.MSBTTitle = SaveMSBTValues(MSBTTitleEditor.MSBTValues);
            _refSelectedItem.NameId = UiSeriesId.TrimStart(MusicConstants.InternalIds.SERIES_ID_PREFIX);
            _refSelectedItem.Unk1 = Unk1;
            _refSelectedItem.IsDlc = IsDlc;
            _refSelectedItem.IsPatch = IsPatch;
            _refSelectedItem.DlcCharaId = DlcCharaId;
            _refSelectedItem.IsUseAmiiboBg = IsUseAmiiboBg;

            return true;
        }

        private Dictionary<string, string> SaveMSBTValues(Dictionary<string, string> msbtValues)
        {
            var output = new Dictionary<string, string>();
            var copyToEmptyLocales = _config.CurrentValue.Sma5hMusicGUI.CopyToEmptyLocales;
            var defaultMSBTLocale = _config.CurrentValue.Sma5hMusicGUI.DefaultMSBTLocale;
            if (msbtValues != null)
            {
                foreach (var msbtValue in msbtValues)
                {
                    if (!string.IsNullOrEmpty(msbtValue.Value))
                        output.Add(msbtValue.Key, msbtValue.Value);
                    else if (copyToEmptyLocales && msbtValues.ContainsKey(defaultMSBTLocale))
                        output.Add(msbtValue.Key, msbtValues[defaultMSBTLocale]);
                }
            }
            return output;
        }

        protected override void LoadItem(SeriesEntryViewModel item)
        {
            _logger.LogDebug("Load Item");

            if (item == null)
            {
                IsInSoundTest = true;
                IsEdit = false;
                UiSeriesId = string.Empty;
                NameId = string.Empty;
                DispOrder = 0;
                DispOrderSound = 0;
                Unk1 = false;
                IsDlc = false;
                IsPatch = false;
                DlcCharaId = string.Empty;
                IsUseAmiiboBg = false;
                MSBTTitleEditor.MSBTValues = new Dictionary<string, string>();
            }
            else
            {
                IsInSoundTest = item.DispOrderSound > -1;
                IsEdit = true;
                UiSeriesId = item.UiSeriesId;
                NameId = item.NameId;
                DispOrder = item.DispOrder;
                DispOrderSound = item.DispOrderSound;
                Unk1 = item.Unk1;
                IsDlc = item.IsDlc;
                IsPatch = item.IsPatch;
                DlcCharaId = item.DlcCharaId;
                IsUseAmiiboBg = item.IsUseAmiiboBg;
                MSBTTitleEditor.MSBTValues = item.MSBTTitle;
            }
        }
    }
}
