using AutoMapper;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sma5h.Mods.Music.Models;
using Sma5hMusic.GUI.Interfaces;
using System.Collections.Generic;

namespace Sma5hMusic.GUI.ViewModels
{
    public class GameTitleEntryViewModel : BgmBaseViewModel<GameTitleEntry>
    {
        private string _currentLocale;
        private string _uiSeriesId;

        //Getters/Private Setters - For This View Only
        public bool AllFlag { get; set; }
        public string UiGameTitleId { get; }
        public string NameId { get; set; }
        public string UiSeriesId
        {
            get => _uiSeriesId;
            set
            {
                this.RaiseAndSetIfChanged(ref _uiSeriesId, value);
                this.RaisePropertyChanged(nameof(SeriesViewModel));
            }
        }
        public bool Unk1 { get; set; }
        public int Release { get; set; }
        public Dictionary<string, string> MSBTTitle { get; set; }

        //To obtain reactive change for locale
        [Reactive]
        public string Title { get; set; }

        //[Reactive]
        public SeriesEntryViewModel SeriesViewModel { get { return _viewModelManager.GetSeriesViewModel(UiSeriesId); } }

        public GameTitleEntryViewModel()
            : base(null, null, null)
        {

        }

        public GameTitleEntryViewModel(IViewModelManager viewModelManager, IMapper mapper, GameTitleEntry gameTitleEntry)
            : base(viewModelManager, mapper, gameTitleEntry)
        {
            UiGameTitleId = gameTitleEntry.UiGameTitleId;
        }

        public void LoadLocalized(string locale)
        {
            if (!string.IsNullOrEmpty(locale))
                _currentLocale = locale;

            if (string.IsNullOrEmpty(_currentLocale))
                return;

            if (MSBTTitle != null && MSBTTitle.ContainsKey(_currentLocale))
                Title = MSBTTitle[_currentLocale];
            else
                Title = UiGameTitleId;
        }

        public override ReactiveObjectBaseViewModel GetCopy()
        {
            return _mapper.Map(this, new GameTitleEntryViewModel(_viewModelManager, _mapper, GetReferenceEntity()));
        }

        public override ReactiveObjectBaseViewModel SaveChanges()
        {
            var original = _viewModelManager.GetGameTitleViewModel(UiGameTitleId);
            _mapper.Map(this, original.GetReferenceEntity());
            _mapper.Map(this, original);
            original.LoadLocalized(_currentLocale);
            return original;
        }
    }
}
