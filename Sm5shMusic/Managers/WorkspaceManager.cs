using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sm5shMusic.Helpers;
using Sm5shMusic.Interfaces;
using System;
using System.IO;

namespace Sm5shMusic.Managers
{
    public class WorkspaceManager : IWorkspaceManager
    {
        private readonly Settings _settings;
        private readonly ILogger _logger;

        public bool IsAudioCacheEnabled { get; private set; }

        public WorkspaceManager(IOptions<Settings> settings, ILogger<IWorkspaceManager> logger)
        {
            _settings = settings.Value;
            _logger = logger;
            IsAudioCacheEnabled = settings.Value.EnableAudioCaching;
        }

        public bool Init()
        {
            _logger.LogDebug("Initialize workspace");

            try
            {
                //Create workspace
                if (!Directory.Exists(_settings.WorkspacePath))
                {
                    _logger.LogDebug("Creating Working folder...");
                    Directory.CreateDirectory(_settings.WorkspacePath);
                }

                //Reset
                var existingFiles = Directory.GetFiles(_settings.WorkspacePath, "*", SearchOption.AllDirectories);
                if (existingFiles.Length > 0)
                {
                    if (!_settings.SkipWorkspaceCleanupConfirmation)
                    {
                        _logger.LogWarning("Files found in the workspace folder, delete? Y/N (Default: N)");
                        var response = Console.ReadKey();
                        if (response.KeyChar == 'y' || response.KeyChar == 'Y')
                        {
                            CleaningWorkspaceFolder(existingFiles);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        CleaningWorkspaceFolder(existingFiles);
                    }
                }

                //Create folder
                Directory.CreateDirectory(GetWorkspaceOutputForNus3Audio());
                Directory.CreateDirectory(GetWorkspaceOutputForUiDb());
                Directory.CreateDirectory(GetWorkspaceOutputForUiMessage());
                Directory.CreateDirectory(GetWorkspaceOutputForSoundConfig());
                if(IsAudioCacheEnabled)
                    Directory.CreateDirectory(GetAudioCacheDirectory());

                return true;
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Workspace error");
                return false;
            }
        }

        public string GetWorkspaceDirectory()
        {
            return _settings.WorkspacePath;
        }

        public string GetCacheDirectory()
        {
            return _settings.CachePath;
        }

        public string GetAudioCacheDirectory()
        {
            return Path.Combine(_settings.CachePath, Constants.WorkspacePaths.AudioCachePath);
        }

        public string GetCacheForNus3Audio(string toneName)
        {
            return Path.Combine(GetAudioCacheDirectory(), string.Format(Constants.WorkspacePaths.WorkspaceNus3AudioFile, toneName));
        }

        public string GetWorkspaceOutputForNus3Audio()
        {
            return Path.Combine(_settings.WorkspacePath, Constants.WorkspacePaths.WorkspaceBgmStream, Constants.WorkspacePaths.WorkspaceBgmStreamSound, Constants.WorkspacePaths.WorkspaceBgmStreamSoundBgm);
        }

        public string GetWorkspaceOutputForNus3Audio(string toneName)
        {
            return Path.Combine(GetWorkspaceOutputForNus3Audio(), string.Format(Constants.WorkspacePaths.WorkspaceNus3AudioFile, toneName));
        }

        public string GetWorkspaceOutputForNus3Bank(string toneName)
        {
            return Path.Combine(GetWorkspaceOutputForNus3Audio(), string.Format(Constants.WorkspacePaths.WorkspaceNus3BankFile, toneName));
        }

        public string GetWorkspaceOutputForUi()
        {
            return Path.Combine(_settings.WorkspacePath, Constants.WorkspacePaths.WorkspaceUi);
        }

        public string GetWorkspaceOutputForUiMessage()
        {
            return Path.Combine(GetWorkspaceOutputForUi(), Constants.WorkspacePaths.WorkspaceUiMessage);
        }

        public string GetWorkspaceOutputForUiDb()
        {
            return Path.Combine(GetWorkspaceOutputForUi(), Constants.WorkspacePaths.WorkspaceUiParam, Constants.WorkspacePaths.WorkspaceUiParamDatabase);
        }

        public string GetWorkspaceOutputForSoundConfig()
        {
            return Path.Combine(_settings.WorkspacePath, Constants.WorkspacePaths.WorkspaceSound, Constants.WorkspacePaths.WorkspaceSoundConfig);
        }

        public string GetWorkspaceOutputForUiGameTitleDbFile()
        {
            return Path.Combine(GetWorkspaceOutputForUiDb(), Constants.WorkspacePaths.WorkspaceUiGameTitleDb);
        }

        public string GetWorkspaceOutputForUiBgmDbFile()
        {
            return Path.Combine(GetWorkspaceOutputForUiDb(), Constants.WorkspacePaths.WorkspaceUiBgmDb);
        }

        public string GetWorkspaceOutputForBgmPropertyFile()
        {
            return Path.Combine(GetWorkspaceOutputForSoundConfig(), Constants.WorkspacePaths.WorkspaceBgmPropertyFile);
        }

        public string GetWorkspaceOutputForMsbtTitleResource(string locale)
        {
            return Path.Combine(GetWorkspaceOutputForUiMessage(), string.Format(Constants.ResourcesFiles.MsbtTitleFile, locale));
        }

        public string GetWorkspaceOutputForMsbtBgmResource(string locale)
        {
            return Path.Combine(GetWorkspaceOutputForUiMessage(), string.Format(Constants.ResourcesFiles.MsbtBgmFile, locale));
        }

        private void CleaningWorkspaceFolder(string[] existingFiles)
        {
            _logger.LogInformation("Cleaning up workspace folder");
            foreach (var fileToDelete in existingFiles)
            {
                File.Delete(fileToDelete);
            }
        }
    }
}
