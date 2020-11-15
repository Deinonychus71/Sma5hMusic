namespace Sm5shMusic.Services
{
    /*public class ArcModGeneratorService : IArcModGeneratorService
    {
        public bool GenerateArcMusicMod(List<MusicModBgmEntry> bgmEntries)
        {
            //Generate PRC UI BGM
            var newBgmEntries = bgmEntries.Select(p => new BgmDbNewEntry()
            {
                ToneName = p.InternalToneName,
                Rarity = p.Song.Rarity,
                RecordType = $"{Constants.InternalIds.RecordTypePrefix}{p.Song.RecordType}",
                GameTitleId = p.Game.Id,
                NameId = p.NameId,
                Playlists = p.Song.Playlists.Select(p => new BgmPlaylistEntry() { Id = $"{Constants.InternalIds.PlaylistPrefix}{p.Id}" }).ToList(),
                IsDlc = p.Song.Playlists.Any(p => Constants.DLCStages.Contains(p.Id)),
                IsPatch = p.Song.Playlists.Any(p => Constants.DLCStages.Contains(p.Id))
            }).ToList();
            _logger.LogInformation("Generate BGM DB - {Entries} new entries", newBgmEntries.Count);
            _paracobService.GenerateBgmPrcFile(newBgmEntries, _workspace.GetWorkspaceOutputForUiBgmDbFile());

            //Generate MSBT Title Files
            if (newGameTitleIds != null && newGameTitleIds.Count() > 0)
            {
                foreach (var locale in LocaleHelper.ValidLocales)
                {
                    var newMsbtGameTitles = newGameTitleIds.Select(p => new MsbtNewEntryModel()
                    {
                        Label = $"{Constants.InternalIds.MsbtTitPrefix}{p.Game.Id}",
                        Value = 
                        p.Game.Title.ContainsKey(locale) && !string.IsNullOrEmpty(p.Game.Title[locale]) ? p.Game.Title[locale] : 
                        p.Game.Title.ContainsKey(Constants.DefaultLocale) && !string.IsNullOrEmpty(p.Game.Title[Constants.DefaultLocale]) ? p.Game.Title[Constants.DefaultLocale] : "MISSING"
                    }).GroupBy(p => p.Label).Select(p => p.First()).ToList();
                    var inputMsbtFile = _resourceService.GetMsbtTitleResource(locale);
                    if (File.Exists(inputMsbtFile))
                    {
                        _logger.LogInformation("Generate MSBT GameTitle - {Entries} new entries - {Path}", newMsbtGameTitles.Count, inputMsbtFile);
                        var outputMsbtFile = _workspace.GetWorkspaceOutputForMsbtTitleResource(locale);
                        _msbtService.GenerateNewEntries(newMsbtGameTitles, inputMsbtFile, outputMsbtFile);
                    }
                }
            }

            //Generate MSBT BGM Files
            foreach (var locale in LocaleHelper.ValidLocales)
            {
                var newMsbtTitleBgms = bgmEntries.Select(p => new MsbtNewEntryModel()
                {
                    Label = $"{Constants.InternalIds.MsbtBgmTitlePrefix}{p.NameId}",
                    Value = 
                    p.Song.Title.ContainsKey(locale) && !string.IsNullOrEmpty(p.Song.Title[locale]) ? p.Song.Title[locale] : 
                    p.Song.Title.ContainsKey(Constants.DefaultLocale) && !string.IsNullOrEmpty(p.Song.Title[Constants.DefaultLocale]) ? p.Song.Title[Constants.DefaultLocale] : "MISSING"
                }).ToList();
                var newMsbtAuthorBgms = bgmEntries.Where(p => p.Song.Author != null && p.Song.Author.ContainsKey(locale) && !string.IsNullOrEmpty(p.Song.Author[locale])).Select(p => new MsbtNewEntryModel()
                {
                    Label = $"{Constants.InternalIds.MsbtBgmAuthorPrefix}{p.NameId}",
                    Value = p.Song.Author[locale]
                });
                var newMsbtCopyrightBgms = bgmEntries.Where(p => p.Song.Copyright != null && p.Song.Copyright.ContainsKey(locale) && !string.IsNullOrEmpty(p.Song.Copyright[locale])).Select(p => new MsbtNewEntryModel()
                {
                    Label = $"{Constants.InternalIds.MsbtBgmCopyrightPrefix}{p.NameId}",
                    Value = p.Song.Copyright[locale]
                });
                var newMsbtBgms = newMsbtTitleBgms;
                newMsbtBgms.AddRange(newMsbtAuthorBgms);
                newMsbtBgms.AddRange(newMsbtCopyrightBgms);
                var inputMsbtFile = _resourceService.GetMsbtBgmResource(locale);
                if (File.Exists(inputMsbtFile))
                {
                    _logger.LogInformation("Generate MSBT BGM - {Entries} new entries - {Path}", newMsbtBgms.Count, inputMsbtFile);
                    var outputMsbtFile = _workspace.GetWorkspaceOutputForMsbtBgmResource(locale);
                    _msbtService.GenerateNewEntries(newMsbtBgms, inputMsbtFile, outputMsbtFile);
                }
            }

            _logger.LogInformation("Output Folder: {OutputFolder}", _workspace.GetWorkspaceDirectory());

            return true;
        }

        public bool GenerateArcStagePlaylistMod(List<MusicModBgmEntry> bgmEntries, List<StagePlaylistModConfig> stagePlaylistEntries)
        {
            _logger.LogDebug("Starting Arc Stage Playlist Mod generation.");

            var corePlaylistEntries = _paracobService.GetCoreDbRootPlaylists();
            corePlaylistEntries.AddRange(bgmEntries.SelectMany(p => p.Song.Playlists.Select(p2 => $"{Constants.InternalIds.PlaylistPrefix}{p2.Id}")));
            var validPlaylistEntries = corePlaylistEntries.Distinct().ToList();

            var stagePlaylistDbEntries = stagePlaylistEntries.Select(p => new StageDbEntry()
            {
                UiStageId = $"{Constants.InternalIds.StageIdPrefix}{p.StageId}",
                BgmSetId = $"{Constants.InternalIds.PlaylistPrefix}{p.PlaylistId}",
                BgmSettingNo = (byte)p.OrderId
            }).ToList();

            //Validate that all new playlists are registered
            if(stagePlaylistDbEntries.Any(p => !validPlaylistEntries.Contains(p.BgmSetId)))
            {
                _logger.LogError("The Arc Stage Playlist Mod Generation failed. At least one playlist is not registered in the BGM DB.");
                return false;
            }

            _paracobService.UpdateStagePrcFile(stagePlaylistDbEntries, _workspace.GetWorkspaceOutputForUiStageDbFile());

            _logger.LogInformation("Output Folder: {OutputFolder}", _workspace.GetWorkspaceDirectory());

            return true;
        }
    }*/
}
