using Avalonia.Controls;
using System.Threading.Tasks;

namespace Sma5hMusic.GUI.Interfaces
{
    public interface IFileDialog
    {
        Task<string[]> OpenFileDialogAudioMultiple(Window parent = null);
        Task<string> OpenFileDialogAudioSingle(Window parent = null);
        Task<string> OpenFolderDialog(Window parent = null);
    }
}
