using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;


namespace Sma5hMusic.GUI.ViewModels
{
    public class IncidencePickerModalWindowViewModel : ViewModelBase
    {
        [Reactive]
        public ushort Incidence { get; set; }
        public ReactiveCommand<Window, Unit> ActionCancel { get; }
        public ReactiveCommand<Window, Unit> ActionOK { get; }

        public IncidencePickerModalWindowViewModel()
        {
            Incidence = ushort.MaxValue;
            ActionCancel = ReactiveCommand.Create<Window>(CancelChanges);
            ActionOK = ReactiveCommand.Create<Window>(SaveChanges);
        }

        private void CancelChanges(Window w)
        {
            w.Close();
        }

        private void SaveChanges(Window w)
        {
            w.Close(w);
        }
    }
}
