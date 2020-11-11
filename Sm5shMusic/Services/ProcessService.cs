using Microsoft.Extensions.Logging;
using Sm5shMusic.Interfaces;
using System;
using System.Diagnostics;

namespace Sm5shMusic.Services
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
            _logger.LogDebug("Launching {Executable} with arguments {Arguments}", executablePath, arguments);

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
                process.StartInfo.RedirectStandardOutput = standardRedirect != null;
                process.OutputDataReceived += (sender, data) => {
                    standardRedirect?.Invoke(sender, data);
                };
                process.Start();
                process.WaitForExit();
            }
        }
    }
}
