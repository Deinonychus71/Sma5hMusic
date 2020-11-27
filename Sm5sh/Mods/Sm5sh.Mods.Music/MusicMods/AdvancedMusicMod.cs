using AutoMapper;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sm5sh.Core.Helpers;
using Sm5sh.Mods.Music.Helpers;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using Sm5sh.Mods.Music.MusicMods.AdvancedMusicModModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sm5sh.Mods.Music.MusicMods
{
    public class AdvancedMusicMod : IMusicMod
    {
        private readonly IAudioMetadataService _audioMetadataService;
        protected readonly IMapper _mapper;
        protected readonly ILogger _logger;

        protected readonly MusicModConfig _musicModConfig;

        public string Id { get { return Mod.Id; } }
        public string Name { get { return Mod.Name; } }
        public string ModPath { get; }
        public MusicModInformation Mod => _musicModConfig;

        public AdvancedMusicMod(IAudioMetadataService audioMetadataService, IMapper mapper, ILogger<IMusicMod> logger, string musicModPath)
        {
            ModPath = musicModPath;
            _audioMetadataService = audioMetadataService;
            _logger = logger;
            _mapper = mapper;
            _musicModConfig = LoadMusicModConfig();
        }

        public AdvancedMusicMod(IAudioMetadataService audioMetadataService, IMapper mapper, ILogger<IMusicMod> logger, string newModPath, MusicModInformation newMod)
        {
            ModPath = newModPath;
            _audioMetadataService = audioMetadataService;
            _logger = logger;
            _mapper = mapper;
            _musicModConfig = InitializeNewMod(newModPath, newMod);
            SaveMusicModConfig();
        }

        public List<BgmEntry> GetBgms()
        {
            if (_musicModConfig == null)
            {
                return new List<BgmEntry>();
            }

            //Process audio mods
            _logger.LogInformation("Mod {MusicMod} by '{Author}' - {NbrSongs} song(s)", _musicModConfig.Name, _musicModConfig.Author, _musicModConfig.Games.Sum(p => p.Bgms.Count));

            var output = new List<BgmEntry>();
            foreach (var game in _musicModConfig.Games)
            {
                var gameEntry = _mapper.Map(game, new GameTitleEntry(game.UiGameTitleId));

                foreach (var bgm in game.Bgms)
                {
                    var newBgmEntry = MapBgmEntry(bgm, gameEntry);
                    if (!File.Exists(newBgmEntry.Filename))
                    {
                        _logger.LogError("Mod {MusicMod}: Song {Song} ({ToneId}) doesn't exist.", _musicModConfig.Name, newBgmEntry.Filename, bgm.ToneId);
                    }
                    else
                    {
                        output.Add(newBgmEntry);
                        _logger.LogInformation("Mod {MusicMod}: Adding song {Song} ({ToneId})", _musicModConfig.Name, newBgmEntry.Filename, bgm.ToneId);
                    }
                }
            }

            return output;
        }

        public BgmEntry AddBgm(string filename)
        {
            if (_musicModConfig == null)
            {
                return null;
            }

            _logger.LogInformation("Adding {Filename} file to Mod {ModName}", filename, _musicModConfig.Name);

            //Get toneId
            var toneId = Path.GetFileNameWithoutExtension(filename).Replace(Constants.InternalIds.NUS3AUDIO_FILE_PREFIX, string.Empty).ToLower();
            var filenameWithoutPath = Path.GetFileName(filename);

            var toneIdExists = _musicModConfig.Games.Any(p => p.Bgms.Any(s => s.ToneId == toneId));
            if (toneIdExists)
            {
                _logger.LogError("The tone id {ToneId} was already found in this mod, skipping.", toneId);
                return null;
            }
            var filenameExists = _musicModConfig.Games.Any(p => p.Bgms.Any(s => s.Filename == filenameWithoutPath));
            if (filenameExists)
            {
                _logger.LogError("The filename {Filename} was already found in this mod, skipping.", filenameWithoutPath);
                return null;
            }

            //Create default game
            var game = _musicModConfig.Games.Where(p => p.UiGameTitleId == Constants.InternalIds.GAME_TITLE_ID_DEFAULT).FirstOrDefault();
            if(game == null)
            {
                game = new GameConfig()
                {
                    UiGameTitleId = Constants.InternalIds.GAME_TITLE_ID_DEFAULT,
                    UiSeriesId = Constants.InternalIds.GAME_SERIES_ID_DEFAULT,
                    Title = new Dictionary<string, string>(),
                    Bgms = new List<BgmConfig>()
                };
                _musicModConfig.Games.Add(game);
            }

            var audioCuePoints = _audioMetadataService.GetCuePoints(filename);
            if (audioCuePoints == null || audioCuePoints.TotalSamples == 0)
            {
                _logger.LogError("The filename {Filename} didn't have cue points.", filenameWithoutPath);
                return null;
            }

            var newBgmEntry = new BgmEntry(toneId, this);
            var newBgmConfig = _mapper.Map<BgmConfig>(newBgmEntry);
            newBgmConfig.Filename = filenameWithoutPath;

            game.Bgms.Add(newBgmConfig);

            //Copy song
            File.Copy(filename, GetMusicModAudioFile(filenameWithoutPath));

            //Save changes
            SaveMusicModConfig();

            _logger.LogInformation("Added {Filename} file to Mod {ModName}", filename, _musicModConfig.Name);

            return MapBgmEntry(newBgmConfig, game);
        }

        public bool UpdateBgm(BgmEntry bgmEntry)
        {
            /*if (_musicModConfig == null)
            {
                return false;
            }

            Song modSong = null;
            //Cleaning up in case game is no longer used
            foreach (var gameToClean in _musicModConfig.Games)
            {
                modSong = gameToClean.Songs.FirstOrDefault(p => p.Id == bgmEntry.ToneId);
                if (modSong != null)
                {
                    gameToClean.Songs.Remove(modSong);
                    if (gameToClean.Songs.Count == 0)
                        _musicModConfig.Games.Remove(gameToClean);
                    break;
                }
            }

            //If null, there's something wrong...
            if (modSong == null)
            {
                _logger.LogError("The BGM Entry with ToneId {ToneId} was not found in the mod", bgmEntry.ToneId);
                return false;
            }

            //Applying updates...
            modSong = new Song()
            {
                Author = bgmEntry.MSBTLabels.Author,
                Copyright = bgmEntry.MSBTLabels.Copyright,
                Title = bgmEntry.MSBTLabels.Title,
                RecordType = bgmEntry.RecordType,
                Playlists = bgmEntry.Playlists.Select(p => new PlaylistInfo() { Id = p.Key }).ToList()
            };
            if (bgmEntry.SpecialCategory != null)
                modSong.SpecialCategory = new SpecialCategory() { Category = bgmEntry.SpecialCategory.Id, Parameters = bgmEntry.SpecialCategory.Parameters };

            var game = _musicModConfig.Games.FirstOrDefault(p => p.Id == bgmEntry.GameTitle.UiGameTitleId);
            if(game == null)
            {
                game = new Game()
                {
                    Id = bgmEntry.GameTitle.UiGameTitleId,
                    SeriesId = bgmEntry.GameTitle.UiSeriesId,
                    Songs = new List<Song>(),
                    Title = bgmEntry.GameTitle.MSBTTitle
                };
                _musicModConfig.Games.Add(game);
            }
            game.Songs.Add(modSong);

            //Save changes
            SaveMusicModConfig();*/

            return true;
        }

        public bool RemoveBgm(string toneId)
        {
            //TODO
            return true;
        }

        public void UpdateModInformation(MusicModInformation configBase)
        {
            _musicModConfig.Author = configBase.Author;
            _musicModConfig.Name = configBase.Name;
            _musicModConfig.Website = configBase.Website;
            SaveMusicModConfig();
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

        private BgmEntry MapBgmEntry(BgmConfig bgmConfig, GameTitleEntry gameTitleEntry)
        {
            var newBgmEntry = _mapper.Map(bgmConfig, new BgmEntry(bgmConfig.ToneId, this));
            newBgmEntry.GameTitle = gameTitleEntry;
            newBgmEntry.Filename = Path.Combine(ModPath, newBgmEntry.Filename);
            return newBgmEntry;
        }

        private BgmEntry MapBgmEntry(BgmConfig bgmConfig, GameConfig gameConfig)
        {
            return MapBgmEntry(bgmConfig, _mapper.Map(gameConfig, new GameTitleEntry(gameConfig.UiGameTitleId)));
        }

        private string GetMusicModAudioFile(string bgmFilename)
        {
            return Path.Combine(ModPath, bgmFilename);
        }

        protected virtual MusicModConfig LoadMusicModConfig()
        {
            //Attempt JSON
            var metadataJsonFile = Path.Combine(ModPath, Constants.MusicModFiles.MUSIC_MOD_METADATA_JSON_FILE);
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

            var metadataJsonFile = Path.Combine(ModPath, Constants.MusicModFiles.MUSIC_MOD_METADATA_JSON_FILE);
            File.WriteAllText(metadataJsonFile, JsonConvert.SerializeObject(_musicModConfig));

            return true;
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
            [JsonProperty("streaming_property")]
            public BgmStreamPropertyConfig StreamingProperty { get; set; }
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

            [JsonProperty("0x1c6a38c480")]
            public int Unk1 { get; set; }
        }

        public class BgmStreamPropertyConfig
        {
            [JsonProperty("stream_id")]
            public string StreamId { get; set; }

            [JsonProperty("data_name0")]
            public string DateName0 { get; set; }

            [JsonProperty("data_name1")]
            public string DateName1 { get; set; }

            [JsonProperty("data_name2")]
            public string DateName2 { get; set; }

            [JsonProperty("data_name3")]
            public string DateName3 { get; set; }

            [JsonProperty("data_name4")]
            public string DateName4 { get; set; }

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
            public ulong LoopStartMs { get; set; }

            [JsonProperty("loop_start_sample")]
            public ulong LoopStartSample { get; set; }

            [JsonProperty("loop_end_ms")]
            public ulong LoopEndMs { get; set; }

            [JsonProperty("loop_end_sample")]
            public ulong LoopEndSample { get; set; }

            [JsonProperty("total_time_ms")]
            public ulong TotalTimeMs { get; set; }

            [JsonProperty("total_samples")]
            public ulong TotalSamples { get; set; }
        }
    }
}
