using CsvHelper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sm5shMusic.Helpers;
using Sm5shMusic.Interfaces;
using Sm5shMusic.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sm5shMusic.Managers
{
    public class MusicModManager : IMusicModManager
    {
        private readonly IAudioMetadataService _audioMetadataService;
        private readonly IParacobService _paracobService;
        private readonly ILogger _logger;

        private readonly Regex _idValidatorRegexp = new Regex(@"^[a-z0-9_\s,]*$");

        private readonly Dictionary<string, MusicModBgmEntry> _bgmEntries;
        private readonly string _musicModPath;
        private readonly MusicModConfig _musicModConfig;

        public Dictionary<string, MusicModBgmEntry> BgmEntries
        {
            get
            {
                return _bgmEntries;
            }
        }

        public MusicModManager(IAudioMetadataService audioMetadataService, IParacobService paracobService, ILogger<IMusicModManager> logger, string musicModPath)
        {
            _musicModPath = musicModPath;
            _audioMetadataService = audioMetadataService;
            _paracobService = paracobService;
            _logger = logger;
            _bgmEntries = new Dictionary<string, MusicModBgmEntry>();
            _musicModConfig = LoadMusicModConfig();
        }

        public bool Init()
        {
            if (_musicModConfig == null || !ValidateAndSanitizeModConfig())
            {
                return false;
            }

            //Process audio mods
            _logger.LogInformation("Mod {MusicMod} by '{Author}' - {NbrSongs} song(s)", _musicModConfig.Name, _musicModConfig.Author, _musicModConfig.Games.Sum(p => p.Songs.Count));

            var index = 0;
            foreach (var game in _musicModConfig.Games)
            {
                foreach (var song in game.Songs)
                {
                    var audioFilePath = GetMusicModAudioFile(song.FileName);
                    var audioCuePoints = _audioMetadataService.GetCuePoints(audioFilePath);

                    var toneName = song.Id;
                    if (!string.IsNullOrEmpty(_musicModConfig.Prefix))
                        toneName = $"{_musicModConfig.Prefix}{index}_{song.Id}";

                    _bgmEntries.Add(toneName, new MusicModBgmEntry()
                    {
                        NameId = _paracobService.GetNewBgmId(),
                        AudioFilePath = audioFilePath,
                        InternalToneName = toneName,
                        Song = song,
                        Game = game,
                        CuePoints = song.SongCuePointsOverride ?? MusicModAudioCuePoints.FromAudioCuePoints(audioCuePoints)
                    });
                    index++;
                    _logger.LogInformation("Mod {MusicMod}: Adding song {Song} ({ToneName})", _musicModConfig.Name, song.Id, toneName);
                }
            }

            return true;
        }

        public string GetMusicModAudioFile(string songFileName)
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
            var metadataJsonFile = Path.Combine(_musicModPath, Constants.MusicModFiles.MusicModMetadataJsonFile);
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
            var metadataCsvFile = Path.Combine(_musicModPath, Constants.MusicModFiles.MusicModMetadataCsvFile);
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
                if (!IsLegalId(_musicModConfig.Prefix))
                {
                    _logger.LogWarning("MusicModFile {MusicMod} - The prefix contains invalid characters. Skipping...", _musicModConfig.Name);
                    return false;
                }
            }

            foreach (var game in _musicModConfig.Games)
            {
                //Gametitle ID
                if (string.IsNullOrEmpty(game.Id))
                {
                    _logger.LogWarning("MusicModFile {MusicMod} - Game {GameId} is invalid. It does not have a Game Title Id. Skipping...", _musicModConfig.Name, game.Id);
                    continue;
                }
                game.Id = game.Id.ToLower();
                if (!IsLegalId(game.Id))
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
                if (game.SeriesId.StartsWith(Constants.InternalIds.GameSeriesIdPrefix))
                    game.SeriesId = game.SeriesId.Replace(Constants.InternalIds.GameSeriesIdPrefix, string.Empty);
                if (!Constants.ValidSeries.Contains(game.SeriesId))
                {
                    _logger.LogWarning("MusicModFile {MusicMod} - Game {GameId} is invalid. The Game Series Id is invalid. Please check the list of valid Game Series Ids. Skipping...", _musicModConfig.Name, game.Id);
                    _logger.LogDebug("MusicModFile {MusicMod} - Valid Game Series Ids: {GameSeriesIds}", _musicModConfig.Name, string.Join(", ", Constants.ValidSeries));
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
                    if (!File.Exists(Path.Combine(_musicModPath, song.FileName)))
                    {
                        _logger.LogWarning("MusicModFile {MusicMod} {Game} - Audio file {AudioFile} does not exist. Skipping...", _musicModConfig.Name, game.Id, song.FileName);
                        continue;
                    }

                    //Filename extensions test
                    if (!Constants.ValidExtensions.Contains(Path.GetExtension(song.FileName).ToLower()))
                    {
                        _logger.LogWarning("MusicModFile {MusicMod} {Game} - Song {SongId} is invalid. The audio file extension is incompatible. Skipping...", _musicModConfig.Name, game.Id, song.Id);
                        _logger.LogDebug("MusicModFile {MusicMod} {Game} - Valid Extensions: {RecordTypes}", _musicModConfig.Name, game.Id, string.Join(", ", Constants.ValidExtensions));
                        continue;
                    }

                    //Song id
                    if (string.IsNullOrEmpty(song?.Id))
                    {
                        _logger.LogWarning("MusicModFile {MusicMod} {Game} - Audio file {AudioFile} is invalid. It does not have a Song Id. Skipping...", _musicModConfig.Name, game.Id, song.FileName);
                        continue;
                    }

                    song.Id = song.Id.ToLower();
                    if (!IsLegalId(song.Id))
                    {
                        _logger.LogWarning("MusicModFile {MusicMod} {Game} - Song {SongId} is invalid. The Song Id contains invalid characters. Skipping...", _musicModConfig.Name, game.Id, song.Id);
                        continue;
                    }

                    //Record type
                    if (song.RecordType.StartsWith(Constants.InternalIds.RecordTypePrefix))
                        song.RecordType = song.RecordType.Replace(Constants.InternalIds.RecordTypePrefix, string.Empty);
                    if (!Constants.ValidRecordTypes.Contains(song.RecordType))
                    {
                        _logger.LogWarning("MusicModFile {MusicMod} {Game} - Song {SongId} is invalid. The record type is invalid. Please check the list of valid record types. Skipping...", _musicModConfig.Name, game.Id, song.Id);
                        _logger.LogDebug("MusicModFile {MusicMod} {Game} - Valid Record Types: {RecordTypes}", _musicModConfig.Name, game.Id, string.Join(", ", Constants.ValidRecordTypes));
                        continue;
                    }

                    //Rarity
                    //TODO: Figure out
                    if (string.IsNullOrEmpty(song.Rarity))
                        song.Rarity = Constants.InternalIds.RarityDefault;

                    //Playlists
                    if (song.Playlists != null)
                    {
                        foreach (var playlist in song.Playlists)
                        {
                            playlist.Id = playlist.Id.ToLower();
                            if (playlist.Id.StartsWith(Constants.InternalIds.PlaylistPrefix))
                            {
                                var newPlaylistId = playlist.Id.Replace(Constants.InternalIds.PlaylistPrefix, string.Empty);
                                _logger.LogDebug("MusicModFile {MusicMod} {Game} - Song {SongId}'s playlist {Playlist} was renamed {RenamedPlaylist}", _musicModConfig.Name, game.Id, song.Id, playlist.Id, newPlaylistId);
                                playlist.Id = newPlaylistId;
                            }
                        }
                    }

                    sanitizedSongs.Add(song);
                }
                game.Songs = sanitizedSongs;

                if (game.Songs == null || game.Songs.Count == 0)
                {
                    _logger.LogWarning("MusicModFile {MusicMod} {Game} is invalid. Skipping...", game.Id, _musicModConfig.Name);
                    continue;
                }
            }

            //Post song validation warnings
            if (_musicModConfig.Games.Sum(p => p.Songs.Count) == 0)
            {
                _logger.LogWarning("MusicModFile {MusicMod} doesn't contain any valid song. Skipping...", _musicModConfig.Name);
                return false;
            }

            return true;
        }

        private bool IsLegalId(string idToCheck)
        {
            return _idValidatorRegexp.IsMatch(idToCheck);
        }
    }
}
