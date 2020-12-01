using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sm5sh.GUI.Interfaces;
using Sm5sh.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sm5sh.GUI.Dialogs
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

        public void Init(Action<bool> callbackSuccess = null, Action<Exception> callbackError = null)
        {
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

        public async Task Build(bool useCache, Action<bool> callbackSuccess = null, Action<Exception> callbackError = null)
        {
            if (!await EnsureArcOutputIsClean())
            {
                callbackSuccess?.Invoke(false);
                return;
            }

            Init(async (o) =>
            {
                if (!o)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        _messageDialog.ShowError("Build", "Could not initialize the build.");
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
                            if(!mod.Build(useCache))
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

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        _messageDialog.ShowInformation("Complete", "Build complete. If something goes wrong, please check the logs for error.");

                        callbackSuccess?.Invoke(true);

                    }, DispatcherPriority.Background);
                });
            });
        }

        public async Task<bool> EnsureArcOutputIsClean()
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
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _messageDialog.ShowError("Failed", "Build failed. Errors happened while writing the resource files. Please check the logs.");

            }, DispatcherPriority.Background);
        }
    }
}
