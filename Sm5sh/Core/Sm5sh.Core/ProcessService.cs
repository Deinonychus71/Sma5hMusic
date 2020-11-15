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
                process.StartInfo.FileName = executablePath;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardError = true;
                process.ErrorDataReceived += (sender, data) => {
                    _logger.LogError("Error while running {Executable} with arguments {Arguments} - {Error}", executablePath, arguments, data.Data);
                    errorRedirect?.Invoke(sender, data);
                };
                process.StartInfo.RedirectStandardOutput = true;
                process.OutputDataReceived += (sender, data) => {
                    _logger.LogDebug("{Executable}: {Data}", executablePath, data.Data);
                    standardRedirect?.Invoke(sender, data);
                };
                process.Start();
                process.WaitForExit();
            }
        }
    }
}
