using Avalonia.Controls;
using Sm5shMusic.GUI.ViewModels;
using System.Threading.Tasks;

namespace Sm5shMusic.GUI.Dialogs
{
    public class ModalDialog<W, VM, T> where W : Window, new() where T : ReactiveObjectBaseViewModel where VM : ModalBaseViewModel<T>
    {
        private VM _refVM;

        public ModalDialog(VM vm)
        {
            _refVM = vm;
        }

        public async Task<T> ShowDialog(Window rootWindow, T reactiveVM)
        {
            bool isEdit = reactiveVM != null;
            await Task.Delay(100); //Tends to prevent memory corruption?!

            var copy = reactiveVM?.GetCopy();
            _refVM.SelectedItem = (T)copy; //copy;
            var window = new W() { DataContext = _refVM };
            var result = await window.ShowDialog<W>(rootWindow);

            if(result != null)
            {
                return (T)_refVM.SelectedItem.SaveChanges();
            }
            return null;
        }
    }
}
