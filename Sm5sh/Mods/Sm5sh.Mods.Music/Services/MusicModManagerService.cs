using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sm5sh.Mods.Music.Helpers;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using Sm5sh.Mods.Music.MusicMods;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sm5sh.Mods.Music.Services
{
    public class MusicModManagerService : IMusicModManagerService
    {
        private readonly IOptions<Sm5shMusicOptions> _config;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<IMusicMod> _musicMods;

        public IEnumerable<IMusicMod> MusicMods => _musicMods;

        public MusicModManagerService(IServiceProvider serviceProvider, IOptions<Sm5shMusicOptions> config, ILogger<IMusicModManagerService> logger)
        {
            _config = config;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _musicMods = new List<IMusicMod>();
        }

        public IEnumerable<IMusicMod> RefreshMusicMods()
        {
            _musicMods.Clear();

            Directory.CreateDirectory(_config.Value.Sm5shMusic.ModPath);
            foreach (var musicModPath in Directory.GetDirectories(_config.Value.Sm5shMusic.ModPath, "*", SearchOption.TopDirectoryOnly))
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
            var basePath = _config.Value.Sm5shMusic.ModPath;
            if (!modPath.StartsWith(basePath))
                modPath = Path.Combine(basePath, modPath);

            var newManagerMod = ActivatorUtilities.CreateInstance<AdvancedMusicMod>(_serviceProvider, modPath, configBase);
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
                if (modBase.Version == 2)
                {
                    musicMod = ActivatorUtilities.CreateInstance<AdvancedMusicMod>(_serviceProvider, musicModFolder);
                }
                else
                {
                    musicMod = ActivatorUtilities.CreateInstance<SimpleMusicMod>(_serviceProvider, musicModFolder);
                }
            }
            else if (File.Exists(Path.Combine(musicModFolder, MusicConstants.MusicModFiles.MUSIC_MOD_METADATA_CSV_FILE)))
            {
                musicMod = ActivatorUtilities.CreateInstance<SimpleCSVMusicMod>(_serviceProvider, musicModFolder);
            }

            if (musicMod?.Mod == null)
                return null;

            if (musicMod.Mod.Version != 2)
            {
                musicMod = ActivatorUtilities.CreateInstance<AdvancedMusicMod>(_serviceProvider, musicModFolder, musicMod);
            }

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
    }
}
