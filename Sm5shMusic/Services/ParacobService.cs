using CsvHelper;
using Microsoft.Extensions.Logging;
using paracobNET;
using Sm5shMusic.Helpers;
using Sm5shMusic.Interfaces;
using Sm5shMusic.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Sm5shMusic.Services
{
    public class ParacobService : IParacobService
    {
        private readonly ILogger _logger;
        private readonly IResourceService _resourceService;
        private readonly List<BgmDbRootEntry> _coreBgmDb;
        private readonly List<GameTitleDbEntry> _coreGameTitleDb;
        private readonly Dictionary<ulong, string> _paramLabels;
        private string _lastNameId;
        private readonly string HEX_CAT_DBROOT = "db_root";
        private readonly string HEX_CAT_STREAM_SET = "stream_set";
        private readonly string HEX_CAT_ASSIGNED_INFO = "assigned_info";
        private readonly string HEX_CAT_STREAM_PROPERTY = "stream_property";
        private readonly string HEX_NAME_ID = "name_id";
        private readonly string HEX_UI_BGM_ID = "ui_bgm_id";
        private readonly string HEX_UI_GAMETITLE_ID = "ui_gametitle_id";
        private readonly string HEX_UI_SERIES_ID = "ui_series_id";
        private readonly string HEX_RELEASE = "release";
        private readonly string HEX_STREAM_SET_ID = "stream_set_id";
        private readonly string HEX_RARITY = "rarity";
        private readonly string HEX_RECORD_TYPE = "record_type";
        private readonly string HEX_SAVE_NO = "save_no";
        private readonly string HEX_TEST_DISP_ORDER = "test_disp_order";
        private readonly string HEX_MENU_VALUE = "menu_value";
        private readonly string HEX_SHOP_PRICE = "shop_price";
        private readonly string HEX_INFO0 = "info0";
        private readonly string HEX_INFO_ID = "info_id";
        private readonly string HEX_STREAM_ID = "stream_id";
        private readonly string HEX_DATA_NAME0 = "data_name0";
        private readonly string HEX_LOOP = "loop";
        private readonly string HEX_END_POINT = "end_point";
        private readonly string HEX_START_POINT_SUDDENDEATH = "start_point_suddendeath";
        private readonly string HEX_START_POINT_TRANSITION = "start_point_transition";
        private readonly string HEX_ORDERNBR = "order{0}";
        private readonly string HEX_INCIDENCENBR = "incidence{0}";
        private readonly string HEX_PLAYLIST_EXAMPLE = "bgmzelda";

        public ParacobService(IResourceService resourceService, ILogger<IParacobService> logger)
        {
            _logger = logger;
            _resourceService = resourceService;
            _paramLabels = GetParamLabels();
            _coreBgmDb = GetCoreDbRootBgmEntries();
            _coreGameTitleDb = GetCoreDbRootGameTitleEntries();
            _lastNameId = _coreBgmDb.Where(p => p.NameId != "random" && !string.IsNullOrEmpty(p.NameId)).OrderByDescending(p => Base36IncrementHelper.ToInt(p.NameId)).FirstOrDefault()?.NameId;
        }

        public List<GameTitleDbEntry> GetCoreDbRootGameTitleEntries()
        {
            var output = new List<GameTitleDbEntry>();

            if (!File.Exists(_resourceService.GetGameTitleDbResource()))
                return output;

            var t = new ParamFile();
            t.Open(_resourceService.GetGameTitleDbResource());

            var dbRoot = t.Root.Nodes[HEX_CAT_DBROOT] as ParamList;

            foreach(var node in dbRoot.Nodes)
            {
                var rootEntry = node as ParamStruct;
                output.Add(new GameTitleDbEntry()
                {
                    UiGameTitleId = GetNodeParamValueHash40(rootEntry, HEX_UI_GAMETITLE_ID),
                    UiSeriesId = GetNodeParamValueHash40(rootEntry, HEX_UI_SERIES_ID),
                    NameId = GetNodeParamValue<string>(rootEntry, HEX_NAME_ID),
                    Release = GetNodeParamValue<int>(rootEntry, HEX_RELEASE),
                    Unk1 = GetNodeParamValue<bool>(rootEntry, 0x1c38302364)
                });
            }

            return output;
        }

        public List<BgmDbRootEntry> GetCoreDbRootBgmEntries()
        {
            var output = new List<BgmDbRootEntry>();

            if (!File.Exists(_resourceService.GetBgmDbResource()))
                return output;

            var t = new ParamFile();
            t.Open(_resourceService.GetBgmDbResource());

            var dbRoot = t.Root.Nodes[HEX_CAT_DBROOT] as ParamList;

            foreach (var node in dbRoot.Nodes)
            {
                var rootEntry = node as ParamStruct;
                output.Add(new BgmDbRootEntry()
                {
                    UiBgmId = GetNodeParamValueHash40(rootEntry, HEX_UI_BGM_ID),
                    StreamSetId = GetNodeParamValueHash40(rootEntry, HEX_STREAM_SET_ID),
                    Rarity = GetNodeParamValueHash40(rootEntry, HEX_RARITY),
                    RecordType = GetNodeParamValueHash40(rootEntry, HEX_RECORD_TYPE),
                    UiGameTitleId = GetNodeParamValueHash40(rootEntry, HEX_UI_GAMETITLE_ID),
                    NameId = GetNodeParamValue<string>(rootEntry, HEX_NAME_ID),
                    SaveNo = GetNodeParamValue<short>(rootEntry, HEX_SAVE_NO),
                    TestDispOrder = GetNodeParamValue<short>(rootEntry, HEX_TEST_DISP_ORDER),
                    MenuValue = GetNodeParamValue<int>(rootEntry, HEX_MENU_VALUE),
                    ShopPrice = GetNodeParamValue<uint>(rootEntry, HEX_SHOP_PRICE),
                });
            }

            return output;
        }

        public bool GenerateGameTitlePrcFile(List<GameTitleDbNewEntry> gameTitleEntries, string outputFilePath)
        {
            var coreGameSeries = _coreGameTitleDb.Select(p => p.UiSeriesId).Distinct().ToList();
            var releaseIndex = _coreGameTitleDb.OrderByDescending(p => p.Release).First().Release + 1;

            var t = new ParamFile();
            t.Open(_resourceService.GetGameTitleDbResource());
            var dbRoot = t.Root.Nodes[HEX_CAT_DBROOT] as ParamList;

            foreach (var gameTitleEntry in gameTitleEntries)
            {
                var uiSeriesId = gameTitleEntry.UiSeriesId;
                if (!coreGameSeries.Contains(gameTitleEntry.UiSeriesId))
                    uiSeriesId = Constants.InternalIds.GameSeriesIdDefault;

                var newEntry = dbRoot.Nodes[0].Clone() as ParamStruct;
                SetNodeParamValue(newEntry, HEX_UI_GAMETITLE_ID, gameTitleEntry.UiGameTitleId);
                SetNodeParamValue(newEntry, HEX_UI_SERIES_ID, uiSeriesId);
                SetNodeParamValue(newEntry, HEX_NAME_ID, gameTitleEntry.NameId);
                SetNodeParamValue(newEntry, HEX_RELEASE, releaseIndex);
                dbRoot.Nodes.Add(newEntry);
                releaseIndex++;
            }

            t.Save(outputFilePath);

            return true;
        }

        public bool GenerateBgmPrcFile(List<BgmDbNewEntry> bgmEntries, string outputFilePath)
        {
            var saveNoIndex = (short)(_coreBgmDb.OrderByDescending(p => p.SaveNo).First().SaveNo + 1);
            var testDispOrderIndex = (short)(_coreBgmDb.OrderByDescending(p => p.TestDispOrder).First().TestDispOrder + 1);
            var menuValueIndex = _coreBgmDb.OrderByDescending(p => p.MenuValue).First().MenuValue + 1;

            var t = new ParamFile();
            t.Open(_resourceService.GetBgmDbResource());

            //DBROOT
            var dbRoot = t.Root.Nodes[HEX_CAT_DBROOT] as ParamList;
            foreach (var bgmEntry in bgmEntries)
            {
                var newEntry = dbRoot.Nodes[1].Clone() as ParamStruct;
                SetNodeParamValue(newEntry, HEX_UI_BGM_ID, bgmEntry.UiBgmId);
                SetNodeParamValue(newEntry, HEX_STREAM_SET_ID, bgmEntry.StreamSetId);
                SetNodeParamValue(newEntry, HEX_RARITY, bgmEntry.Rarity);
                SetNodeParamValue(newEntry, HEX_RECORD_TYPE, bgmEntry.RecordType);
                SetNodeParamValue(newEntry, HEX_UI_GAMETITLE_ID, bgmEntry.UiGameTitleId);
                SetNodeParamValue(newEntry, HEX_NAME_ID, bgmEntry.NameId);
                //SetNodeParamValue(newEntry, HEX_SAVE_NO, saveNoIndex);
                SetNodeParamValue(newEntry, HEX_TEST_DISP_ORDER, testDispOrderIndex);
                SetNodeParamValue(newEntry, HEX_MENU_VALUE, menuValueIndex);
                SetNodeParamValue(newEntry, HEX_SHOP_PRICE, (uint)0);
                dbRoot.Nodes.Add(newEntry);
                saveNoIndex++;
                testDispOrderIndex++;
                menuValueIndex++;

            }

            //STREAM_SET
            var streamSet = t.Root.Nodes[HEX_CAT_STREAM_SET] as ParamList;
            foreach (var bgmEntry in bgmEntries)
            {
                var newEntry = streamSet.Nodes[0].Clone() as ParamStruct;
                SetNodeParamValue(newEntry, HEX_STREAM_SET_ID, bgmEntry.StreamSetId);
                SetNodeParamValue(newEntry, HEX_INFO0, bgmEntry.Info0);
                streamSet.Nodes.Add(newEntry);
            }

            //ASSIGNED_INFO
            var assignedInfo = t.Root.Nodes[HEX_CAT_ASSIGNED_INFO] as ParamList;
            foreach (var bgmEntry in bgmEntries)
            {
                var newEntry = assignedInfo.Nodes[0].Clone() as ParamStruct;
                SetNodeParamValue(newEntry, HEX_INFO_ID, bgmEntry.InfoId);
                SetNodeParamValue(newEntry, HEX_STREAM_ID, bgmEntry.StreamId);
                assignedInfo.Nodes.Add(newEntry);
            }

            //STREAM_PROPERTY
            var streamProperty = t.Root.Nodes[HEX_CAT_STREAM_PROPERTY] as ParamList;
            foreach (var bgmEntry in bgmEntries)
            {
                var newEntry = streamProperty.Nodes[0].Clone() as ParamStruct;
                SetNodeParamValue(newEntry, HEX_STREAM_ID, bgmEntry.StreamId);
                SetNodeParamValue(newEntry, HEX_DATA_NAME0, bgmEntry.DataName0);
                streamProperty.Nodes.Add(newEntry);
            }

            //BGM PLAYLIST (QUICK & DIRTY)
            foreach (var bgmEntry in bgmEntries)
            {
                if(!bgmEntry.PlaylistId.StartsWith("bgm"))
                {
                    _logger.LogWarning("The playlist_id for song '{Song}' must start with 'bgm', skipping...", bgmEntry.ToneName);
                    continue;
                }

                var hexValue = Hash40Util.StringToHash40(bgmEntry.PlaylistId);

                ParamList bgmPlaylist = null;
                ParamStruct newEntry = null;
                //If the playlist doesn't exist...
                if (!t.Root.Nodes.ContainsKey(hexValue))
                {
                    var playlistToClone = t.Root.Nodes[HEX_PLAYLIST_EXAMPLE] as ParamList;
                    bgmPlaylist = playlistToClone.Clone() as ParamList;
                    
                    t.Root.Nodes.Add(hexValue, bgmPlaylist);
                    if (bgmPlaylist.Nodes.Count > 1)
                    {
                        bgmPlaylist.Nodes.RemoveRange(1, bgmPlaylist.Nodes.Count - 1);
                        newEntry = bgmPlaylist.Nodes[0] as ParamStruct;
                    }
                }
                else
                {
                    bgmPlaylist = t.Root.Nodes[hexValue] as ParamList;
                    newEntry = bgmPlaylist.Nodes[0].Clone() as ParamStruct;
                    bgmPlaylist.Nodes.Add(newEntry);
                }

                //Add song
                SetNodeParamValue(newEntry, HEX_UI_BGM_ID, bgmEntry.UiBgmId);
                for (int i = 0; i <= 15; i++)
                {
                    SetNodeParamValue(newEntry, string.Format(HEX_ORDERNBR, i), (short)(bgmPlaylist.Nodes.Count));
                    SetNodeParamValue(newEntry, string.Format(HEX_INCIDENCENBR, i), (ushort)500);
                }
            }

            t.Save(outputFilePath);

            return true;
        }

        public string GetNewBgmId()
        {
            _lastNameId = Base36IncrementHelper.ToString(Base36IncrementHelper.ToInt(_lastNameId) + 1);
            if (_lastNameId == "random")
                return GetNewBgmId();
            return _lastNameId;
        }

        #region Private
        private Dictionary<ulong, string> GetParamLabels()
        {
            var output = new Dictionary<ulong, string>();

            if (!File.Exists(_resourceService.GetBgmDbLabelsCsvResource()))
                return output;

            using (var reader = new StreamReader(_resourceService.GetBgmDbLabelsCsvResource()))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<dynamic>();
                    foreach (var record in records)
                    {
                        var id = Convert.ToUInt64(record.ID, 16);
                        output.Add(id, record.Label);
                    }
                }
            }
            return output;
        }

        private string GetNodeParamValueHash40(ParamStruct root, string hex40)
        {
            var paramValue = root.Nodes[hex40] as ParamValue;
            if (paramValue.TypeKey == ParamType.hash40)
                return Hash40Util.FormatToString((ulong)paramValue.Value, _paramLabels);
            return string.Empty;
        }

        private string GetNodeParamValueHash40(ParamStruct root, ulong hex40)
        {
            var paramValue = root.Nodes[hex40] as ParamValue;
            if (paramValue.TypeKey == ParamType.hash40)
                return Hash40Util.FormatToString((ulong)paramValue.Value, _paramLabels);
            return string.Empty;
        }

        private T GetNodeParamValue<T>(ParamStruct root, string hex40)
        {
            return (T)(root.Nodes[hex40] as ParamValue).Value;
        }

        private T GetNodeParamValue<T>(ParamStruct root, ulong hex40)
        {
            return (T)(root.Nodes[hex40] as ParamValue).Value;
        }

        private void SetNodeParamValue(ParamStruct root, string hex40, object value)
        {
            var paramValue = ((ParamValue)root.Nodes[hex40]);
            if(paramValue.TypeKey == ParamType.hash40)
                paramValue.Value = Hash40Util.StringToHash40(value.ToString());
            else
                paramValue.Value = value;
        }

        private void SetNodeParamValue(ParamStruct root, ulong hex40, object value)
        {
            var paramValue = ((ParamValue)root.Nodes[hex40]);
            if (paramValue.TypeKey == ParamType.hash40)
                paramValue.Value = Hash40Util.StringToHash40(value.ToString());
            else
                paramValue.Value = value;
        }
        #endregion
    }
}
