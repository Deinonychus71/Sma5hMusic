using Avalonia.Controls;
using Avalonia.Threading;
using System.IO;
using System.Text;

namespace Sm5sh.GUI.Helpers
{
    public class ControlWriter : TextWriter
    {
        private readonly TextBox _textbox;
        private readonly ScrollViewer _scrollViewer;

        public ControlWriter(TextBox textbox, ScrollViewer scrollViewer)
        {
            _textbox = textbox;
            _scrollViewer = scrollViewer;
        }

        public override void Write(char value)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                _textbox.Text += value;
                _scrollViewer.ScrollToEnd();
            }, DispatcherPriority.Background);
        }

        public override void Write(string value)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                _textbox.Text += value;
                _scrollViewer.ScrollToEnd();
            }, DispatcherPriority.Background);
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
