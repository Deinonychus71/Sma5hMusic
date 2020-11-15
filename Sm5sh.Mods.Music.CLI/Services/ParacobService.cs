namespace Sm5shMusic.Services
{
    /*public class ParacobService : IParacobService
    {
        private readonly string LOG_NEW_GAMETITLE_ENTRY_DB_ROOT;
        private readonly string LOG_NEW_BGM_ENTRY_DB_ROOT;
        private readonly string LOG_NEW_BGM_ENTRY_STREAM_SET;
        private readonly string LOG_NEW_BGM_ENTRY_ASSIGNED_INFO;
        private readonly string LOG_NEW_BGM_ENTRY_STREAM_PROPERTY;
        private readonly string LOG_NEW_BGM_ENTRY_PLAYLIST;
        private readonly string LOG_UPDATE_STAGE_ENTRY;

        public ParacobService(IResourceService resourceService, ILogger<IParacobService> logger)
        {
            _logger = logger;
            _resourceService = resourceService;
            _paramLabels = GetParamLabels();
            _coreBgmDb = GetCoreDbRootBgmEntries();
            _coreGameTitleDb = GetCoreDbRootGameTitleEntries();
            _coreStageDb = GetCoreDbRootStageEntries();
            _lastNameId = _coreBgmDb.Where(p => p.NameId != "random" && !string.IsNullOrEmpty(p.NameId)).OrderByDescending(p => Base36IncrementHelper.ToInt(p.NameId)).FirstOrDefault()?.NameId;
            LOG_NEW_GAMETITLE_ENTRY_DB_ROOT = $"Generating new Game Title DB - db_root entry: {HEX_UI_GAMETITLE_ID}: {{UI_GAMETITLE_ID}}, {HEX_UI_SERIES_ID}: {{HEX_UI_SERIES_ID}}, {HEX_NAME_ID}: {{HEX_NAME_ID}}, {HEX_RELEASE}: {{HEX_RELEASE}}";
            LOG_NEW_BGM_ENTRY_DB_ROOT = $"Generating new BGM DB - db_root: {HEX_UI_BGM_ID}: {{HEX_UI_BGM_ID}}, {HEX_STREAM_SET_ID}: {{HEX_STREAM_SET_ID}}, {HEX_RARITY}: {{HEX_RARITY}}, {HEX_RECORD_TYPE}: {{HEX_RECORD_TYPE}}, {HEX_UI_GAMETITLE_ID}: {{HEX_UI_GAMETITLE_ID}}, {HEX_NAME_ID}: {{HEX_NAME_ID}}, {HEX_TEST_DISP_ORDER}: {{HEX_TEST_DISP_ORDER}}, {HEX_MENU_VALUE}: {{HEX_MENU_VALUE}}, {HEX_SHOP_PRICE}: {{HEX_SHOP_PRICE}}";
            LOG_NEW_BGM_ENTRY_STREAM_SET = $"Generating new BGM DB - stream_set entry: {HEX_STREAM_SET_ID}: {{HEX_STREAM_SET_ID}}, {HEX_INFO0}: {{HEX_INFO0}}";
            LOG_NEW_BGM_ENTRY_ASSIGNED_INFO = $"Generating new BGM DB - assigned_info entry: {HEX_INFO_ID}: {{HEX_INFO_ID}}, {HEX_STREAM_ID}: {{HEX_STREAM_ID}}";
            LOG_NEW_BGM_ENTRY_STREAM_PROPERTY = $"Generating new BGM DB - stream_property entry: {HEX_STREAM_ID}: {{HEX_STREAM_ID}}, {HEX_DATA_NAME0}: {{HEX_DATA_NAME0}}";
            LOG_NEW_BGM_ENTRY_PLAYLIST = $"Generating new BGM DB - playlist {{PlaylistId}} entry : {HEX_UI_BGM_ID}: {{HEX_UI_BGM_ID}}, order: {{HEX_ORDERNBR}}, incidence: {{HEX_INCIDENCENBR}}";
            LOG_UPDATE_STAGE_ENTRY = $"Updating Stage DB - Stage Id {{StageId}}:0x{{Hash40Hex:X}} entry : {HEX_BGM_SET_ID}: {{HEX_BGM_SET_ID}}, {HEX_BGM_SETTING_NO}: {{HEX_BGM_SETTING_NO}}"; 
        }

        public bool UpdateStagePrcFile(List<StageDbEntry> stageEntries, string outputFilePath)
        {
            var t = new ParamFile();
            t.Open(_resourceService.GetStageDbResource());
            var dbRoot = t.Root.Nodes[HEX_CAT_DBROOT] as ParamList;

            int entriesUpdated = 0;
            foreach (var stageNode in dbRoot.Nodes)
            {
                var stageEntry = (ParamStruct)stageNode;
                var stageIdHex = stageEntry.Nodes[HEX_UI_STAGE_ID] as ParamValue;
                var stageId = Hash40Util.FormatToString((ulong)stageIdHex.Value, _paramLabels);

                var stageUpdateEntry = stageEntries.FirstOrDefault(p => p.UiStageId == stageId);
                if(stageUpdateEntry != null)
                {
                    _logger.LogDebug(LOG_UPDATE_STAGE_ENTRY, stageId, stageIdHex.Value, stageUpdateEntry.BgmSetId, stageUpdateEntry.BgmSettingNo);

                    SetNodeParamValue(stageEntry, HEX_BGM_SET_ID, stageUpdateEntry.BgmSetId);
                    SetNodeParamValue(stageEntry, HEX_BGM_SETTING_NO, stageUpdateEntry.BgmSettingNo);
                    entriesUpdated++;
                }
                else
                {
                    _logger.LogDebug("Stage Id {StageId}:0x{Hash40Hex:X} could not be found the Stage DB", stageId, stageIdHex.Value);
                }
            }

            _logger.LogDebug("{NbrEntries} entries updated in Stage DB - db_root", entriesUpdated);

            t.Save(outputFilePath);

            return true;
        }

        public bool GenerateBgmPrcFile(List<BgmDbNewEntry> bgmEntries, string outputFilePath)
        {
            var t = new ParamFile();
            t.Open(_resourceService.GetBgmDbResource());

            //BGM PLAYLIST (QUICK & DIRTY)
            foreach (var bgmEntry in bgmEntries)
            {
                if (bgmEntry.Playlists == null)
                    continue;
                foreach (var playlist in bgmEntry.Playlists)
                {
                    var playlistId = playlist.Id;
                    if (!playlistId.StartsWith("bgm"))
                    {
                        _logger.LogWarning("The playlist_id for song '{Song}' must start with 'bgm', skipping...", bgmEntry.ToneName);
                        continue;
                    }

                    var hexValue = Hash40Util.StringToHash40(playlistId);
                    _logger.LogDebug("Playlist {PlaylistName}:0x{PlaylistId:X}", playlistId, hexValue);

                    ParamList bgmPlaylist = null;
                    ParamStruct newEntry = null;
                    //If the playlist doesn't exist...
                    if (!t.Root.Nodes.ContainsKey(hexValue))
                    {
                        _logger.LogDebug("Playlist {PlaylistName}:0x{PlaylistId:X} doesn't exist and will be created", playlistId, hexValue);

                        var playlistToClone = t.Root.Nodes[HEX_PLAYLIST_EXAMPLE] as ParamList;
                        bgmPlaylist = playlistToClone.Clone() as ParamList;

                        t.Root.Nodes.Add(hexValue, bgmPlaylist);
                        if (bgmPlaylist.Nodes.Count > 1)
                        {
                            bgmPlaylist.Nodes.RemoveRange(1, bgmPlaylist.Nodes.Count - 1);
                            newEntry = bgmPlaylist.Nodes[0] as ParamStruct;
                        }

                        _logger.LogDebug("Playlist {PlaylistName}:0x{PlaylistId:X} was created", playlistId, hexValue);
                    }
                    else
                    {
                        bgmPlaylist = t.Root.Nodes[hexValue] as ParamList;
                        newEntry = bgmPlaylist.Nodes[0].Clone() as ParamStruct;
                        bgmPlaylist.Nodes.Add(newEntry);
                    }

                    //Add song
                    _logger.LogDebug(LOG_NEW_BGM_ENTRY_PLAYLIST, playlistId, bgmEntry.UiBgmId, bgmPlaylist.Nodes.Count, 500);
                    SetNodeParamValue(newEntry, HEX_UI_BGM_ID, bgmEntry.UiBgmId);
                    for (int i = 0; i <= 15; i++)
                    {
                        SetNodeParamValue(newEntry, string.Format(HEX_ORDERNBR, i), (short)(bgmPlaylist.Nodes.Count));
                        SetNodeParamValue(newEntry, string.Format(HEX_INCIDENCENBR, i), (ushort)500);
                    }
                }
            }

            t.Save(outputFilePath);

            return true;
        }
    }*/
}
