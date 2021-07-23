using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sma5hMusic.GUI.Helpers
{
    public class ControlWriter : TextWriter
    {
        private readonly TextBox _textbox;
        private readonly ScrollViewer _scrollViewer;
        private readonly ConcurrentQueue<string> _queuedMessages;
        private readonly CancellationTokenSource _cts;

        public ControlWriter(TextBox textbox, ScrollViewer scrollViewer)
        {
            _textbox = textbox;
            _scrollViewer = scrollViewer;
            _queuedMessages = new ConcurrentQueue<string>();
            _cts = new CancellationTokenSource();
            _ = Task.Run(async() => await RunQueue(_cts.Token));
        }

        public override void Write(char value)
        {
            _queuedMessages.Enqueue(value.ToString());
        }

        public override void Write(string value)
        {
            if(!string.IsNullOrWhiteSpace(value))
                _queuedMessages.Enqueue($"{DateTime.Now:yyyy-MM-ddTHH\\:mm\\:ss}: {value}\r\n");
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        private async Task RunQueue(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var messagesToPrint = string.Empty;
                int i = 0;
                while (i < 100 && _queuedMessages.TryDequeue(out string newMessage))
                {
                    messagesToPrint += newMessage;
                    i++;
                }
                if (!string.IsNullOrEmpty(messagesToPrint))
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        _textbox.Text += messagesToPrint;
                        _scrollViewer.ScrollToEnd();
                    }, DispatcherPriority.Background);
                }
                await Task.Delay(50);
            }
        }

        public override ValueTask DisposeAsync()
        {
            _cts.Cancel();
            _cts?.Dispose();
            return base.DisposeAsync();
        }
    }
}
