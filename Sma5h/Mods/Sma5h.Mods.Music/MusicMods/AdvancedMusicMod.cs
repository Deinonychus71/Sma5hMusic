using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sma5h.Mods.Music.Helpers;
using Sma5h.Mods.Music.Interfaces;
using Sma5h.Mods.Music.Models;
using Sma5h.Mods.Music.MusicMods.AdvancedMusicModModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sma5h.Mods.Music.MusicMods
{
    public class AdvancedMusicMod : IMusicMod
    {
        protected readonly IMapper _mapper;
        protected readonly ILogger _logger;

        protected readonly MusicModConfig _musicModConfig;

        public string Id { get { return Mod.Id; } }
        public string Name { get { return Mod.Name; } }
        public string ModPath { get; }
        public MusicModInformation Mod => _musicModConfig;

        public AdvancedMusicMod(IMapper mapper, ILogger<IMusicMod> logger, string musicModPath)
        {
            ModPath = musicModPath;
            _logger = logger;
            _mapper = mapper;
            _musicModConfig = LoadMusicModConfig();
        }

        public AdvancedMusicMod(IMapper mapper, ILogger<IMusicMod> logger, string newModPath, MusicModInformation newMod)
        {
            ModPath = newModPath;
            _logger = logger;
            _mapper = mapper;
            _musicModConfig = InitializeNewMod(newModPath, newMod);
            SaveMusicModConfig();
        }

        public AdvancedMusicMod(IMapper mapper, ILogger<IMusicMod> logger, string musicModPath, IMusicMod oldMod)
        {
            ModPath = musicModPath;
            _logger = logger;
            _mapper = mapper;
            _musicModConfig = ConvertOldMod(oldMod);
            var bgms = oldMod.GetMusicModEntries();
            AddOrUpdateMusicModEntries(bgms).GetAwaiter().GetResult();
            SaveMusicModConfig();
        }

        public MusicModEntries GetMusicModEntries()
        {
            var output = new MusicModEntries();

            if (_musicModConfig == null)
                return output;

            //Process audio mods
            _logger.LogInformation("Mod {MusicMod} by '{Author}' - {NbrSongs} song(s)", _musicModConfig.Name, _musicModConfig.Author, _musicModConfig.Games.Sum(p => p.Bgms.Count));

            foreach (var game in _musicModConfig.Games)
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

            return output;
        }

        public async Task<bool> AddOrUpdateMusicModEntries(MusicModEntries musicModEntries)
        {
            if (musicModEntries == null)
            {
                return false;
            }

            //Update game in v2. v3 will have the same update mechanism for everything
            if (musicModEntries.BgmDbRootEntries.Count == 0 &&
               musicModEntries.BgmAssignedInfoEntries.Count == 0 &&
               musicModEntries.BgmStreamSetEntries.Count == 0 &&
               musicModEntries.BgmStreamPropertyEntries.Count == 0 &&
               musicModEntries.BgmPropertyEntries.Count == 0 &&
               musicModEntries.GameTitleEntries.Count == 1)
            {
                return UpdateGameTitleEntry(musicModEntries.GameTitleEntries.FirstOrDefault());
            }

            //For this specific mod, we want 1 entry of everything
            if (musicModEntries.BgmDbRootEntries.Count < 1 ||
               musicModEntries.BgmAssignedInfoEntries.Count < 1 ||
               musicModEntries.BgmStreamSetEntries.Count < 1 ||
               musicModEntries.BgmStreamPropertyEntries.Count < 1 ||
               musicModEntries.BgmPropertyEntries.Count < 1)
            {
                _logger.LogError("This update is not compatible with {MusicMod}", nameof(AdvancedMusicMod));
                return false;
            }
            if (musicModEntries.BgmDbRootEntries.Count != musicModEntries.BgmAssignedInfoEntries.Count ||
                musicModEntries.BgmDbRootEntries.Count != musicModEntries.BgmStreamSetEntries.Count ||
                musicModEntries.BgmDbRootEntries.Count != musicModEntries.BgmStreamPropertyEntries.Count ||
                musicModEntries.BgmDbRootEntries.Count != musicModEntries.BgmPropertyEntries.Count)
            {
                _logger.LogError("This update is not compatible with {MusicMod}", nameof(AdvancedMusicMod));
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

                _logger.LogInformation("Adding {Filename} file to Mod {ModName}", bgmProperty.Filename, _musicModConfig.Name);

                //Get toneId
                var toneId = bgmProperty.NameId;
                var isFileEdit = _musicModConfig.Games.Any(p => p.Bgms.Any(s => s.ToneId == toneId));
                var filename = bgmProperty.Filename;
                var filenameWithoutPath = Path.GetFileName(filename);

                //In case of change of file
                if (isFileEdit)
                {
                    var oldBgmData = _musicModConfig.Games.Select(p => p.Bgms.FirstOrDefault(s => s.ToneId == toneId)).Where(p => p != null)?.FirstOrDefault();
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
                var game = _musicModConfig.Games.Where(p => p.UiGameTitleId == dbRoot.UiGameTitleId).FirstOrDefault();
                if (game == null)
                {
                    if (gameTitle != null)
                        game = _mapper.Map<GameConfig>(gameTitle);
                    if (game == null)
                    {
                        game = new GameConfig()
                        {
                            UiGameTitleId = MusicConstants.InternalIds.GAME_TITLE_ID_DEFAULT,
                            UiSeriesId = MusicConstants.InternalIds.GAME_SERIES_ID_DEFAULT,
                            Title = new Dictionary<string, string>(),
                            Bgms = new List<BgmConfig>()
                        };
                    }
                    _musicModConfig.Games.Add(game);
                    if (game.Bgms == null)
                        game.Bgms = new List<BgmConfig>();
                }

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
                _musicModConfig.Games.ForEach(g => g.Bgms.RemoveAll(p => p.ToneId == bgmConfig.ToneId));

                game.Bgms.Add(bgmConfig);

                //Remove potential empty groups
                _musicModConfig.Games.RemoveAll(p => p.Bgms.Count == 0);

                _logger.LogInformation("Added {Filename} file to Mod {ModName}", filename, _musicModConfig.Name);
            }

            //Save changes
            SaveMusicModConfig();

            _logger.LogInformation("Save Changes to Mod {ModName}", _musicModConfig.Name);

            return true;
        }

        public bool UpdateGameTitleEntry(GameTitleEntry gameTitleEntry)
        {
            if (_musicModConfig?.Games != null)
            {
                bool change = false;

                foreach (var game in _musicModConfig.Games)
                {
                    if (game.UiGameTitleId == gameTitleEntry.UiGameTitleId)
                    {
                        _mapper.Map(gameTitleEntry, game);
                        change = true;
                    }
                }

                if (change)
                {
                    return SaveMusicModConfig();
                }
            }
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
                _logger.LogError("This update is not compatible with {MusicMod}", nameof(AdvancedMusicMod));
                return false;
            }

            var toneId = musicModDeleteEntries.BgmPropertyEntries.FirstOrDefault();

            _logger.LogInformation("Remove ToneId {ToneId} from Mod {ModName}", toneId, Mod.Name);
            var bgms = _musicModConfig.Games.SelectMany(g => g.Bgms.Where(s => s.ToneId == toneId)).ToList();
            _musicModConfig.Games.ForEach(g => g.Bgms.RemoveAll(p => p.ToneId == toneId));
            _musicModConfig.Games.RemoveAll(p => p.Bgms.Count == 0);
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
                Games = new List<GameConfig>(),
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
            File.WriteAllText(metadataJsonFile, JsonConvert.SerializeObject(_musicModConfig, Formatting.Indented));

            return true;
        }

        protected MusicModConfig ConvertOldMod(IMusicMod oldModConfig)
        {
            _logger.LogWarning("Convert Old Mod {ModName} to version 2. A backup file will be created...", oldModConfig.Name);

            var newMod = new MusicModConfig(Guid.NewGuid().ToString())
            {
                Author = oldModConfig.Mod.Author,
                Description = oldModConfig.Mod.Description,
                Website = oldModConfig.Mod.Website,
                Name = oldModConfig.Name,
                Version = 2,
                Games = new List<GameConfig>()
            };
            return newMod;
        }
    }

    namespace AdvancedMusicModModels
    {
        public class MusicModConfig : MusicModInformation
        {
            [JsonProperty("id")]
            public override string Id { get; }
            [JsonProperty("version")]
            public override int Version { get; set; }
            [JsonProperty("games")]
            public List<GameConfig> Games { get; set; }

            public MusicModConfig(string id)
            {
                Id = id;
                Version = 2;
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

            [JsonProperty("0x18db285704")]
            public bool Unk1 { get; set; }

            [JsonProperty("0x16fe9a28fe")]
            public bool Unk2 { get; set; }

            [JsonProperty("is_dlc")]
            public bool IsDlc { get; set; }

            [JsonProperty("is_patch")]
            public bool IsPatch { get; set; }

            [JsonProperty("0x0ff71e57ec")]
            public string Unk3 { get; set; }

            [JsonProperty("0x14341640b8")]
            public string Unk4 { get; set; }

            [JsonProperty("0x1560c0949b")]
            public string Unk5 { get; set; }
            [JsonProperty("title")]
            public Dictionary<string, string> Title { get; set; }
            [JsonProperty("copyright")]
            public Dictionary<string, string> Copyright { get; set; }
            [JsonProperty("author")]
            public Dictionary<string, string> Author { get; set; }
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
