using CsvHelper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sm5sh.Core.Helpers;
using Sm5sh.Mods.Music.Helpers;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sm5sh.Mods.Music
{
    public class MusicModManager : IMusicModManager
    {
        private readonly IAudioMetadataService _audioMetadataService;
        private readonly ILogger _logger;

        private readonly string _musicModPath;
        private readonly MusicModConfig _musicModConfig;


        public MusicModManager(IAudioMetadataService audioMetadataService, ILogger<IMusicModManager> logger, string musicModPath)
        {
            _musicModPath = musicModPath;
            _audioMetadataService = audioMetadataService;
            _logger = logger;
            _musicModConfig = LoadMusicModConfig();
        }

        public List<BgmEntry> LoadBgmEntries()
        {
            if (_musicModConfig == null || !ValidateAndSanitizeModConfig())
            {
                return new List<BgmEntry>();
            }

            //Process audio mods
            _logger.LogInformation("Mod {MusicMod} by '{Author}' - {NbrSongs} song(s)", _musicModConfig.Name, _musicModConfig.Author, _musicModConfig.Games.Sum(p => p.Songs.Count));

            var index = 0;
            var output = new List<BgmEntry>();
            foreach (var game in _musicModConfig.Games)
            {
                foreach (var song in game.Songs)
                {
                    var audioFilePath = GetMusicModAudioFile(song.FileName);
                    var audioCuePoints = _audioMetadataService.GetCuePoints(audioFilePath);

                    var toneId = song.Id;
                    if (!string.IsNullOrEmpty(_musicModConfig.Prefix))
                        toneId = $"{_musicModConfig.Prefix}{index}_{song.Id}";

                    var hasDlcPlaylistId = song.Playlists.Any(p => CoreConstants.DLC_STAGES.Contains(p.Id));
                    output.Add(new BgmEntry()
                    {
                        ToneId = toneId,
                        AudioVolume = 0, //TODO
                        RecordType = song.RecordType,
                        Title = song.Title,
                        Author = song.Author,
                        Copyright = song.Copyright,
                        GameTitle = new GameTitleEntry()
                        {
                            GameTitleId = game.Id,
                            SeriesId = game.SeriesId,
                            NameId = game.Id.Replace(Constants.InternalIds.GAME_TITLE_ID_PREFIX, string.Empty),
                            Title = game.Title
                        },
                        IsDlc = hasDlcPlaylistId,
                        IsPatch = hasDlcPlaylistId,
                        Playlists = song.Playlists.Select(p => new Models.BgmEntryModels.PlaylistEntry() {  Id = p.Id}).ToList(),
                        FileName = audioFilePath,
                        AudioCuePoints = audioCuePoints
                    });
                    index++;
                    _logger.LogInformation("Mod {MusicMod}: Adding song {Song} ({ToneName})", _musicModConfig.Name, song.Id, toneId);
                }
            }

            return output;
        }

        private string GetMusicModAudioFile(string songFileName)
        {
            return Path.Combine(_musicModPath, songFileName);
        }

        private MusicModConfig LoadMusicModConfig()
        {
            //Check if disabled
            if (Path.GetFileName(_musicModPath).StartsWith("."))
            {
                _logger.LogDebug("{MusicModFile} is disabled.");
                return null;
            }

            //Attempt JSON
            var metadataJsonFile = Path.Combine(_musicModPath, Constants.MusicModFiles.MUSIC_MOD_METADATA_JSON_FILE);
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
                _logger.LogInformation("MusicModFile {MusicModFile} does not exist! Attempt to retrieve CSV.", _musicModPath);
            }

            //Attempt CSV
            var metadataCsvFile = Path.Combine(_musicModPath, Constants.MusicModFiles.MUSIC_MOD_METADATA_CSV_FILE);
            if (File.Exists(metadataCsvFile))
            {
                _logger.LogDebug("Parsing {MusicModFile} CSV File", metadataCsvFile);
                using (var reader = new StreamReader(metadataCsvFile))
                {
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csv.Configuration.PrepareHeaderForMatch = (header, index) => Regex.Replace(header, @"\s", string.Empty);
                        csv.Configuration.MissingFieldFound = null;
                        csv.Configuration.HeaderValidated = null;
                        List<MusicModCSVConfig> records = null;
                        try
                        {
                            records = csv.GetRecords<MusicModCSVConfig>().ToList();
                        }
                        catch(Exception e)
                        {
                            _logger.LogError("MusicModFile {MusicModFile} exists, but an error occured during parsing.", metadataCsvFile);
                            _logger.LogDebug("MusicModFile {MusicModFile} exists, but an error occured during parsing. {Exception} - {Stacktrace}", metadataCsvFile, e.Message, e.StackTrace);
                            return null;
                        }
                        _logger.LogDebug("Parsed {MusicModFile} CSV File", metadataCsvFile);
                        var output =  CSVModMapper.FromCSV(_musicModPath, records);
                        _logger.LogDebug("Mapped {MusicModFile} CSV File to MusicModConfig model", metadataCsvFile);
                        return output;
                    }
                }
            }
            else
            {
                //Cannot load music mod
                _logger.LogError("MusicModFile {MusicModFile} does not exist!", _musicModPath);
            }

            return null;
        }

        private bool ValidateAndSanitizeModConfig()
        {
            //Validate
            if(_musicModConfig.Games == null || _musicModConfig.Games.Count == 0)
            {
                _logger.LogWarning("MusicModFile {MusicMod} is invalid. Skipping...", _musicModConfig.Name);
                return false;
            }

            //Warning - but no skipping
            if (!string.IsNullOrEmpty(_musicModConfig.Prefix))
            {
                if (_musicModConfig.Prefix.Length > 3)
                {
                    _logger.LogWarning("MusicModFile {MusicMod} {Game} - The prefix seems a little long. It shouldn't be longer than 3 characters.", _musicModConfig.Name);
                }

                //Sanitize
                _musicModConfig.Prefix = _musicModConfig.Prefix.ToLower();
                if (!IdValidationHelper.IsLegalId(_musicModConfig.Prefix))
                {
                    _logger.LogWarning("MusicModFile {MusicMod} - The prefix contains invalid characters. Skipping...", _musicModConfig.Name);
                    return false;
                }
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

                    if (!File.Exists(Path.Combine(_musicModPath, song.FileName)))
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
}
