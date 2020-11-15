using Newtonsoft.Json;

namespace Sm5sh.Mods.StagePlaylist.Models
{
    public class StagePlaylistModConfig
    {
        [JsonProperty("stage_id")]
        public string StageId { get; set; }
        [JsonProperty("playlist_id")]
        public string PlaylistId { get; set; }
        [JsonProperty("order_id")]
        public byte OrderId { get; set; }
    }
}
