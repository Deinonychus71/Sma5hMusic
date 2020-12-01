using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace Sm5sh.CLI
{
    public class WorkspaceManager : IWorkspaceManager
    {
        private readonly ILogger _logger;
        private readonly IOptions<Sm5shOptions> _config;

        public WorkspaceManager(IOptions<Sm5shOptions> config, ILogger<IWorkspaceManager> logger)
        {
            _logger = logger;
            _config = config;
        }

        public bool Init()
        {
            _logger.LogDebug("Initialize workspace");

            try
            {
                //Create workspace
                if (!Directory.Exists(_config.Value.OutputPath))
                {
                    _logger.LogDebug("Creating Working folder...");
                    Directory.CreateDirectory(_config.Value.OutputPath);
                }

                //Reset
                var existingFiles = Directory.GetFiles(_config.Value.OutputPath, "*", SearchOption.AllDirectories);
                if (existingFiles.Length > 0)
                {
                    if (!_config.Value.SkipOutputPathCleanupConfirmation)
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

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Workspace error");
                return false;
            }
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
