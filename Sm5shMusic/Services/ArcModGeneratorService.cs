using Microsoft.Extensions.Logging;
using Sm5shMusic.Helpers;
using Sm5shMusic.Interfaces;
using Sm5shMusic.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sm5shMusic.Services
{
    public class ArcModGeneratorService : IArcModGeneratorService
    {
        private readonly IResourceService _resourceService;
        private readonly IParacobService _paracobService;
        private readonly INus3AudioService _nus3AudioService;
        private readonly IAudioMetadataService _audioMetadataService;
        private readonly IBgmPropertyService _bgmPropertyService;
        private readonly IMsbtService _msbtService;
        private readonly IWorkspaceManager _workspace;
        private readonly ILogger _logger;

        public ArcModGeneratorService(IResourceService resourceService, IParacobService paracobService, INus3AudioService nus3AudioService, IAudioMetadataService audioMetadataService,
            IMsbtService msbtService, IBgmPropertyService bgmPropertyService, IWorkspaceManager workspace, ILogger<IArcModGeneratorService> logger)
        {
            _logger = logger;
            _resourceService = resourceService;
            _workspace = workspace;
            _nus3AudioService = nus3AudioService;
            _audioMetadataService = audioMetadataService;
            _msbtService = msbtService;
            _bgmPropertyService = bgmPropertyService;
            _paracobService = paracobService;
        }

        public bool GenerateArcMusicMod(List<MusicModBgmEntry> bgmEntries)
        {
            if(bgmEntries == null || bgmEntries.Count == 0)
            {
                _logger.LogError("No Music Mod BGM Entry.");
                return false;
            }

            //Prefix
            if (!ValidateUniqueToneNames(bgmEntries))
            {
                _logger.LogError("The Arc Music Mod Generation failed. At least two songs have the same tone name.");
                return false;
            }

            _logger.LogDebug("Starting Arc Music Mod generation.");

            //Get new Game Titles
            var coreGameTitleEntries = _paracobService.GetCoreDbRootGameTitleEntries();
            var coreGameTitleIds = coreGameTitleEntries.Select(p => p.UiGameTitleId).Distinct().ToList();
            var newGameTitleIds = bgmEntries.Where(p => !coreGameTitleIds.Contains(p.Game.Id));

            //Generate NUS3AUDIO and NUS3BANK
            _logger.LogInformation("Generate/Copy Nus3Audio and Nus3Bank - {NbrFiles} files", bgmEntries.Count * 2);
            var nusBankTemplate = _resourceService.GetNusBankTemplateResource();
            foreach (var bgmEntry in bgmEntries)
            {
                var nusBankOutputFile = _workspace.GetWorkspaceOutputForNus3Bank(bgmEntry.InternalToneName);
                var audioInputFile = bgmEntry.AudioFilePath;
                var nusAudioOutputFile = _workspace.GetWorkspaceOutputForNus3Audio(bgmEntry.InternalToneName);

                //We always generate a new Nus3Bank as the internal ID might change
                _nus3AudioService.GenerateNus3Bank(bgmEntry.InternalToneName, nusBankTemplate, nusBankOutputFile);

                //Test for audio cache
                if (_workspace.IsAudioCacheEnabled)
                {
                    var cachedAudioFile = _workspace.GetCacheForNus3Audio(bgmEntry.InternalToneName);
                    if (!File.Exists(cachedAudioFile))
                    {
                        GenerateNus3Audio(bgmEntry.InternalToneName, audioInputFile, cachedAudioFile);
                    }
                    else
                    {
                        _logger.LogDebug("Retrieving nus3audio {InternalToneName} from cache {CacheFile}", bgmEntry.InternalToneName, cachedAudioFile);
                    }
                    _logger.LogDebug("Copy nus3audio {InternalToneName} from cache {CacheFile} to {Nus3AudioOutputFile}", bgmEntry.InternalToneName, cachedAudioFile, nusAudioOutputFile);
                    File.Copy(cachedAudioFile, nusAudioOutputFile);
                }
                else
                {
                    GenerateNus3Audio(bgmEntry.InternalToneName, audioInputFile, nusAudioOutputFile);
                }
            }

            //Generate PRC UI Title
            var newGameTitleDbEntries = newGameTitleIds.Select(p => new GameTitleDbNewEntry()
            {
                GameTitleId = p.Game.Id,
                SeriesId = p.Game.SeriesId
            }).GroupBy(p => p.NameId).Select(p => p.First()).ToList();
            _logger.LogInformation("Generate Game Title DB - {Entries} new entries", newGameTitleDbEntries.Count);
            _paracobService.GenerateGameTitlePrcFile(newGameTitleDbEntries, _workspace.GetWorkspaceOutputForUiGameTitleDbFile());

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

            //Generate BGM_Property
            _logger.LogInformation("Generate BGM Property - {Entries} new entries", bgmEntries.Count);
            _bgmPropertyService.GenerateBgmProperty(bgmEntries);

            //Generate MSBT Title Files
            if (newGameTitleIds != null && newGameTitleIds.Count() > 0)
            {
                foreach (var locale in LocaleHelper.ValidLocales)
                {
                    var newMsbtGameTitles = newGameTitleIds.Select(p => new MsbtNewEntryModel()
                    {
                        Label = $"{Constants.InternalIds.MsbtTitPrefix}{p.Game.Id}",
                        Value = p.Game.Title.ContainsKey(locale) ? p.Game.Title[locale] : p.Game.Title.ContainsKey(Constants.DefaultLocale) ? p.Game.Title[Constants.DefaultLocale] : "MISSING"
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
                    Value = p.Song.Title.ContainsKey(locale) ? p.Song.Title[locale] : p.Song.Title.ContainsKey(Constants.DefaultLocale) ? p.Song.Title[Constants.DefaultLocale] : "MISSING"
                }).ToList();
                var newMsbtAuthorBgms = bgmEntries.Select(p => new MsbtNewEntryModel()
                {
                    Label = $"{Constants.InternalIds.MsbtBgmAuthorPrefix}{p.NameId}",
                    Value = p.Song.Author.ContainsKey(locale) ? p.Song.Author[locale] : p.Song.Author.ContainsKey(Constants.DefaultLocale) ? p.Song.Author[Constants.DefaultLocale] : "MISSING"
                });
                var newMsbtCopyrightBgms = bgmEntries.Select(p => new MsbtNewEntryModel()
                {
                    Label = $"{Constants.InternalIds.MsbtBgmCopyrightPrefix}{p.NameId}",
                    Value = p.Song.Copyright.ContainsKey(locale) ? p.Song.Copyright[locale] : p.Song.Copyright.ContainsKey(Constants.DefaultLocale) ? p.Song.Copyright[Constants.DefaultLocale] : "MISSING"
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

        private bool GenerateNus3Audio(string toneId, string audioInputFile, string nusAudioOutputFile)
        {
            //Check if conversion if necessary
            bool needConversion = false;
            if (Constants.ExtensionsNeedConversion.Contains(Path.GetExtension(audioInputFile).ToLower()))
            {
                _audioMetadataService.ConvertAudio(audioInputFile, _resourceService.GetTemporaryAudioConversionFile());
                audioInputFile = _resourceService.GetTemporaryAudioConversionFile();
                needConversion = true;
            }

            var result = _nus3AudioService.GenerateNus3Audio(toneId, audioInputFile, nusAudioOutputFile);

            if (needConversion)
            {
                File.Delete(_resourceService.GetTemporaryAudioConversionFile());
            }

            return result;
        }

        private bool ValidateUniqueToneNames(List<MusicModBgmEntry> bgmEntries)
        {
            var coreBgmEntries = _paracobService.GetCoreDbRootBgmEntries();
            var tones = coreBgmEntries.Select(p => p.UiBgmId.Replace(Constants.InternalIds.BgmIdPrefix, string.Empty)).ToList();
            foreach(var musicMod in bgmEntries)
            {
                tones.Add(musicMod.InternalToneName);
            }
            return tones.Count == tones.Distinct().Count();
        }
    }
}
