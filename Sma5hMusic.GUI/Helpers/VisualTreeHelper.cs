using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Sma5hMusic.GUI.Helpers
{
    public static class VisualTreeHelper
    {
        public const string STYLES_CLASS_IS_DRAGGING = "isDragging";

        public static T GetControl<T>(IInteractive control) where T : class
        {
            var source = control;
            while (!(source is T) && source != null)
            {
                source = source.InteractiveParent;
            }
            return source == null ? null : (T)source;
        }

        public static void RemoveClassStyle<T>(IInteractive control, string styleClass) where T: Control
        {
            var dataGrid = GetControl<T>(control);
            if (dataGrid != null)
                dataGrid.Classes.Remove(styleClass);
        }

        public static void AddClassStyle<T>(IInteractive control, string styleClass) where T : Control
        {
            var dataGrid = GetControl<T>(control);
            if (dataGrid != null)
                dataGrid.Classes.Add(styleClass);
        }
    }
}