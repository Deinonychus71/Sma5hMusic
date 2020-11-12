using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sm5shMusic.Helpers;
using Sm5shMusic.Interfaces;
using Sm5shMusic.Models;
using System.Collections.Generic;
using System.IO;
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

            //Load Mod
            var metadataFile = GetMusicModMetadataFile();
            if (File.Exists(metadataFile))
            {
                var file = File.ReadAllText(metadataFile);
                _musicModConfig = JsonConvert.DeserializeObject<MusicModConfig>(file);
            }
            else
            {
                //Cannot load music mod
                _logger.LogError("MusicModFile {MusicModFile} does not exist!", metadataFile);
            }
        }

        public bool Init()
        {
            if (_musicModConfig == null || !ValidateAndSanitizeModConfig())
            {
                return false;
            }

            //Process audio mods
            _logger.LogInformation("Mod '{MusicMod}' by {Author} - {NbrSongs} song(s)", _musicModConfig.Name, _musicModConfig.Author, _musicModConfig.Songs.Count);

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
                _logger.LogInformation("Mod '{MusicMod}': Adding song {Song} ({ToneName})", _musicModConfig.Name, song.SongInfo.Id, toneName);
            }

            return true;
        }

        public string GetMusicModMetadataFile()
        {
            return Path.Combine(_musicModPath, Constants.MusicModFiles.MusicModMetadataJsonFile);
        }

        public string GetMusicModAudioFile(string songFileName)
        {
            return Path.Combine(_musicModPath, songFileName);
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
                if(song.SongInfo.RecordType != "record_original" && song.SongInfo.RecordType != "record_new_arrange" && song.SongInfo.RecordType != "record_arrange")
                {
                    _logger.LogWarning("MusicModFile {MusicMod} - Song {SongId} is invalid. The record type is invalid. It should be either 'record_original', 'record_arrange' or 'record_new_arrange'. Skipping...", _musicModConfig.Name, song.SongInfo.Id);
                    continue;
                }

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

                //Playlists
                if(song.SongInfo.Playlists != null)
                {
                    foreach(var playlist in song.SongInfo.Playlists)
                    {
                        playlist.Id = playlist.Id.ToLower();
                        if (!playlist.Id.StartsWith("bgm"))
                        {
                            _logger.LogWarning("MusicModFile {MusicMod} - Song {SongId}'s playlist {Playlist} is invalid. It should start with 'bgm'.", _musicModConfig.Name, song.SongInfo.Id, playlist.Id);
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
