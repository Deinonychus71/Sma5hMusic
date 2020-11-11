using Sm5shMusic.Helpers;

namespace Sm5shMusic.Models
{
    public class GameTitleDbNewEntry
    {
        public string SeriesId { get; set; }
        public string UiGameTitleId { get { return $"{Constants.InternalIds.GameTitleIdPrefix}{GameTitleId}"; } }
        public string GameTitleId { get; set; }
        public string NameId { get { return GameTitleId; } }
        public string UiSeriesId { get { return $"{Constants.InternalIds.GameSeriesIdPrefix}{SeriesId}"; } }
    }
}
