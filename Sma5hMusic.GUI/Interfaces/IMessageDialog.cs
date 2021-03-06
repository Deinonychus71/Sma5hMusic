using System;
using System.Threading.Tasks;

namespace Sma5hMusic.GUI.Interfaces
{
    public interface IMessageDialog
    {
        Task<bool> ShowWarningConfirm(string title, string message);
        Task ShowError(string title, string message, Exception e = null);
        Task ShowInformation(string title, string message);
        Task<string> PromptInput(string title, string message);
    }
}
