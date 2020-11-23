using Avalonia.Controls;
using System.Threading.Tasks;

namespace Sm5sh.GUI.Interfaces
{
    public interface IDialogWindow
    {
        void Show(Window parent);
        void Show();
        Task<TResult> ShowDialog<TResult>(Window owner);
        Task ShowDialog(Window owner);
    }
}
