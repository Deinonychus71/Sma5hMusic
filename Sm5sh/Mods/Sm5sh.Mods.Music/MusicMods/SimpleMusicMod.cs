using CsvHelper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sm5sh.Core.Helpers;
using Sm5sh.Mods.Music.Helpers;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using Sm5sh.Mods.Music.MusicMods.SimpleMusicModModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Sm5sh.Mods.Music.MusicMods
{
    public class SimpleMusicMod : IMusicMod
    {
        private readonly IAudioMetadataService _audioMetadataService;
        protected readonly ILogger _logger;

        protected readonly MusicModConfig _musicModConfig;

        public string Name { get { return Mod.Name; } }
        public string Id { get { return Mod.Id; } }
        public string ModPath { get; }
        public MusicModInformation Mod => _musicModConfig;

        public SimpleMusicMod(IAudioMetadataService audioMetadataService, ILogger<IMusicMod> logger, string musicModPath)
        {
            ModPath = musicModPath;
            _audioMetadataService = audioMetadataService;
            _logger = logger;
            _musicModConfig = LoadMusicModConfig();
        }

        public List<BgmEntry> GetBgms()
        {
            if (_musicModConfig == null || !ValidateAndSanitizeModConfig())
            {
                return new List<BgmEntry>();
            }

            //Process audio mods
            _logger.LogInformation("Mod {MusicMod} by '{Author}' - {NbrSongs} song(s)", _musicModConfig.Name, _musicModConfig.Author, _musicModConfig.Games.Sum(p => p.Songs.Count));

            var output = new List<BgmEntry>();
            foreach (var game in _musicModConfig.Games)
            {
                var gameEntry = new GameTitleEntry(game.Id)
                {
                    UiSeriesId = game.SeriesId,
                    NameId = game.Id.TrimStart(Constants.InternalIds.GAME_TITLE_ID_PREFIX),
                    MSBTTitle = game.Title
                };

                foreach (var song in game.Songs)
                {
                    var audioFilePath = GetMusicModAudioFile(song.FileName);
                    var audioCuePoints = _audioMetadataService.GetCuePoints(audioFilePath);

                    var toneId = song.Id;
                    var hasDlcPlaylistId = song.Playlists != null && song.Playlists.Any(p => CoreConstants.DLC_STAGES.Contains(p.Id));
                    var newSong = new BgmEntry(toneId, this);
                    newSong.GameTitle = gameEntry;
                    newSong.DbRoot.RecordType = song.RecordType;
                    newSong.DbRoot.TestDispOrder = -1;
                    newSong.DbRoot.IsDlc = hasDlcPlaylistId;
                    newSong.DbRoot.IsPatch = hasDlcPlaylistId;
                    newSong.StreamSet.SpecialCategory = song.SpecialCategory?.Category;
                    newSong.StreamSet.Info1 = song.SpecialCategory?.Parameters?[0];
                    newSong.MSBTLabels.Title = song.Title;
                    newSong.MSBTLabels.Author = song.Author;
                    newSong.MSBTLabels.Copyright = song.Copyright;
                    newSong.Filename = audioFilePath;
                    newSong.NUS3BankConfig.AudioVolume = 0;
                    newSong.BgmProperties.LoopEndMs = audioCuePoints.LoopEndMs;
                    newSong.BgmProperties.LoopEndSample = audioCuePoints.LoopEndSample;
                    newSong.BgmProperties.LoopStartMs = audioCuePoints.LoopStartMs;
                    newSong.BgmProperties.LoopStartSample = audioCuePoints.LoopStartSample;
                    newSong.BgmProperties.TotalSamples = audioCuePoints.TotalSamples;
                    newSong.BgmProperties.TotalTimeMs = audioCuePoints.TotalTimeMs;
                    if (song.Playlists != null)
                    {
                        foreach (var playlist in song.Playlists)
                        {
                            if (!newSong.Playlists.ContainsKey(playlist.Id))
                                newSong.Playlists.Add(playlist.Id, new List<Models.BgmEntryModels.BgmPlaylistEntry>());
                            newSong.Playlists[playlist.Id].Add(new Models.BgmEntryModels.BgmPlaylistEntry(newSong));
                        }
                    }
                    output.Add(newSong);

                    _logger.LogInformation("Mod {MusicMod}: Adding song {Song} ({ToneName})", _musicModConfig.Name, song.Id, toneId);
                }
            }

            return output;
        }

        public BgmEntry AddBgm(string filename)
        {
            throw new NotImplementedException();
        }

        public bool UpdateBgm(BgmEntry bgmEntry)
        {
            throw new NotImplementedException();
        }

        public bool RemoveBgm(string toneId)
        {
            throw new NotImplementedException();
        }

        private string GetMusicModAudioFile(string songFileName)
        {
            return Path.Combine(ModPath, songFileName);
        }

        public void UpdateModInformation(MusicModInformation configBase)
        {
            throw new NotImplementedException();
        }

        protected virtual MusicModConfig LoadMusicModConfig()
        {
            //Attempt JSON
            var metadataJsonFile = Path.Combine(ModPath, Constants.MusicModFiles.MUSIC_MOD_METADATA_JSON_FILE);
            if (File.Exists(metadataJsonFile))
            {
                var file = File.ReadAllText(metadataJsonFile);
                _logger.LogDebug("Parsing {MusicModFile} Json File", metadataJsonFile);
                var output = JsonConvert.DeserializeObject<MusicModConfig>(file);
                _logger.LogDebug("Parsed {MusicModFile} Json File", metadataJsonFile);
                return output;
            }
            else
            {
                //Cannot load music mod
                _logger.LogError("MusicModFile {MusicModFile} does not exist! Attempt to retrieve CSV.", ModPath);
            }

            return null;
        }

        protected virtual bool SaveMusicModConfig()
        {
            //Check if disabled
            if (Path.GetFileName(ModPath).StartsWith("."))
            {
                _logger.LogDebug("{MusicModFile} is disabled.");
                return false;
            }

            var metadataJsonFile = Path.Combine(ModPath, Constants.MusicModFiles.MUSIC_MOD_METADATA_JSON_FILE);
            File.WriteAllText(metadataJsonFile, JsonConvert.SerializeObject(_musicModConfig));

            return true;
        }

        private bool ValidateAndSanitizeModConfig()
        {
            //Validate
            if(_musicModConfig.Games == null || _musicModConfig.Games.Count == 0)
            {
                _logger.LogWarning("MusicModFile {MusicMod} is invalid. Skipping...", _musicModConfig.Name);
                return false;
            }

            var sanitizedGames = new List<Game>();
            foreach (var game in _musicModConfig.Games)
            {
                //Gametitle ID
                if (string.IsNullOrEmpty(game.Id))
                {
                    _logger.LogWarning("MusicModFile {MusicMod} - Game {GameId} is invalid. It does not have a Game Title Id. Skipping...", _musicModConfig.Name, game.Id);
                    continue;
                }
                game.Id = game.Id.ToLower();
                if (!game.Id.StartsWith(Constants.InternalIds.GAME_TITLE_ID_PREFIX))
                    game.Id = $"{Constants.InternalIds.GAME_TITLE_ID_PREFIX}{game.Id}";
                if (!IdValidationHelper.IsLegalId(game.Id))
                {
                    _logger.LogWarning("MusicModFile {MusicMod} - Game {GameId} is invalid. The Game Title Id contains invalid characters. Skipping...", _musicModConfig.Name, game.Id);
                    continue;
                }

                //Series ID
                if (string.IsNullOrEmpty(game.SeriesId))
                {
                    _logger.LogWarning("MusicModFile {MusicMod} - Game {GameId} is invalid. It does not have a Game Series Id. Skipping...", _musicModConfig.Name, game.Id);
                    continue;
                }

                game.SeriesId = game.SeriesId.ToLower();
                if (!game.SeriesId.StartsWith(Constants.InternalIds.GAME_SERIES_ID_PREFIX))
                    game.SeriesId = $"{Constants.InternalIds.GAME_SERIES_ID_PREFIX}{game.SeriesId}";
                if (!CoreConstants.VALID_SERIES.Contains(game.SeriesId))
                {
                    _logger.LogWarning("MusicModFile {MusicMod} - Game {GameId} is invalid. The Game Series Id is invalid. Please check the list of valid Game Series Ids. Skipping...", _musicModConfig.Name, game.Id);
                    _logger.LogDebug("MusicModFile {MusicMod} - Valid Game Series Ids: {GameSeriesIds}", _musicModConfig.Name, string.Join(", ", CoreConstants.VALID_SERIES));
                    continue;
                }

                if (game.Songs == null || game.Songs.Count == 0)
                {
                    _logger.LogWarning("MusicModFile {MusicMod} {Game} is invalid. Skipping...", _musicModConfig.Name, game.Id);
                    continue;
                }

                var sanitizedSongs = new List<Song>();
                foreach (var song in game.Songs)
                {
                    //Filename test
                    if (string.IsNullOrEmpty(song.FileName))
                    {
                        _logger.LogWarning("MusicModFile {MusicMod} {Game} - The song {SongId} doesn't have a filename. Skipping...", _musicModConfig.Name, game.Id, song.Id);
                        continue;
                    }

                    if (!File.Exists(Path.Combine(ModPath, song.FileName)))
                    {
                        _logger.LogWarning("MusicModFile {MusicMod} {Game} - Audio file {AudioFile} does not exist. Skipping...", _musicModConfig.Name, game.Id, song.FileName);
                        continue;
                    }

                    //Filename extensions test
                    if (!Constants.VALID_MUSIC_EXTENSIONS.Contains(Path.GetExtension(song.FileName).ToLower()))
                    {
                        _logger.LogWarning("MusicModFile {MusicMod} {Game} - Song {SongId} is invalid. The audio file extension is incompatible. Skipping...", _musicModConfig.Name, game.Id, song.Id);
                        _logger.LogDebug("MusicModFile {MusicMod} {Game} - Valid Extensions: {RecordTypes}", _musicModConfig.Name, game.Id, string.Join(", ", Constants.VALID_MUSIC_EXTENSIONS));
                        continue;
                    }

                    //Song id
                    if (string.IsNullOrEmpty(song?.Id))
                    {
                        _logger.LogWarning("MusicModFile {MusicMod} {Game} - Audio file {AudioFile} is invalid. It does not have a Song Id. Skipping...", _musicModConfig.Name, game.Id, song.FileName);
                        continue;
                    }

                    song.Id = song.Id.ToLower();
                    if (!IdValidationHelper.IsLegalId(song.Id))
                    {
                        _logger.LogWarning("MusicModFile {MusicMod} {Game} - Song {SongId} is invalid. The Song Id contains invalid characters. Skipping...", _musicModConfig.Name, game.Id, song.Id);
                        continue;
                    }

                    //Record type
                    if (!song.RecordType.StartsWith(Constants.InternalIds.RECORD_TYPE_PREFIX))
                        song.RecordType = $"{Constants.InternalIds.RECORD_TYPE_PREFIX}{song.RecordType}";
                    if (!Constants.VALID_RECORD_TYPES.Contains(song.RecordType))
                    {
                        _logger.LogWarning("MusicModFile {MusicMod} {Game} - Song {SongId} is invalid. The record type is invalid. Please check the list of valid record types. Skipping...", _musicModConfig.Name, game.Id, song.Id);
                        _logger.LogDebug("MusicModFile {MusicMod} {Game} - Valid Record Types: {RecordTypes}", _musicModConfig.Name, game.Id, string.Join(", ", Constants.VALID_RECORD_TYPES));
                        continue;
                    }

                    //Special cat info fix
                    if (song.SpecialCategory != null && song.SpecialCategory.Parameters != null)
                    {
                        for(int i = 0; i < song.SpecialCategory.Parameters.Count; i++)
                        {
                            var param = song.SpecialCategory.Parameters[i];
                            if (!param.StartsWith(Constants.InternalIds.INFO_ID_PREFIX))
                                song.SpecialCategory.Parameters[i] = $"{Constants.InternalIds.INFO_ID_PREFIX}{param}";
                        }
                    }

                    //Playlists
                    if (song.Playlists != null)
                    {
                        foreach (var playlist in song.Playlists)
                        {
                            playlist.Id = playlist.Id.ToLower();
                            if (!playlist.Id.StartsWith(Constants.InternalIds.PLAYLIST_PREFIX))
                            {
                                var newPlaylistId = $"{Constants.InternalIds.PLAYLIST_PREFIX}{playlist.Id}";
                                _logger.LogDebug("MusicModFile {MusicMod} {Game} - Song {SongId}'s playlist {Playlist} was renamed {RenamedPlaylist}", _musicModConfig.Name, game.Id, song.Id, playlist.Id, newPlaylistId);
                                playlist.Id = newPlaylistId;
                            }
                        }
                    }

                    sanitizedSongs.Add(song);
                }

                game.Songs = sanitizedSongs;
                sanitizedGames.Add(game);

                if (game.Songs == null || game.Songs.Count == 0)
                {
                    _logger.LogWarning("MusicModFile {MusicMod} {Game} is invalid. Skipping...", game.Id, _musicModConfig.Name);
                    continue;
                }
            }

            _musicModConfig.Games = sanitizedGames;

            //Post song validation warnings
            if (_musicModConfig.Games.Sum(p => p.Songs.Count) == 0)
            {
                _logger.LogWarning("MusicModFile {MusicMod} doesn't contain any valid song. Skipping...", _musicModConfig.Name);
                return false;
            }

            return true;
        }
    }

    namespace SimpleMusicModModels
    {
        public class MusicModConfig : MusicModInformation
        {
            [JsonProperty("id")]
            public override string Id { get { return GetIdFromTitle(); } }
            [JsonProperty("version")]
            public override int Version { get { return 1; } }
            [JsonProperty("games")]
            public List<Game> Games { get; set; }

            private string GetIdFromTitle()
            {
                if (Name == null)
                    return null;
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(Name));
                    var sBuilder = new StringBuilder();
                    for (int i = 0; i < data.Length; i++)
                    {
                        sBuilder.Append(data[i].ToString("x2"));
                    }
                    return sBuilder.ToString();
                }
            }
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
            [JsonProperty("hidden_in_soundtest")]
            public bool HiddenInSoundTest { get; set; }
        }

        public class Game
        {
            [JsonProperty("id")]
            public string Id { get; set; }
            [JsonProperty("title")]
            public Dictionary<string, string> Title { get; set; }
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
            public string Category { get; set; }

            [JsonProperty("parameters")]
            public List<string> Parameters { get; set; }
        }
    }
}
