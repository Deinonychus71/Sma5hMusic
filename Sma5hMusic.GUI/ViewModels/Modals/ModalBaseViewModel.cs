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
            ActionCancel = ReactiveCommand.CreateFromTask<Window>(CancelChanges);
            ActionOK = ReactiveCommand.CreateFromTask<Window>(SaveChanges, canExecute);
        }

        protected virtual IObservable<bool> GetValidationRule()
        {
            return this.WhenAnyValue(x => x.ValidationContext.IsValid);
        }

        protected virtual void LoadItem(T item) { }
        protected virtual Task<bool> SaveChanges() { return Task.FromResult(true); }
        protected virtual Task CancelChanges() { return Task.CompletedTask; }

        private async Task CancelChanges(Window w)
        {
            await CancelChanges();
            w.Close();
        }

        private async Task SaveChanges(Window window)
        {
            if (await SaveChanges())
                window.Close(window);
        }
    }
}
