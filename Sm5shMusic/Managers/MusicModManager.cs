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
            _logger.LogInformation("Mod {MusicMod} by '{Author}' - {NbrSongs} song(s)", _musicModConfig.Name, _musicModConfig.Author, _musicModConfig.Songs.Count);

            var index = 0;
            foreach (var song in _musicModConfig.Songs)
            {
                var audioFilePath = GetMusicModAudioFile(song.FileName);
                var audioCuePoints = _audioMetadataService.GetCuePoints(audioFilePath);

                var toneName = song.SongInfo.Id;
                if(!string.IsNullOrEmpty(_musicModConfig.Prefix))
                    toneName = $"{_musicModConfig.Prefix}{index}_{song.SongInfo.Id}";

                _bgmEntries.Add(toneName, new MusicModBgmEntry()
                {
                    NameId = _paracobService.GetNewBgmId(),
                    AudioFilePath = audioFilePath,
                    InternalToneName = toneName,
                    Song = song,
                    CuePoints = song.SongCuePointsOverride ?? MusicModAudioCuePoints.FromAudioCuePoints(audioCuePoints)
                });
                index++;
                _logger.LogInformation("Mod {MusicMod}: Adding song {Song} ({ToneName})", _musicModConfig.Name, song.SongInfo.Id, toneName);
            }

            return true;
        }

        public string GetMusicModAudioFile(string songFileName)
        {
            return Path.Combine(_musicModPath, songFileName);
        }

        private MusicModConfig LoadMusicModConfig()
        {
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
            if(_musicModConfig.Songs == null)
            {
                _logger.LogWarning("MusicModFile {MusicMod} is invalid. Skipping...", _musicModConfig.Name);
                return false;
            }

            //Warning - but no skipping
            if (!string.IsNullOrEmpty(_musicModConfig.Prefix))
            {
                if (_musicModConfig.Prefix.Length > 3)
                {
                    _logger.LogWarning("MusicModFile {MusicMod} - The prefix seems a little long. It shouldn't be longer than 3 characters.", _musicModConfig.Name);
                }

                //Sanitize

                _musicModConfig.Prefix = _musicModConfig.Prefix.ToLower();
                if (!IsLegalId(_musicModConfig.Prefix))
                {
                    _logger.LogWarning("MusicModFile {MusicMod} - The prefix contains invalid characters. Skipping...", _musicModConfig.Name);
                    return false;
                }
            }

            var sanitizedSongs = new List<Song>();
            foreach(var song in _musicModConfig.Songs)
            {
                //Filename test
                if (!File.Exists(Path.Combine(_musicModPath, song.FileName)))
                {
                    _logger.LogWarning("MusicModFile {MusicMod} - Audio file {AudioFile} does not exist. Skipping...", _musicModConfig.Name, song.FileName);
                    continue;
                }

                //Song id
                if (string.IsNullOrEmpty(song.SongInfo?.Id))
                {
                    _logger.LogWarning("MusicModFile {MusicMod} - Audio file {AudioFile} is invalid. It does not have a Song Id. Skipping...", _musicModConfig.Name, song.FileName);
                    continue;
                }

                song.SongInfo.Id = song.SongInfo.Id.ToLower();
                if (!IsLegalId(song.SongInfo.Id))
                {
                    _logger.LogWarning("MusicModFile {MusicMod} - Song {SongId} is invalid. The Song Id contains invalid characters. Skipping...", _musicModConfig.Name, song.SongInfo.Id);
                    continue;
                }

                //Record type
                if (song.SongInfo.RecordType.StartsWith(Constants.InternalIds.RecordTypePrefix))
                    song.SongInfo.RecordType = song.SongInfo.RecordType.Replace(Constants.InternalIds.RecordTypePrefix, string.Empty);
                if (!Constants.ValidRecordTypes.Contains(song.SongInfo.RecordType))
                {
                    _logger.LogWarning("MusicModFile {MusicMod} - Song {SongId} is invalid. The record type is invalid. Please check the list of valid record types. Skipping...", _musicModConfig.Name, song.SongInfo.Id);
                    _logger.LogDebug("MusicModFile {MusicMod} - Valid Record Types: {RecordTypes}", _musicModConfig.Name, string.Join(", ", Constants.ValidRecordTypes));
                    continue;
                }

                //Rarity
                //TODO: Figure out
                if (string.IsNullOrEmpty(song.SongInfo.Rarity))
                    song.SongInfo.Rarity = Constants.InternalIds.RarityDefault;

                //Gametitle ID
                if (string.IsNullOrEmpty(song.GameTitle?.Id))
                {
                    _logger.LogWarning("MusicModFile {MusicMod} - Song {SongId} is invalid. It does not have a Game Title Id. Skipping...", _musicModConfig.Name, song.SongInfo.Id);
                    continue;
                }
                song.GameTitle.Id = song.GameTitle.Id.ToLower();
                if (!IsLegalId(song.GameTitle?.Id))
                {
                    _logger.LogWarning("MusicModFile {MusicMod} - Song {SongId} is invalid. The Game Title Id contains invalid characters. Skipping...", _musicModConfig.Name, song.SongInfo.Id);
                    continue;
                }

                //Series ID
                if (string.IsNullOrEmpty(song.GameTitle?.SeriesId))
                {
                    _logger.LogWarning("MusicModFile {MusicMod} - Song {SongId} is invalid. It does not have a Game Series Id. Skipping...", _musicModConfig.Name, song.SongInfo.Id);
                    continue;
                }
                song.GameTitle.SeriesId = song.GameTitle.SeriesId.ToLower();
                if (song.GameTitle.SeriesId.StartsWith(Constants.InternalIds.GameSeriesIdPrefix))
                    song.GameTitle.SeriesId = song.GameTitle.SeriesId.Replace(Constants.InternalIds.GameSeriesIdPrefix, string.Empty);
                if (!Constants.ValidSeries.Contains(song.GameTitle.SeriesId))
                {
                    _logger.LogWarning("MusicModFile {MusicMod} - Song {SongId} is invalid. The Game Series Id is invalid. Please check the list of valid Game Series Ids. Skipping...", _musicModConfig.Name, song.SongInfo.Id);
                    _logger.LogDebug("MusicModFile {MusicMod} - Valid Game Series Ids: {GameSeriesIds}", _musicModConfig.Name, string.Join(", ", Constants.ValidSeries));
                    continue;
                }

                //Playlists
                if (song.SongInfo.Playlists != null)
                {
                    foreach(var playlist in song.SongInfo.Playlists)
                    {
                        playlist.Id = playlist.Id.ToLower();
                        if (playlist.Id.StartsWith(Constants.InternalIds.PlaylistPrefix))
                        {
                            var newPlaylistId = playlist.Id.Replace(Constants.InternalIds.PlaylistPrefix, string.Empty);
                            _logger.LogDebug("MusicModFile {MusicMod} - Song {SongId}'s playlist {Playlist} was renamed {RenamedPlaylist}", _musicModConfig.Name, song.SongInfo.Id, playlist.Id, newPlaylistId);
                            playlist.Id = newPlaylistId;
                        }
                    }
                }

                sanitizedSongs.Add(song);
            }
            _musicModConfig.Songs = sanitizedSongs;

            //Post song validation warnings
            if (_musicModConfig.Songs.Count == 0)
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
