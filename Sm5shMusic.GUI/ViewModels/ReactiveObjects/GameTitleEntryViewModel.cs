using AutoMapper;
using ReactiveUI.Fody.Helpers;
using Sm5sh.Mods.Music.Models;
using Sm5shMusic.GUI.Interfaces;
using System.Collections.Generic;

namespace Sm5shMusic.GUI.ViewModels
{
    public class GameTitleEntryViewModel : BgmBaseViewModel<GameTitleEntry>
    {
        //Getters/Private Setters - For This View Only
        public bool AllFlag { get; set; }
        public string UiGameTitleId { get; set; }
        public string NameId { get; set; }
        public string UiSeriesId { get; set; }
        public bool Unk1 { get; set; }
        public int Release { get; set; }
        public Dictionary<string, string> MSBTTitle { get; set; }

        //To obtain reactive change for locale
        [Reactive]
        public string Title { get; set; }

        //[Reactive]
        public SeriesEntryViewModel SeriesViewModel { get { return _audioStateManager.GetSeriesViewModel(UiSeriesId); } }

        public GameTitleEntryViewModel()
            : base(null, null, null)
        {

        }

        public GameTitleEntryViewModel(IAudioStateViewModelManager audioStateManager, IMapper mapper, GameTitleEntry gameTitleEntry)
            : base(audioStateManager, mapper, gameTitleEntry)
        {
            UiGameTitleId = gameTitleEntry.UiGameTitleId;
        }

        public override BgmBaseViewModel<GameTitleEntry> GetCopy()
        {
            return _mapper.Map(this, new GameTitleEntryViewModel(_audioStateManager, _mapper, new GameTitleEntry(UiGameTitleId, MusicMod)));
        }

        public void LoadLocalized(string locale)
        {
            if (string.IsNullOrEmpty(locale))
                return;

            if (MSBTTitle != null && MSBTTitle.ContainsKey(locale))
                Title = MSBTTitle[locale];
            else
                Title = UiGameTitleId;
        }
    }
}
