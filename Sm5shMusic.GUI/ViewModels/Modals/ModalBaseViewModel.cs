using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;
using System.Reactive;

namespace Sm5shMusic.GUI.ViewModels
{
    public class ModalBaseViewModel<T> : ReactiveValidationObject where T : ReactiveObjectBaseViewModel
    {
        protected T _refSelectedItem;
        public ReactiveCommand<Window, Unit> ActionCancel { get; }
        public ReactiveCommand<Window, Unit> ActionSave { get; }

        public T SelectedItem
        {
            get => _refSelectedItem;
            set
            {
                _refSelectedItem = value;
                LoadItem();
                this.RaiseAndSetIfChanged(ref _refSelectedItem, value);
            }
        }

        public ModalBaseViewModel()
        {
            var canExecute = this.WhenAnyValue(x => x.ValidationContext.IsValid);
            ActionCancel = ReactiveCommand.Create<Window>(CancelChanges);
            ActionSave = ReactiveCommand.Create<Window>(SaveChanges, canExecute);
        }

        protected virtual void LoadItem() { }
        protected virtual void SaveChanges() { }

        private void CancelChanges(Window w)
        {
            w.Close();
        }

        private void SaveChanges(Window window)
        {
            SaveChanges();
            window.Close(window);
        }
    }
}
