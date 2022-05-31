using AutoMapper;
using ReactiveUI.Fody.Helpers;
using Sma5h.Mods.Music.Models;
using Sma5hMusic.GUI.Interfaces;
using System.Collections.Generic;

namespace Sma5hMusic.GUI.ViewModels
{
    public class SeriesEntryViewModel : BgmBaseViewModel<SeriesEntry>
    {
        private string _currentLocale;

        public bool AllFlag { get; set; }
        public string UiSeriesId { get; }
        public string NameId { get; set; }
        public sbyte DispOrder { get; set; }
        public sbyte DispOrderSound { get; set; }
        public sbyte SaveNo { get; set; }
        public bool Unk1 { get; set; }
        public bool IsDlc { get; set; }
        public bool IsPatch { get; set; }
        public string DlcCharaId { get; set; }
        public bool IsUseAmiiboBg { get; set; }

        [Reactive]
        public string Title { get; set; }

        public Dictionary<string, string> MSBTTitle { get; set; }

        public SeriesEntryViewModel()
        : base(null, null, null) 
        { 
        }

        public SeriesEntryViewModel(IViewModelManager viewModelManager, IMapper mapper, SeriesEntry seriesEntry)
            : base(viewModelManager, mapper, seriesEntry)
        {
            UiSeriesId = seriesEntry.UiSeriesId;
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
                Title = UiSeriesId;
        }

        /*public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is SeriesEntryViewModel p))
                return false;

            return p.UiSeriesId == this.UiSeriesId;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }*/

        public override ReactiveObjectBaseViewModel GetCopy()
        {
            return _mapper.Map(this, new SeriesEntryViewModel(_viewModelManager, _mapper, GetReferenceEntity()));
        }

        public override ReactiveObjectBaseViewModel SaveChanges()
        {
            var original = _viewModelManager.GetSeriesViewModel(UiSeriesId);
            _mapper.Map(this, original.GetReferenceEntity());
            _mapper.Map(this, original);
            original.LoadLocalized(_currentLocale);
            return original;
        }
    }
}
