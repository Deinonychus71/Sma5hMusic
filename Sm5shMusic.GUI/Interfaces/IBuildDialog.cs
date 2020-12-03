using System;
using System.Threading.Tasks;

namespace Sm5shMusic.GUI.Interfaces
{
    public interface IBuildDialog
    {
        Task Init(Action<bool> callbackSuccess = null, Action<Exception> callbackError = null);
        Task Build(bool useCache, Action<bool> callbackSuccess = null, Action<Exception> callbackError = null);
    }
}
