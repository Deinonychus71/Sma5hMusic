using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sma5h.Mods.Music.Helpers;
using Sma5h.Mods.Music.Interfaces;
using Sma5h.Mods.Music.Models;
using Sma5h.Mods.Music.MusicMods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Sma5h.Mods.Music.Services
{
    public class MusicModManagerService : IMusicModManagerService
    {
        private readonly IOptionsMonitor<Sma5hMusicOptions> _config;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<IMusicMod> _musicMods;

        public IEnumerable<IMusicMod> MusicMods => _musicMods;

        public MusicModManagerService(IServiceProvider serviceProvider, IOptionsMonitor<Sma5hMusicOptions> config, ILogger<IMusicModManagerService> logger)
        {
            _config = config;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _musicMods = new List<IMusicMod>();
        }

        public IEnumerable<IMusicMod> RefreshMusicMods()
        {
            _musicMods.Clear();

            Directory.CreateDirectory(_config.CurrentValue.Sma5hMusic.ModPath);
            foreach (var musicModPath in Directory.GetDirectories(_config.CurrentValue.Sma5hMusic.ModPath, "*", SearchOption.TopDirectoryOnly))
            {
                //Check if disabled
                if (Path.GetFileName(musicModPath).StartsWith("."))
                {
                    _logger.LogDebug("{MusicModFile} is disabled.");
                    continue;
                }

                //Load Music Mod Manager
                var newMusicMod = GetMusicModManager(musicModPath);
                if (newMusicMod == null)
                {
                    _logger.LogWarning("Could not load the music mod {MusicModPath}.", musicModPath);
                    continue;
                }

                _musicMods.Add(newMusicMod);
            }

            return MusicMods;
        }

        public IMusicMod AddMusicMod(MusicModInformation configBase, string modPath)
        {
            var basePath = _config.CurrentValue.Sma5hMusic.ModPath;
            if (!modPath.StartsWith(basePath))
                modPath = Path.Combine(basePath, modPath);

            var newManagerMod = ActivatorUtilities.CreateInstance<MusicMod>(_serviceProvider, modPath, configBase);
            _musicMods.Add(newManagerMod);
            return newManagerMod;
        }


        private IMusicMod GetMusicModManager(string musicModFolder)
        {
            IMusicMod musicMod = null;
            var jsonBaseFilename = Path.Combine(musicModFolder, MusicConstants.MusicModFiles.MUSIC_MOD_METADATA_JSON_FILE);
            if (File.Exists(jsonBaseFilename))
            {
                var modBase = LoadJsonBaseMod(jsonBaseFilename);
                if (modBase.Version == 2 || modBase.Version == 3 || modBase.Version == 4)
                {
                    musicMod = ActivatorUtilities.CreateInstance<MusicMod>(_serviceProvider, musicModFolder);
                }
            }

            if (musicMod?.Mod == null)
                return null;

            return musicMod;
        }

        private MusicModInformation LoadJsonBaseMod(string filename)
        {
            try
            {
                return JsonConvert.DeserializeObject<MusicModInformation>(File.ReadAllText(filename));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not read file {JsonFile}", filename);
                return null;
            }
        }

        public async Task<bool> UpdateGameEntry(GameTitleEntry gameTitleEntry)
        {
            if (_musicMods != null)
            {
                foreach (var musicMod in _musicMods)
                {
                    var newMusicModEntries = new MusicModEntries();
                    newMusicModEntries.GameTitleEntries.Add(gameTitleEntry);
                    if (!await musicMod.AddOrUpdateMusicModEntries(newMusicModEntries))
                        return false;
                }
            }
            return true;
        }
    }
}
