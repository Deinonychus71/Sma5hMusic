using Newtonsoft.Json;

namespace Sm5shMusic.Models
{
    public class StagePlaylistModConfig
    {
        [JsonProperty("stage_id")]
        public string StageId { get; set; }
        [JsonProperty("playlist_id")]
        public string PlaylistId { get; set; }
        [JsonProperty("order_id")]
        public int OrderId { get; set; }
    }
}
