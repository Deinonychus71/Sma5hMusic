using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace Sma5hMusic.GUI.ViewModels
{
    public class ModalBaseViewModel<T> : ReactiveValidationObject where T : ReactiveObjectBaseViewModel
    {
        protected T _refSelectedItem;
        public ReactiveCommand<Window, Unit> ActionCancel { get; }
        public ReactiveCommand<Window, Unit> ActionOK { get; }

        public T SelectedItem
        {
            get => _refSelectedItem;
            set
            {
                LoadItem(value);
                this.RaiseAndSetIfChanged(ref _refSelectedItem, value);
            }
        }

        public ModalBaseViewModel()
        {
            var canExecute = GetValidationRule();
            ActionCancel = ReactiveCommand.Create<Window>(CancelChanges);
            ActionOK = ReactiveCommand.CreateFromTask<Window>(SaveChanges, canExecute);
        }

        protected virtual IObservable<bool> GetValidationRule()
        {
            return this.WhenAnyValue(x => x.ValidationContext.IsValid);
        }

        protected virtual void LoadItem(T item) { }
        protected virtual Task<bool> SaveChanges() { return Task.FromResult(true); }

        private void CancelChanges(Window w)
        {
            w.Close();
        }

        private async Task SaveChanges(Window window)
        {
            if (await SaveChanges())
                window.Close(window);
        }
    }
}
