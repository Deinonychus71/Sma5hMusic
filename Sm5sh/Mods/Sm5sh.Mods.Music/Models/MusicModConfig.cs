using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Sm5sh.Mods.Music.Models
{
    public class MusicModConfig
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("author")]
        public string Author { get; set; }
        [JsonProperty("prefix")]
        public string Prefix { get; set; }
        [JsonProperty("games")]
        public List<Game> Games { get; set; }
    }

    public class Song
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("file_name")]
        public string FileName { get; set; }
        [JsonProperty("title")]
        public Dictionary<string, string> Title { get; set; }
        [JsonProperty("copyright")]
        public Dictionary<string, string> Copyright { get; set; }
        [JsonProperty("author")]
        public Dictionary<string, string> Author { get; set; }

        [JsonProperty("record_type")]
        public string RecordType { get; set; }

        [JsonProperty("playlists")]
        public List<PlaylistInfo> Playlists { get; set; }

        [JsonProperty("special_category")]
        public SpecialCategory SpecialCategory { get; set; }
    }

    public class Game
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("title")]
        public Dictionary<string,string> Title { get; set; }
        [JsonProperty("series_id")]
        public string SeriesId { get; set; }
        [JsonProperty("songs")]
        public List<Song> Songs { get; set; }
    }

    public class PlaylistInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class SpecialCategory
    {
        [JsonProperty("category")]
        public EnumSpecialCategories Category { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public enum EnumSpecialCategories
    {
        Unknown,
        //mario_hurry_up,
        //mario_maker,
        [EnumMember(Value = "persona_stage")]
        Persona,
        [EnumMember(Value = "sf_pinch")]
        SF_Pinch,
    }
}
