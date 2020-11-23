using Avalonia.Controls;
using System.Threading.Tasks;

namespace Sm5sh.GUI.Interfaces
{
    public interface IFileDialog
    {
        Task<string[]> OpenFileDialogAudio(Window parent = null);
    }
}
