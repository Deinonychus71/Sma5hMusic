using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sm5shMusic.GUI.Interfaces;
using Sm5sh.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using Sm5sh;
using System.Collections.Generic;

namespace Sm5shMusic.GUI.Dialogs
{
    public class BuildDialog : IBuildDialog
    {
        private readonly ILogger _logger;
        private readonly IDialogWindow _rootDialogWindow;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageDialog _messageDialog;
        private readonly IStateManager _stateManager;
        private readonly IOptions<Sm5shOptions> _config;

        public BuildDialog(IOptions<Sm5shOptions> config, IServiceProvider serviceProvider, IStateManager stateManager,
            IDialogWindow rootDialogWindow, IMessageDialog messageDialog, ILogger<BuildDialog> logger)
        {
            _logger = logger;
            _config = config;
            _rootDialogWindow = rootDialogWindow;
            _serviceProvider = serviceProvider;
            _stateManager = stateManager;
            _messageDialog = messageDialog;
        }

        public async Task Init(Action<bool> callbackSuccess = null, Action<Exception> callbackError = null)
        {
            try
            {
                //Check required files
                foreach (var fileCheck in GetRequiredFiles())
                {
                    if (!File.Exists(fileCheck))
                    {
                        await Dispatcher.UIThread.InvokeAsync(async() =>
                        {
                            await _messageDialog.ShowError("File Check", $"The file {fileCheck} is required and was not found.\r\nPlease check your setup.");
                            callbackError?.Invoke(new Exception("File Check Exception"));
                        }, DispatcherPriority.Background);
                        return;
                    }
                }

                //Check locale
                var messagePath = Path.Combine(_config.Value.GameResourcesPath, "ui", "message");
                Directory.CreateDirectory(messagePath);
                var localeCheckMsgBgm = Directory.GetFiles(Path.Combine(_config.Value.GameResourcesPath, "ui", "message"), "msg_bgm*.msbt", SearchOption.TopDirectoryOnly);
                var localeCheckMsgTitle = Directory.GetFiles(Path.Combine(_config.Value.GameResourcesPath, "ui", "message"), "msg_title*.msbt", SearchOption.TopDirectoryOnly);
                if (localeCheckMsgBgm.Length == 0 || localeCheckMsgTitle.Length == 0)
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await _messageDialog.ShowError("File Check", $"There should be at least one localized msg_bgm.msbt and one msg_title.msbt\r\nresource in {Path.Combine(_config.Value.GameResourcesPath, "ui", "message")}.");
                        callbackError?.Invoke(new Exception("File Check Exception"));
                    }, DispatcherPriority.Background);
                    return;
                }

                _ = Task.Run(() =>
                {
                    try
                    {
                        //Init/Reload StateManager
                        _stateManager.UnloadResources();
                        _stateManager.Init();

                        //Load
                        var mods = _serviceProvider.GetServices<ISm5shMod>();
                        foreach (var mod in mods)
                        {
                            mod.Init();
                        }

                        callbackSuccess?.Invoke(true);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e.Message);
                        callbackError?.Invoke(e);
                    }
                });
            }
            catch (Exception e)
            {
                await Dispatcher.UIThread.InvokeAsync(async() =>
                {
                    await _messageDialog.ShowError("Initialization failure", $"There was a general exception during Init.\r\n{e.Message}");
                    callbackError?.Invoke(new Exception("Initialization failure"));
                }, DispatcherPriority.Background);
            }
        }

        public async Task Build(bool useCache, Action<bool> callbackSuccess = null, Action<Exception> callbackError = null)
        {
            if (!await EnsureArcOutputIsClean())
            {
                callbackSuccess?.Invoke(false);
                return;
            }

            _ = Init(async (o) =>
            {
                if (!o)
                {
                    await Dispatcher.UIThread.InvokeAsync(async() =>
                    {
                        await _messageDialog.ShowError("Build", "Could not initialize the build.");
                        callbackError?.Invoke(new Exception("Mod Init Exception"));
                    }, DispatcherPriority.Background);
                }

                //Check if ArcOutput is empty
                _ = Task.Run(async () =>
                {
                    try
                    {
                        //Load
                        var mods = _serviceProvider.GetServices<ISm5shMod>();
                        foreach (var mod in mods)
                        {
                            if (!mod.Build(useCache))
                            {
                                await ShowBuildFailedError();
                                callbackError?.Invoke(new Exception("Mod Build Exception"));
                                return;
                            }
                        }

                        if (!_stateManager.WriteChanges())
                        {
                            await ShowBuildFailedError();
                            callbackError?.Invoke(new Exception("StateManager Exception"));
                            return;
                        }
                        _logger.LogInformation(" Build Complete");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e.Message);
                        callbackError?.Invoke(e);
                    }

                    await Dispatcher.UIThread.InvokeAsync(async() =>
                    {
                        await _messageDialog.ShowInformation("Complete", "Build complete. If something goes wrong, please check the logs for error.");

                        callbackSuccess?.Invoke(true);

                    }, DispatcherPriority.Background);
                });
            });
        }

        public async Task<bool> EnsureArcOutputIsClean()
        {
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
                        _logger.LogWarning("Files found in the workspace folder.");
                        if (await _messageDialog.ShowWarningConfirm("Clean Output folder?", $"Your folder {_config.Value.OutputPath} must be empty before building the mod.\r\nProceed?"))
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
            }
            catch(Exception e)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowInformation("Cleanup Error", $"Error while cleaning the output folder\r\n{e.Message}.");
                }, DispatcherPriority.Background);
                return false;
            }
            return true;
        }

        private void CleaningWorkspaceFolder(string[] existingFiles)
        {
            _logger.LogInformation("Cleaning up workspace folder");
            foreach (var fileToDelete in existingFiles)
            {
                File.Delete(fileToDelete);
            }
        }

        private async Task ShowBuildFailedError()
        {
            await Dispatcher.UIThread.InvokeAsync(async() =>
            {
                await _messageDialog.ShowError("Failed", "Build failed. Errors happened while writing the resource files. Please check the logs.");

            }, DispatcherPriority.Background);
        }

        private List<string> GetRequiredFiles()
        {
            var config = _config.Value;

            var requiredFiles = new List<string>()
            {
                Path.Combine(config.ResourcesPath, "template.nus3bank"),
                Path.Combine(config.ResourcesPath, "param_labels.csv"),
                Path.Combine(config.ResourcesPath, "nusbank_ids.csv"),
                Path.Combine(config.GameResourcesPath, "sound", "config", "bgm_property.bin"),
                Path.Combine(config.GameResourcesPath, "ui", "param", "database", "ui_bgm_db.prc"),
                Path.Combine(config.GameResourcesPath, "ui", "param", "database", "ui_gametitle_db.prc"),
                Path.Combine(config.GameResourcesPath, "ui", "param", "database", "ui_stage_db.prc"),
                Path.Combine(config.ToolsPath, "VGAudioCli.exe"),
                Path.Combine(config.ToolsPath, "paracobNET.dll"),
                Path.Combine(config.ToolsPath, "MsbtEditor.dll"),
                Path.Combine(config.ToolsPath, "BgmProperty", "bgm_hashes.txt"),
                Path.Combine(config.ToolsPath, "BgmProperty", "bgm-property.exe"),
                Path.Combine(config.ToolsPath, "Nus3Audio", "nus3audio.exe"),
            };

            return requiredFiles;
        }
    }
}
