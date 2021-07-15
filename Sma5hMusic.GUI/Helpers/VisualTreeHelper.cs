using Avalonia.Interactivity;

namespace Sma5hMusic.GUI.Helpers
{
    public static class VisualTreeHelper
    {
        public static T GetControl<T>(IInteractive control) where T : class
        {
            var source = control;
            while (!(source is T) && source != null)
            {
                source = source.InteractiveParent;
            }
            return source == null ? null : (T)source;
        }
    }
}
