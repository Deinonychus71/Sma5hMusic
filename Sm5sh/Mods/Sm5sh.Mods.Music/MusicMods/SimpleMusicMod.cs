using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sm5sh.Helpers;
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
using System.Threading.Tasks;

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

        public MusicModEntries GetMusicModEntries()
        {
            var output = new MusicModEntries();

            if (_musicModConfig == null || !ValidateAndSanitizeModConfig())
            {
                return output;
            }

            //Process audio mods
            _logger.LogInformation("Mod {MusicMod} by '{Author}' - {NbrSongs} song(s)", _musicModConfig.Name, _musicModConfig.Author, _musicModConfig.Games.Sum(p => p.Songs.Count));

            foreach (var game in _musicModConfig.Games)
            {
                var gameTitleEntry = new GameTitleEntry(game.Id)
                {
                    UiSeriesId = game.SeriesId,
                    NameId = game.Id.TrimStart(MusicConstants.InternalIds.GAME_TITLE_ID_PREFIX),
                    MSBTTitle = game.Title
                };

                foreach (var song in game.Songs)
                {
                    var audioFilePath = GetMusicModAudioFile(song.FileName);
                    var audioCuePoints = _audioMetadataService.GetCuePoints(audioFilePath).GetAwaiter().GetResult();
                    if (audioCuePoints == null || audioCuePoints.TotalSamples <= 0)
                    {
                        _logger.LogError("The filename {Filename} didn't have cue points. Make sure audio library is properly installed.", audioFilePath);
                        continue;
                    }

                    var toneId = song.Id;
                    var hasDlcPlaylistId = song.Playlists != null && song.Playlists.Any(p => CoreConstants.DLC_STAGES.Contains(p.Id));

                    var bgmDbRootEntry = new BgmDbRootEntry($"{MusicConstants.InternalIds.UI_BGM_ID_PREFIX}{toneId}", this)
                    {
                        StreamSetId = $"{MusicConstants.InternalIds.STREAM_SET_PREFIX}{toneId}",
                        RecordType = song.RecordType,
                        TestDispOrder = -1,
                        IsDlc = hasDlcPlaylistId,
                        IsPatch = hasDlcPlaylistId,
                        UiGameTitleId = gameTitleEntry.UiGameTitleId,
                        Title = song.Title,
                        Author = song.Author,
                        Copyright = song.Copyright
                    };

                    string specialCategory = "";
                    string info1 = "";
                    if (song.SpecialCategory?.Category == "sf_pinch")
                    {
                        specialCategory = "0x105274ba4f";
                        info1 = song.SpecialCategory?.Parameters?[0];
                    }

                    var bgmStreamSetEntry = new BgmStreamSetEntry($"{MusicConstants.InternalIds.STREAM_SET_PREFIX}{toneId}", this)
                    {
                        SpecialCategory = specialCategory,
                        Info0 = $"{MusicConstants.InternalIds.INFO_ID_PREFIX}{toneId}",
                        Info1 = info1
                    };

                    var bgmAssignedInfoEntry = new BgmAssignedInfoEntry($"{MusicConstants.InternalIds.INFO_ID_PREFIX}{toneId}", this)
                    {
                        StreamId = $"{MusicConstants.InternalIds.STREAM_PREFIX}{toneId}",
                    };

                    var bgmStreamPropertyEntry = new BgmStreamPropertyEntry($"{MusicConstants.InternalIds.STREAM_PREFIX}{toneId}", this)
                    {
                        DataName0 = toneId
                    };

                    var bgmPropertyEntry = new BgmPropertyEntry(toneId, audioFilePath, this)
                    {
                        LoopEndMs = audioCuePoints.LoopEndMs,
                        LoopEndSample = audioCuePoints.LoopEndSample,
                        LoopStartMs = audioCuePoints.LoopStartMs,
                        LoopStartSample = audioCuePoints.LoopStartSample,
                        TotalSamples = audioCuePoints.TotalSamples,
                        TotalTimeMs = audioCuePoints.TotalTimeMs
                    };

                    output.GameTitleEntries.Add(gameTitleEntry);
                    output.BgmDbRootEntries.Add(bgmDbRootEntry);
                    output.BgmStreamSetEntries.Add(bgmStreamSetEntry);
                    output.BgmAssignedInfoEntries.Add(bgmAssignedInfoEntry);
                    output.BgmStreamPropertyEntries.Add(bgmStreamPropertyEntry);
                    output.BgmPropertyEntries.Add(bgmPropertyEntry);

                    _logger.LogInformation("Mod {MusicMod}: Adding song {Song} ({ToneName})", _musicModConfig.Name, song.Id, toneId);
                }
            }

            return output;
        }

        private string GetMusicModAudioFile(string songFileName)
        {
            return Path.Combine(ModPath, songFileName);
        }

        public bool UpdateModInformation(MusicModInformation configBase)
        {
            throw new NotImplementedException();
        }

        protected virtual MusicModConfig LoadMusicModConfig()
        {
            //Attempt JSON
            var metadataJsonFile = Path.Combine(ModPath, MusicConstants.MusicModFiles.MUSIC_MOD_METADATA_JSON_FILE);
            if (File.Exists(metadataJsonFile))
            {
                var file = File.ReadAllText(metadataJsonFile);
                _logger.LogDebug("Parsing {MusicModFile} Json File", metadataJsonFile);
                var output = JsonConvert.DeserializeObject<MusicModConfig>(file);
                _logger.LogDebug("Parsed {MusicModFile} Json File", metadataJsonFile);

                //File backup, as it's an old version of mod
                if (!File.Exists($"{metadataJsonFile}.bak"))
                    File.Copy(metadataJsonFile, $"{metadataJsonFile}.bak");

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

            var metadataJsonFile = Path.Combine(ModPath, MusicConstants.MusicModFiles.MUSIC_MOD_METADATA_JSON_FILE);
            File.WriteAllText(metadataJsonFile, JsonConvert.SerializeObject(_musicModConfig));

            return true;
        }

        private bool ValidateAndSanitizeModConfig()
        {
            //Validate
            if (_musicModConfig.Games == null || _musicModConfig.Games.Count == 0)
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
                if (!game.Id.StartsWith(MusicConstants.InternalIds.GAME_TITLE_ID_PREFIX))
                    game.Id = $"{MusicConstants.InternalIds.GAME_TITLE_ID_PREFIX}{game.Id}";
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
                if (!game.SeriesId.StartsWith(MusicConstants.InternalIds.GAME_SERIES_ID_PREFIX))
                    game.SeriesId = $"{MusicConstants.InternalIds.GAME_SERIES_ID_PREFIX}{game.SeriesId}";
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
                    if (!MusicConstants.VALID_MUSIC_EXTENSIONS.Contains(Path.GetExtension(song.FileName).ToLower()))
                    {
                        _logger.LogWarning("MusicModFile {MusicMod} {Game} - Song {SongId} is invalid. The audio file extension is incompatible. Skipping...", _musicModConfig.Name, game.Id, song.Id);
                        _logger.LogDebug("MusicModFile {MusicMod} {Game} - Valid Extensions: {RecordTypes}", _musicModConfig.Name, game.Id, string.Join(", ", MusicConstants.VALID_MUSIC_EXTENSIONS));
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
                    if (!song.RecordType.StartsWith(MusicConstants.InternalIds.RECORD_TYPE_PREFIX))
                        song.RecordType = $"{MusicConstants.InternalIds.RECORD_TYPE_PREFIX}{song.RecordType}";
                    if (!MusicConstants.VALID_RECORD_TYPES.Contains(song.RecordType))
                    {
                        _logger.LogWarning("MusicModFile {MusicMod} {Game} - Song {SongId} is invalid. The record type is invalid. Please check the list of valid record types. Skipping...", _musicModConfig.Name, game.Id, song.Id);
                        _logger.LogDebug("MusicModFile {MusicMod} {Game} - Valid Record Types: {RecordTypes}", _musicModConfig.Name, game.Id, string.Join(", ", MusicConstants.VALID_RECORD_TYPES));
                        continue;
                    }

                    //Special cat info fix
                    if (song.SpecialCategory != null && song.SpecialCategory.Parameters != null)
                    {
                        for (int i = 0; i < song.SpecialCategory.Parameters.Count; i++)
                        {
                            var param = song.SpecialCategory.Parameters[i];
                            if (!param.StartsWith(MusicConstants.InternalIds.INFO_ID_PREFIX))
                                song.SpecialCategory.Parameters[i] = $"{MusicConstants.InternalIds.INFO_ID_PREFIX}{param}";
                        }
                    }

                    //Playlists
                    if (song.Playlists != null)
                    {
                        foreach (var playlist in song.Playlists)
                        {
                            playlist.Id = playlist.Id.ToLower();
                            if (!playlist.Id.StartsWith(MusicConstants.InternalIds.PLAYLIST_PREFIX))
                            {
                                var newPlaylistId = $"{MusicConstants.InternalIds.PLAYLIST_PREFIX}{playlist.Id}";
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

        public Task<bool> AddOrUpdateMusicModEntries(MusicModEntries musicModEntries)
        {
            throw new NotImplementedException();
        }

        public bool RemoveMusicModEntries(MusicModDeleteEntries musicModDeleteEntries)
        {
            throw new NotImplementedException();
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
