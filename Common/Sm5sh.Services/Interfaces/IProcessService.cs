using System;
using System.Diagnostics;

namespace Sm5sh.Services.Interfaces
{
    public interface IProcessService
    {
        void RunProcess(string executablePath, string arguments, Action<object, DataReceivedEventArgs> standardRedirect = null, Action<object, DataReceivedEventArgs> errorRedirect = null);
    }
}