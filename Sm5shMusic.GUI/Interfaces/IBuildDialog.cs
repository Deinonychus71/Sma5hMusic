using System;
using System.Threading.Tasks;

namespace Sm5sh.GUI.Interfaces
{
    public interface IBuildDialog
    {
        void Init(Action<bool> callbackSuccess = null, Action<Exception> callbackError = null);
        Task Build(bool useCache, Action<bool> callbackSuccess = null, Action<Exception> callbackError = null);
    }
}
