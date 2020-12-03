using Microsoft.Extensions.Logging;
using Sm5sh.Interfaces;
using System;
using System.Diagnostics;

namespace Sm5sh
{
    public class ProcessService : IProcessService
    {
        private readonly ILogger<IProcessService> _logger;

        public ProcessService(ILogger<IProcessService> logger)
        {
            _logger = logger;
        }

        public void RunProcess(string executablePath, string arguments, Action<object, DataReceivedEventArgs> standardRedirect = null, Action<object, DataReceivedEventArgs> errorRedirect = null)
        {
            _logger.LogDebug($"Launching {executablePath} {arguments}");

            using (var process = new Process())
            {
                void onError(object sender, DataReceivedEventArgs data)
                {
                    if (data != null && data.Data != null)
                    {
                        _logger.LogError("Error while running {Executable} with arguments {Arguments} - {Error}", executablePath, arguments, data.Data);
                        errorRedirect?.Invoke(sender, data);
                    }
                }
                void onInfo(object sender, DataReceivedEventArgs data)
                {
                    if (data != null && data.Data != null)
                    {
                        _logger.LogDebug("{Executable}: {Data}", executablePath, data.Data);
                        standardRedirect?.Invoke(sender, data);
                    }
                }
                process.StartInfo.FileName = executablePath;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardError = true;
                process.ErrorDataReceived += onError;
                process.StartInfo.RedirectStandardOutput = true;
                process.OutputDataReceived += onInfo;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                process.CancelOutputRead();
                process.CancelErrorRead();
                process.ErrorDataReceived -= onError;
                process.OutputDataReceived -= onInfo;
                process.Close();
            }
        }

        private void ReadOnErrorReceived(object sender, DataReceivedEventArgs args) { 
        }
    }
}
