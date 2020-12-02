using System.Threading.Tasks;

namespace Sm5shMusic.GUI.Interfaces
{
    public interface IMessageDialog
    {
        Task<bool> ShowWarningConfirm(string title, string message);
        Task ShowError(string title, string message);
        Task ShowInformation(string title, string message);
        Task<string> PromptInput(string title, string message);
    }
}
