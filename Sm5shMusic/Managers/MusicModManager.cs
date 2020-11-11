using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sm5shMusic.Helpers;
using Sm5shMusic.Interfaces;
using Sm5shMusic.Models;
using System.Collections.Generic;
using System.IO;

namespace Sm5shMusic.Managers
{
    public class MusicModManager : IMusicModManager
    {
        private readonly IAudioMetadataService _audioMetadataService;
        private readonly IParacobService _paracobService;
        private readonly ILogger _logger;

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
            if (_musicModConfig == null)
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
            return Path.Combine(_musicModPath, Constants.MusicModFiles.MusicModMetadataFile);
        }

        public string GetMusicModAudioFile(string songFileName)
        {
            return Path.Combine(_musicModPath, songFileName);
        }
    }
}
