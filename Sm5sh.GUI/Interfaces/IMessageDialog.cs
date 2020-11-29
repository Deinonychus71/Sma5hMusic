using Avalonia.Controls;
using System.Threading.Tasks;

namespace Sm5sh.GUI.Interfaces
{
    public interface IMessageDialog
    {
        Task<bool> ShowWarningConfirm(string title, string message);
        Task ShowError(string title, string message);
        Task ShowInformation(string title, string message);
    }
}
