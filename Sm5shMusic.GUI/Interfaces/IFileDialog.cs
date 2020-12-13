using Avalonia.Controls;
using System.Threading.Tasks;

namespace Sm5shMusic.GUI.Interfaces
{
    public interface IFileDialog
    {
        Task<string[]> OpenFileDialogAudio(Window parent = null);
        Task<string> OpenFolderDialog(Window parent = null);
    }
}
