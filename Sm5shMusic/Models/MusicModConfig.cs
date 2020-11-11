using Newtonsoft.Json;
using Sm5shMusic.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sm5shMusic.Models
{
    public class MusicModConfig
    {
        [JsonProperty("prefix")]
        public string Prefix { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("author")]
        public string Author { get; set; }
        [JsonProperty("songs")]
        public List<Song> Songs { get; set; }
    }

    public class Song
    {
        [JsonProperty("file_name")]
        public string FileName { get; set; }
        [JsonProperty("series_id")]
        public string SeriesId { get; set; }
        [JsonProperty("playlist_id")]
        public string PlaylistId { get; set; }
        [JsonProperty("game_title")]
        public GameTitle GameTitle { get; set; }
        [JsonProperty("song_info")]
        public SongInfo SongInfo { get; set; }
        [JsonProperty("song_flags")]
        public SongFlags SongFlags { get; set; }
        [JsonProperty("song_cue_points_override")]
        public MusicModAudioCuePoints SongCuePointsOverride { get; set; }
    }

    public class GameTitle
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("title")]
        public Dictionary<string,string> Title { get; set; }

        public string GetDefaultTitle()
        {
            if (Title.ContainsKey(Constants.DefaultLocale))
                return Title[Constants.DefaultLocale];
            return Title.Count > 0 ? Title.First().Value : string.Empty;
        }
    }

    public class SongInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("title")]
        public Dictionary<string, string> Title { get; set; }
        [JsonProperty("copyright")]
        public Dictionary<string, string> Copyright { get; set; }
        [JsonProperty("author")]
        public Dictionary<string, string> Author { get; set; }

        public string GetDefaultTitle()
        {
            if (Title.ContainsKey(Constants.DefaultLocale))
                return Title[Constants.DefaultLocale];
            return Title.Count > 0 ? Title.First().Value : string.Empty;
        }

        public string GetDefaultAuthor()
        {
            if (Author.ContainsKey(Constants.DefaultLocale))
                return Author[Constants.DefaultLocale];
            return Author.Count > 0 ? Author.First().Value : string.Empty;
        }

        public string GetDefaultCopyright()
        {
            if (Copyright.ContainsKey(Constants.DefaultLocale))
                return Copyright[Constants.DefaultLocale];
            return Copyright.Count > 0 ? Author.First().Value : string.Empty;
        }
    }

    public class SongFlags
    {
        [JsonProperty("rarity")]
        public string Rarity { get; set; }
        [JsonProperty("record_type")]
        public string RecordType { get; set; }
    }
}
