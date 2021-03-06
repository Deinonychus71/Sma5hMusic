using Newtonsoft.Json;

namespace Sma5h.Mods.Music.Models
{
    public class MusicModInformation
    {
        [JsonProperty("id")]
        public virtual string Id { get; }
        [JsonProperty("name")]
        public virtual string Name { get; set; }
        [JsonProperty("author")]
        public virtual string Author { get; set; }
        [JsonProperty("website")]
        public virtual string Website { get; set; }
        [JsonProperty("description")]
        public virtual string Description { get; set; }
        [JsonProperty("version")]
        public virtual int Version { get; set; }
    }
}
