using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sma5h.Mods.Music.Helpers;
using Sma5h.Mods.Music.Interfaces;
using Sma5h.Mods.Music.Models;
using Sma5h.Mods.Music.MusicMods.MusicModModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sma5h.Mods.Music.MusicMods
{
    public class MusicMod : IMusicMod
    {
        protected readonly IMapper _mapper;
        protected readonly ILogger _logger;

        protected readonly MusicModConfig _musicModConfig;

        public string Id { get { return Mod.Id; } }
        public string Name { get { return Mod.Name; } }
        public string ModPath { get; }
        public MusicModInformation Mod => _musicModConfig;

        public MusicMod(IMapper mapper, ILogger<IMusicMod> logger, string musicModPath)
        {
            ModPath = musicModPath;
            _logger = logger;
            _mapper = mapper;
            _musicModConfig = LoadMusicModConfig();
        }

        public MusicMod(IMapper mapper, ILogger<IMusicMod> logger, string newModPath, MusicModInformation newMod)
        {
            ModPath = newModPath;
            _logger = logger;
            _mapper = mapper;
            _musicModConfig = InitializeNewMod(newModPath, newMod);
            SaveMusicModConfig();
        }

        public MusicModEntries GetMusicModEntries()
        {
            var output = new MusicModEntries();

            if (_musicModConfig == null)
                return output;

            //Process audio mods
            _logger.LogInformation("Mod {MusicMod} by '{Author}' - {NbrSongs} song(s)", _musicModConfig.Name, _musicModConfig.Author, _musicModConfig.Series.Sum(s => s.Games.Sum(p => p.Bgms.Count)));

            foreach (var series in _musicModConfig.Series)
            {
                output.SeriesEntries.Add(_mapper.Map(series, new SeriesEntry(series.UiSeriesId, EntrySource.Mod)));

                foreach (var game in series.Games)
                {
                    output.GameTitleEntries.Add(_mapper.Map(game, new GameTitleEntry(game.UiGameTitleId, EntrySource.Mod)));

                    foreach (var bgm in game.Bgms)
                    {
                        string filename = Path.Combine(ModPath, bgm.Filename);
                        if (!File.Exists(filename))
                        {
                            _logger.LogError("Mod {MusicMod}: Song {Song} ({ToneId}) doesn't exist.", _musicModConfig.Name, filename, bgm.ToneId);
                            continue;
                        }

                        GetUpdatedBgmAssignedInfoConfig(bgm.AssignedInfo);
                        GetUpdatedStreamSetConfig(bgm.StreamSet);

                        _logger.LogInformation("Mod {MusicMod}: Adding song {Song} ({ToneId})", _musicModConfig.Name, filename, bgm.ToneId);
                        var bgmDbRootEntry = _mapper.Map(bgm.DbRoot, new BgmDbRootEntry(bgm.DbRoot.UiBgmId, this));
                        bgmDbRootEntry.Title = bgm.MSBTLabels.Title;
                        bgmDbRootEntry.Author = bgm.MSBTLabels.Author;
                        bgmDbRootEntry.Copyright = bgm.MSBTLabels.Copyright;
                        bgmDbRootEntry.UiGameTitleId = game.UiGameTitleId; //Enforce
                        output.BgmDbRootEntries.Add(bgmDbRootEntry);
                        output.BgmStreamSetEntries.Add(_mapper.Map(bgm.StreamSet, new BgmStreamSetEntry(bgm.StreamSet.StreamSetId, this)));
                        output.BgmAssignedInfoEntries.Add(_mapper.Map(bgm.AssignedInfo, new BgmAssignedInfoEntry(bgm.AssignedInfo.InfoId, this)));
                        output.BgmStreamPropertyEntries.Add(_mapper.Map(bgm.StreamProperty, new BgmStreamPropertyEntry(bgm.StreamProperty.StreamId, this)));
                        output.BgmPropertyEntries.Add(_mapper.Map(bgm.BgmProperties, new BgmPropertyEntry(bgm.BgmProperties.NameId, filename, this) { AudioVolume = bgm.NUS3BankConfig.AudioVolume }));
                    }
                }
            }

            return output;
        }

        public async Task<bool> AddOrUpdateMusicModEntries(MusicModEntries musicModEntries)
        {
            if (musicModEntries == null)
            {
                return false;
            }

            //Update game/series in v4. v5 will have the same update mechanism for everything
            if (musicModEntries.BgmDbRootEntries.Count == 0 &&
               musicModEntries.BgmAssignedInfoEntries.Count == 0 &&
               musicModEntries.BgmStreamSetEntries.Count == 0 &&
               musicModEntries.BgmStreamPropertyEntries.Count == 0 &&
               musicModEntries.BgmPropertyEntries.Count == 0 &&
               musicModEntries.GameTitleEntries.Count == 0 &&
               musicModEntries.SeriesEntries.Count == 1)
            {
                return UpdateSeriesEntry(musicModEntries.SeriesEntries.FirstOrDefault());
            }

            if (musicModEntries.BgmDbRootEntries.Count == 0 &&
               musicModEntries.BgmAssignedInfoEntries.Count == 0 &&
               musicModEntries.BgmStreamSetEntries.Count == 0 &&
               musicModEntries.BgmStreamPropertyEntries.Count == 0 &&
               musicModEntries.BgmPropertyEntries.Count == 0 &&
               musicModEntries.SeriesEntries.Count == 1 &&
               musicModEntries.GameTitleEntries.Count == 1)
            {
                return UpdateGameTitleEntry(musicModEntries.SeriesEntries.FirstOrDefault(),
                    musicModEntries.GameTitleEntries.FirstOrDefault());
            }

            //For this specific mod, we want 1 entry of everything
            if (musicModEntries.BgmDbRootEntries.Count < 1 ||
               musicModEntries.BgmAssignedInfoEntries.Count < 1 ||
               musicModEntries.BgmStreamSetEntries.Count < 1 ||
               musicModEntries.BgmStreamPropertyEntries.Count < 1 ||
               musicModEntries.BgmPropertyEntries.Count < 1)
            {
                _logger.LogError("This update is not compatible with {MusicMod}", nameof(MusicMod));
                return false;
            }
            if (musicModEntries.BgmDbRootEntries.Count != musicModEntries.BgmAssignedInfoEntries.Count ||
                musicModEntries.BgmDbRootEntries.Count != musicModEntries.BgmStreamSetEntries.Count ||
                musicModEntries.BgmDbRootEntries.Count != musicModEntries.BgmStreamPropertyEntries.Count ||
                musicModEntries.BgmDbRootEntries.Count != musicModEntries.BgmPropertyEntries.Count)
            {
                _logger.LogError("This update is not compatible with {MusicMod}", nameof(MusicMod));
                return false;
            }

            for (int i = 0; i < musicModEntries.BgmDbRootEntries.Count; i++)
            {
                var dbRoot = musicModEntries.BgmDbRootEntries[i];
                var streamSet = musicModEntries.BgmStreamSetEntries[i];
                var assignedInfo = musicModEntries.BgmAssignedInfoEntries[i];
                var streamProperty = musicModEntries.BgmStreamPropertyEntries[i];
                var bgmProperty = musicModEntries.BgmPropertyEntries[i];
                var gameTitle = musicModEntries.GameTitleEntries.FirstOrDefault(p => p.UiGameTitleId == dbRoot.UiGameTitleId);
                var seriesEntry = musicModEntries.SeriesEntries.FirstOrDefault(p => p.UiSeriesId == gameTitle?.UiSeriesId);

                _logger.LogInformation("Adding {Filename} file to Mod {ModName}", bgmProperty.Filename, _musicModConfig.Name);

                //Get toneId
                var toneId = bgmProperty.NameId;
                var isFileEdit = _musicModConfig.Series.Any(s => s.Games.Any(p => p.Bgms.Any(s => s.ToneId == toneId)));
                var filename = bgmProperty.Filename;
                var filenameWithoutPath = Path.GetFileName(filename);

                //In case of change of file
                if (isFileEdit)
                {
                    var oldBgmData = _musicModConfig.Series.Select(s => s.Games.Select(p => p.Bgms.FirstOrDefault(s => s.ToneId == toneId)).Where(p => p != null)?.FirstOrDefault())?.FirstOrDefault();
                    string oldFilename = Path.Combine(ModPath, oldBgmData.Filename);
                    if (oldFilename.ToLower() != bgmProperty.Filename.ToLower())
                    {
                        _logger.LogInformation("Need to update filename for toneId: {ToneId}", oldBgmData.ToneId);

                        if (File.Exists(oldFilename))
                        {
                            File.Delete(oldFilename);
                            _logger.LogDebug("Old Filename deleted: {OldFilename}", oldFilename);
                        }
                        else
                        {
                            _logger.LogWarning("Old Filename could not be deleted: {OldFilename}", oldFilename);
                        }

                        isFileEdit = false;
                    }
                }

                //New
                if (!isFileEdit)
                {
                    var oldFileName = filenameWithoutPath;
                    filenameWithoutPath = string.Format("{0}{1}", toneId, Path.GetExtension(filenameWithoutPath));
                    _logger.LogDebug("New filename for {OldFilename}: {NewFilename}", oldFileName, filenameWithoutPath);

                    //Copy song
                    var outputFile = GetMusicModAudioFile(filenameWithoutPath);
                    if (!File.Exists(outputFile))
                        File.Copy(filename, outputFile);
                    else
                        _logger.LogWarning("The file {OutputFile} already exist and will not replaced. If you are migrating a mod you can ignore this warning.", outputFile);

                    //Set new name / TODO: Try to find a better, less "hacky" way
                    bgmProperty.ChangeFilename(outputFile);
                }

                //Attempt to retrieve, create none if needed
                var game = _musicModConfig.Series.SelectMany(s => s.Games.Where(p => p.UiGameTitleId == dbRoot.UiGameTitleId)).FirstOrDefault();
                if (game == null)
                {
                    if (gameTitle != null)
                        game = _mapper.Map<GameConfig>(gameTitle);
                    if (game == null)
                    {
                        game = new GameConfig()
                        {
                            UiGameTitleId = MusicConstants.InternalIds.GAME_TITLE_ID_DEFAULT,
                            UiSeriesId = MusicConstants.InternalIds.SERIES_ID_DEFAULT,
                            Title = new Dictionary<string, string>(),
                            Bgms = new List<BgmConfig>()
                        };
                    }
                    if (game.Bgms == null)
                        game.Bgms = new List<BgmConfig>();
                }
                else if(gameTitle != null)
                    game = _mapper.Map(gameTitle, game);

                //Remove game from previous series location
                _musicModConfig.Series.ForEach(g => g.Games.RemoveAll(p => p.UiGameTitleId == game.UiGameTitleId));

                var series = _musicModConfig.Series.Where(p => p.UiSeriesId == game.UiSeriesId).FirstOrDefault();
                if (series == null)
                {
                    if (seriesEntry != null)
                        series = _mapper.Map<SeriesConfig>(seriesEntry);
                    if (series == null)
                    {
                        series = new SeriesConfig()
                        {
                            UiSeriesId = MusicConstants.InternalIds.SERIES_ID_DEFAULT,
                            DispOrder = -1,
                            DispOrderSound = -1,
                            Title = new Dictionary<string, string>(),
                            Games = new List<GameConfig>()
                        };
                    }
                    if (series.Games == null)
                        series.Games = new List<GameConfig>();
                    _musicModConfig.Series.Add(series);
                }
                else if(seriesEntry != null)
                    series = _mapper.Map(seriesEntry, series);
                series.Games.Add(game);

                var bgmConfig = new BgmConfig()
                {
                    DbRoot = _mapper.Map<BgmDbRootConfig>(dbRoot),
                    StreamSet = _mapper.Map<BgmStreamSetConfig>(streamSet),
                    StreamProperty = _mapper.Map<BgmStreamPropertyConfig>(streamProperty),
                    AssignedInfo = _mapper.Map<BgmAssignedInfoConfig>(assignedInfo),
                    BgmProperties = _mapper.Map<BgmPropertyEntryConfig>(bgmProperty),
                    Filename = filenameWithoutPath,
                    ToneId = bgmProperty.NameId,
                    MSBTLabels = new MSBTLabelsConfig()
                    {
                        Author = dbRoot.Author,
                        Title = dbRoot.Title,
                        Copyright = dbRoot.Copyright
                    },
                    NUS3BankConfig = new NUS3BankConfig()
                    {
                        AudioVolume = bgmProperty.AudioVolume
                    }
                };

                //Remove previous version
                _musicModConfig.Series.ForEach(s => s.Games.ForEach(g => g.Bgms.RemoveAll(p => p.ToneId == bgmConfig.ToneId)));

                game.Bgms.Add(bgmConfig);

                //Remove potential empty groups
                _musicModConfig.Series.ForEach(s => s.Games.RemoveAll(g => g.Bgms == null || g.Bgms.Count == 0));
                _musicModConfig.Series.RemoveAll(p => p.Games.Count == 0);

                _logger.LogInformation("Added {Filename} file to Mod {ModName}", filename, _musicModConfig.Name);
            }

            //Save changes
            SaveMusicModConfig();

            _logger.LogInformation("Save Changes to Mod {ModName}", _musicModConfig.Name);

            return true;
        }

        public bool ReorderSongs(List<string> orderedList)
        {
            //Sanity check
            var allModSongsDict = _musicModConfig.Series.SelectMany(s => s.Games.SelectMany(p => p.Bgms).OrderBy(p => p.DbRoot.UiBgmId)).ToDictionary(p => p.DbRoot.UiBgmId, p => p);
            if (!orderedList.OrderBy(p => p).SequenceEqual(allModSongsDict.Select(p => p.Key)))
            {
                _logger.LogError("The provider list of songs to reorder did not match the list of songs found in the mod. Aborting reorder...");
                return false;
            }

            //Wipe all games & series
            var seriesCache = _musicModConfig.Series.ToList();
            var gamesCache = _musicModConfig.Series.SelectMany(s => s.Games).ToList();
            gamesCache.ForEach(p => p.Bgms.Clear());
            seriesCache.ForEach(p => p.Games.Clear());
            _musicModConfig.Series.Clear();

            //Reorder
            foreach (var orderedSongId in orderedList)
            {
                var orderedSong = allModSongsDict[orderedSongId];
                var game = gamesCache.FirstOrDefault(p => p.UiGameTitleId == orderedSong.DbRoot.UiGameTitleId);
                if (game == null)
                {
                    _logger.LogError("A game wasn't found during reordering. Aborting reorder...");
                    return false;
                }
                var series = seriesCache.FirstOrDefault(p => p.UiSeriesId == game.UiSeriesId);
                if (series == null)
                {
                    _logger.LogError("A series wasn't found during reordering. Aborting reorder...");
                    return false;
                }

                game.Bgms.Add(orderedSong);
                if (!series.Games.Contains(game))
                    series.Games.Add(game);
                if (!_musicModConfig.Series.Contains(series))
                    _musicModConfig.Series.Add(series);
            }

            //Save
            SaveMusicModConfig();

            return true;
        }

        public bool RemoveMusicModEntries(MusicModDeleteEntries musicModDeleteEntries)
        {
            if (musicModDeleteEntries == null)
            {
                return false;
            }

            //For this specific mod, we want 1 entry of everything
            if (musicModDeleteEntries.BgmDbRootEntries.Count != 1 ||
               musicModDeleteEntries.BgmAssignedInfoEntries.Count != 1 ||
               musicModDeleteEntries.BgmStreamSetEntries.Count != 1 ||
               musicModDeleteEntries.BgmStreamPropertyEntries.Count != 1 ||
               musicModDeleteEntries.BgmPropertyEntries.Count != 1)
            {
                _logger.LogError("This update is not compatible with {MusicMod}", nameof(MusicMod));
                return false;
            }

            var toneId = musicModDeleteEntries.BgmPropertyEntries.FirstOrDefault();

            _logger.LogInformation("Remove ToneId {ToneId} from Mod {ModName}", toneId, Mod.Name);
            var bgms = _musicModConfig.Series.SelectMany(s => s.Games.SelectMany(g => g.Bgms.Where(s => s.ToneId == toneId))).ToList();
            _musicModConfig.Series.ForEach(s =>
            {
                s.Games.ForEach(g => g.Bgms.RemoveAll(p => p.ToneId == toneId));
                s.Games.RemoveAll(p => p.Bgms.Count == 0);
            });
            _musicModConfig.Series.RemoveAll(p => p.Games.Count == 0);
            SaveMusicModConfig();
            foreach (var bgm in bgms)
            {
                var file = Path.Combine(ModPath, bgm.Filename);
                if (File.Exists(file))
                {
                    _logger.LogInformation("Remove File {File} from Mod {ModName}", file, Mod.Name);
                    File.Delete(file);
                }
            }
            return true;
        }

        public bool UpdateSeriesEntry(SeriesEntry seriesEntry)
        {
            if (_musicModConfig?.Series != null)
            {
                var series = _musicModConfig.Series.FirstOrDefault(p => p.UiSeriesId == seriesEntry.UiSeriesId);
                if(series != null)
                {
                    _mapper.Map(seriesEntry, series);
                    return SaveMusicModConfig();
                }
            }
            return true;
        }

        public bool UpdateGameTitleEntry(SeriesEntry seriesEntry, GameTitleEntry gameTitleEntry)
        {
            if (_musicModConfig?.Series != null)
            {
                var game = _musicModConfig.Series.SelectMany(s => s.Games.Where(p => p.UiGameTitleId == gameTitleEntry.UiGameTitleId))?.FirstOrDefault();
                if (game != null)
                {
                    var oldSeries = game.UiSeriesId;
                    _mapper.Map(gameTitleEntry, game);

                    if(oldSeries != game.UiSeriesId)
                    {
                        var series = _musicModConfig.Series.FirstOrDefault(p => p.UiSeriesId == seriesEntry.UiSeriesId);
                        if (series != null)
                        {
                            _mapper.Map(seriesEntry, series);
                        }
                        else
                        {
                            series = _mapper.Map<SeriesConfig>(seriesEntry);
                            series.Games = new List<GameConfig>();
                            _musicModConfig.Series.Add(series);
                        }

                        _musicModConfig.Series.ForEach(s => s.Games.RemoveAll(g => g.UiGameTitleId == game.UiGameTitleId));
                        series.Games.Add(game);
                        _musicModConfig.Series.RemoveAll(s => s.Games == null || s.Games.Count == 0);
                    }
                        
                    return SaveMusicModConfig();
                }
            }
            return true;
        }

        public bool UpdateModInformation(MusicModInformation configBase)
        {
            _musicModConfig.Author = configBase.Author;
            _musicModConfig.Name = configBase.Name;
            _musicModConfig.Website = configBase.Website;
            _musicModConfig.Description = configBase.Description;
            return SaveMusicModConfig();
        }

        protected virtual MusicModConfig InitializeNewMod(string newModPath, MusicModInformation newMod)
        {
            var musicModConfig = new MusicModConfig(Guid.NewGuid().ToString())
            {
                Name = newMod.Name,
                Author = newMod.Author,
                Description = newMod.Description,
                Series = new List<SeriesConfig>(),
                Website = newMod.Website
            };

            Directory.CreateDirectory(newModPath);

            return musicModConfig;
        }

        private string GetMusicModAudioFile(string bgmFilename)
        {
            return Path.Combine(ModPath, bgmFilename);
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
                if (output.Version == 2)
                    output = ConvertFromV2Mod(output);
                if (output.Version == 3)
                    output = ConvertFromV3Mod(output);
                return output;
            }
            else
            {
                //Cannot load music mod
                _logger.LogError("MusicModFile {MusicModFile} does not exist! Attempt to retrieve CSV.", ModPath);
            }

            return null;
        }

        protected virtual bool SaveMusicModConfig(MusicModConfig musicModConfig = null)
        {
            //Check if disabled
            if (Path.GetFileName(ModPath).StartsWith("."))
            {
                _logger.LogDebug("{MusicModFile} is disabled.");
                return false;
            }

            var saveMod = musicModConfig ?? _musicModConfig;
            var metadataJsonFile = Path.Combine(ModPath, MusicConstants.MusicModFiles.MUSIC_MOD_METADATA_JSON_FILE);
            File.WriteAllText(metadataJsonFile, JsonConvert.SerializeObject(saveMod, Formatting.Indented));

            return true;
        }

        private MusicModConfig ConvertFromV2Mod(MusicModConfig v2ModConfig)
        {
            if (v2ModConfig != null)
            {
                _logger.LogWarning("Convert v2 Mod {ModName} to v3.", v2ModConfig.Name);
                v2ModConfig.Version = 3;
                foreach (var game in v2ModConfig.Games)
                {
                    foreach (var bgm in game.Bgms)
                    {
                        CrackedHashValueConverter.UpdateBgmDbRootConfig(bgm.DbRoot);
                        CrackedHashValueConverter.UpdateAssignedInfoConfig(bgm.AssignedInfo);
                    }
                }
            }
            SaveMusicModConfig(v2ModConfig);
            return v2ModConfig;
        }

        private MusicModConfig ConvertFromV3Mod(MusicModConfig v3ModConfig)
        {
            if (v3ModConfig != null)
            {
                _logger.LogWarning("Convert v3 Mod {ModName} to v4.", v3ModConfig.Name);
                v3ModConfig.Version = 4;
                v3ModConfig.Series = v3ModConfig.Games.GroupBy(p => p.UiSeriesId).Select(p => new SeriesConfig()
                {
                    UiSeriesId = p.Key,
                    SaveNo = short.MinValue,
                    Games = p.ToList()
                }).ToList();
                v3ModConfig.Games = null;
            }
            return v3ModConfig;
        }

        private BgmStreamSetConfig GetUpdatedStreamSetConfig(BgmStreamSetConfig bgmStreamSetConfig)
        {
            if (!string.IsNullOrEmpty(bgmStreamSetConfig.SpecialCategory) && bgmStreamSetConfig.SpecialCategory.StartsWith("0x") &&
                MusicConstants.SPECIAL_CATEGORY_LABELS.ContainsKey(bgmStreamSetConfig.SpecialCategory))
            {
                bgmStreamSetConfig.SpecialCategory = MusicConstants.SPECIAL_CATEGORY_LABELS[bgmStreamSetConfig.SpecialCategory];
            }
            return bgmStreamSetConfig;
        }

        private BgmAssignedInfoConfig GetUpdatedBgmAssignedInfoConfig(BgmAssignedInfoConfig bgmAssignedInfoConfig)
        {
            if (!string.IsNullOrEmpty(bgmAssignedInfoConfig.Condition) && bgmAssignedInfoConfig.Condition.StartsWith("0x") &&
                MusicConstants.SOUND_CONDITION_LABELS.ContainsKey(bgmAssignedInfoConfig.Condition))
            {
                bgmAssignedInfoConfig.Condition = MusicConstants.SOUND_CONDITION_LABELS[bgmAssignedInfoConfig.Condition];
            }
            if (!string.IsNullOrEmpty(bgmAssignedInfoConfig.ConditionProcess) && bgmAssignedInfoConfig.ConditionProcess.StartsWith("0x") &&
                MusicConstants.SOUND_CONDITION_PROCESS_LABELS.ContainsKey(bgmAssignedInfoConfig.ConditionProcess))
            {
                bgmAssignedInfoConfig.ConditionProcess = MusicConstants.SOUND_CONDITION_PROCESS_LABELS[bgmAssignedInfoConfig.ConditionProcess];
            }
            return bgmAssignedInfoConfig;
        }
    }

    namespace MusicModModels
    {
        public class MusicModConfig : MusicModInformation
        {
            [JsonProperty("id")]
            public override string Id { get; }
            [JsonProperty("version")]
            public override int Version { get; set; }
            [JsonProperty("series")]
            public List<SeriesConfig> Series { get; set; }
            [JsonProperty("games")]
            public List<GameConfig> Games { get; set; }

            public MusicModConfig(string id)
            {
                Id = id;
                Version = 4;
            }

            public bool ShouldSerializeGames()
            {
                return false;
            }
        }

        public class GameConfig
        {
            [JsonProperty("ui_gametitle_id")]
            public string UiGameTitleId { get; set; }

            [JsonProperty("name_id")]
            public string NameId { get; set; }

            [JsonProperty("ui_series_id")]
            public string UiSeriesId { get; set; }

            [JsonProperty("0x1c38302364")]
            public bool Unk1 { get; set; }

            [JsonProperty("release")]
            public int Release { get; set; }

            [JsonProperty("msbt_title")]
            public Dictionary<string, string> Title { get; set; }

            [JsonProperty("bgms")]
            public List<BgmConfig> Bgms { get; set; }
        }

        public class SeriesConfig
        {
            [JsonProperty("ui_series_id")]
            public string UiSeriesId { get; set; }

            [JsonProperty("name_id")]
            public string NameId { get; set; }

            [JsonProperty("disp_order")]
            public sbyte DispOrder { get; set; }

            [JsonProperty("disp_order_sound")]
            public sbyte DispOrderSound { get; set; }

            [JsonProperty("save_no")]
            public short SaveNo { get; set; }

            [JsonProperty("0x1c38302364")]
            public bool Unk1 { get; set; }

            [JsonProperty("is_dlc")]
            public bool IsDlc { get; set; }

            [JsonProperty("is_patch")]
            public bool IsPatch { get; set; }

            [JsonProperty("dlc_chara_id")]
            public string DlcCharaId { get; set; }

            [JsonProperty("is_use_amiibo_bg")]
            public bool IsUseAmiiboBg { get; set; }

            [JsonProperty("msbt_title")]
            public Dictionary<string, string> Title { get; set; }

            [JsonProperty("games")]
            public List<GameConfig> Games { get; set; }
        }

        public class BgmConfig
        {
            [JsonProperty("tone_id")]
            public string ToneId { get; set; }
            [JsonProperty("filename")]
            public string Filename { get; set; }
            [JsonProperty("nus3bank_config")]
            public NUS3BankConfig NUS3BankConfig { get; set; }
            [JsonProperty("msbt_labels")]
            public MSBTLabelsConfig MSBTLabels { get; set; }
            [JsonProperty("bgm_properties")]
            public BgmPropertyEntryConfig BgmProperties { get; set; }
            [JsonProperty("db_root")]
            public BgmDbRootConfig DbRoot { get; set; }
            [JsonProperty("assigned_info")]
            public BgmAssignedInfoConfig AssignedInfo { get; set; }
            [JsonProperty("stream_set")]
            public BgmStreamSetConfig StreamSet { get; set; }
            [JsonProperty("stream_property")]
            public BgmStreamPropertyConfig StreamProperty { get; set; }
        }

        public class MSBTLabelsConfig
        {
            [JsonProperty("title")]
            public Dictionary<string, string> Title { get; set; }
            [JsonProperty("copyright")]
            public Dictionary<string, string> Copyright { get; set; }
            [JsonProperty("author")]
            public Dictionary<string, string> Author { get; set; }
        }

        public class NUS3BankConfig
        {
            [JsonProperty("volume")]
            public float AudioVolume { get; set; }
        }

        public class BgmDbRootConfig
        {
            [JsonProperty("ui_bgm_id")]
            public string UiBgmId { get; set; }

            [JsonProperty("stream_set_id")]
            public string StreamSetId { get; set; }

            [JsonProperty("rarity")]
            public string Rarity { get; set; }

            [JsonProperty("record_type")]
            public string RecordType { get; set; }

            [JsonProperty("ui_gametitle_id")]
            public string UiGameTitleId { get; set; }

            [JsonProperty("ui_gametitle_id_1")]
            public string UiGameTitleId1 { get; set; }

            [JsonProperty("ui_gametitle_id_2")]
            public string UiGameTitleId2 { get; set; }

            [JsonProperty("ui_gametitle_id_3")]
            public string UiGameTitleId3 { get; set; }

            [JsonProperty("ui_gametitle_id_4")]
            public string UiGameTitleId4 { get; set; }

            [JsonProperty("name_id")]
            public string NameId { get; set; }

            [JsonProperty("save_no")]
            public short SaveNo { get; set; }

            [JsonProperty("test_disp_order")]
            public short TestDispOrder { get; set; }

            [JsonProperty("menu_value")]
            public int MenuValue { get; set; }

            [JsonProperty("jp_region")]
            public bool JpRegion { get; set; }

            [JsonProperty("other_region")]
            public bool OtherRegion { get; set; }

            [JsonProperty("possessed")]
            public bool Possessed { get; set; }

            [JsonProperty("prize_lottery")]
            public bool PrizeLottery { get; set; }

            [JsonProperty("shop_price")]
            public uint ShopPrice { get; set; }

            [JsonProperty("count_target")]
            public bool CountTarget { get; set; }

            [JsonProperty("menu_loop")]
            public byte MenuLoop { get; set; }

            [JsonProperty("is_selectable_stage_make")]
            public bool IsSelectableStageMake { get; set; }

            [JsonProperty("is_selectable_movie_edit")]
            public bool IsSelectableMovieEdit { get; set; }

            [JsonProperty("is_selectable_original")]
            public bool IsSelectableOriginal { get; set; }

            [JsonProperty("is_dlc")]
            public bool IsDlc { get; set; }

            [JsonProperty("is_patch")]
            public bool IsPatch { get; set; }

            [JsonProperty("dlc_ui_chara_id")]
            public string DlcUiCharaId { get; set; }

            [JsonProperty("dlc_mii_hat_motif_id")]
            public string DlcMiiHatMotifId { get; set; }

            [JsonProperty("dlc_mii_body_motif_id")]
            public string DlcMiiBodyMotifId { get; set; }
            [JsonProperty("title")]
            public Dictionary<string, string> Title { get; set; }

            [JsonProperty("copyright")]
            public Dictionary<string, string> Copyright { get; set; }
            [JsonProperty("author")]
            public Dictionary<string, string> Author { get; set; }

            //Field here to handle older json version that did not have the discovered name
            [JsonProperty("0x18db285704")]
            public bool? Unk1 { get; set; }

            [JsonProperty("0x16fe9a28fe")]
            public bool? Unk2 { get; set; }

            [JsonProperty("0x0ff71e57ec")]
            public string Unk3 { get; set; }

            [JsonProperty("0x14341640b8")]
            public string Unk4 { get; set; }

            [JsonProperty("0x1560c0949b")]
            public string Unk5 { get; set; }

            public bool ShouldSerializeUnk1()
            {
                return false;
            }

            public bool ShouldSerializeUnk2()
            {
                return false;
            }

            public bool ShouldSerializeUnk3()
            {
                return false;
            }

            public bool ShouldSerializeUnk4()
            {
                return false;
            }

            public bool ShouldSerializeUnk5()
            {
                return false;
            }
        }

        public class BgmStreamSetConfig
        {
            [JsonProperty("stream_set_id")]
            public string StreamSetId { get; set; }

            [JsonProperty("special_category")]
            public string SpecialCategory { get; set; }

            [JsonProperty("info0")]
            public string Info0 { get; set; }

            [JsonProperty("info1")]
            public string Info1 { get; set; }

            [JsonProperty("info2")]
            public string Info2 { get; set; }

            [JsonProperty("info3")]
            public string Info3 { get; set; }

            [JsonProperty("info4")]
            public string Info4 { get; set; }

            [JsonProperty("info5")]
            public string Info5 { get; set; }

            [JsonProperty("info6")]
            public string Info6 { get; set; }

            [JsonProperty("info7")]
            public string Info7 { get; set; }

            [JsonProperty("info8")]
            public string Info8 { get; set; }

            [JsonProperty("info9")]
            public string Info9 { get; set; }

            [JsonProperty("info10")]
            public string Info10 { get; set; }

            [JsonProperty("info11")]
            public string Info11 { get; set; }

            [JsonProperty("info12")]
            public string Info12 { get; set; }

            [JsonProperty("info13")]
            public string Info13 { get; set; }

            [JsonProperty("info14")]
            public string Info14 { get; set; }

            [JsonProperty("info15")]
            public string Info15 { get; set; }
        }

        public class BgmAssignedInfoConfig
        {
            [JsonProperty("info_id")]
            public string InfoId { get; set; }

            [JsonProperty("stream_id")]
            public string StreamId { get; set; }

            [JsonProperty("condition")]
            public string Condition { get; set; }

            [JsonProperty("condition_process")]
            public string ConditionProcess { get; set; }

            [JsonProperty("start_frame")]
            public int StartFrame { get; set; }

            [JsonProperty("change_fadein_frame")]
            public int ChangeFadeInFrame { get; set; }

            [JsonProperty("change_start_delay_frame")]
            public int ChangeStartDelayFrame { get; set; }

            [JsonProperty("change_fadeout_frame")]
            public int ChangeFadoutFrame { get; set; }

            [JsonProperty("change_stop_delay_frame")]
            public int ChangeStopDelayFrame { get; set; }

            [JsonProperty("menu_change_fadein_frame")]
            public int MenuChangeFadeInFrame { get; set; }

            [JsonProperty("menu_change_start_delay_frame")]
            public int MenuChangeStartDelayFrame { get; set; }

            [JsonProperty("menu_change_fadeout_frame")]
            public int MenuChangeFadeOutFrame { get; set; }

            [JsonProperty("menu_change_stop_delay_frame")]
            public int MenuChangeStopDelayFrame { get; set; }

            //Field here to handle older json version that did not have the discovered name
            [JsonProperty("0x1c6a38c480")]
            public int? Unk1 { get; set; }

            public bool ShouldSerializeUnk1()
            {
                return false;
            }
        }

        public class BgmStreamPropertyConfig
        {
            [JsonProperty("stream_id")]
            public string StreamId { get; set; }

            [JsonProperty("data_name0")]
            public string DataName0 { get; set; }

            [JsonProperty("data_name1")]
            public string DataName1 { get; set; }

            [JsonProperty("data_name2")]
            public string DataName2 { get; set; }

            [JsonProperty("data_name3")]
            public string DataName3 { get; set; }

            [JsonProperty("data_name4")]
            public string DataName4 { get; set; }

            [JsonProperty("loop")]
            public byte Loop { get; set; }

            [JsonProperty("end_point")]
            public string EndPoint { get; set; }

            [JsonProperty("fadeout_frame")]
            public ushort FadeOutFrame { get; set; }

            [JsonProperty("start_point_suddendeath")]
            public string StartPointSuddenDeath { get; set; }

            [JsonProperty("start_point_transition")]
            public string StartPointTransition { get; set; }

            [JsonProperty("start_point0")]
            public string StartPoint0 { get; set; }

            [JsonProperty("start_point1")]
            public string StartPoint1 { get; set; }

            [JsonProperty("start_point2")]
            public string StartPoint2 { get; set; }

            [JsonProperty("start_point3")]
            public string StartPoint3 { get; set; }

            [JsonProperty("start_point4")]
            public string StartPoint4 { get; set; }
        }

        public class BgmPropertyEntryConfig
        {
            [JsonProperty("name_id")]
            public string NameId { get; set; }

            [JsonProperty("loop_start_ms")]
            public uint LoopStartMs { get; set; }

            [JsonProperty("loop_start_sample")]
            public uint LoopStartSample { get; set; }

            [JsonProperty("loop_end_ms")]
            public uint LoopEndMs { get; set; }

            [JsonProperty("loop_end_sample")]
            public uint LoopEndSample { get; set; }

            [JsonProperty("total_time_ms")]
            public uint TotalTimeMs { get; set; }

            [JsonProperty("total_samples")]
            public uint TotalSamples { get; set; }
        }
    }
}