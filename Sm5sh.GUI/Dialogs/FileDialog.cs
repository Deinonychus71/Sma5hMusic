using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Sm5sh.GUI.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Sm5sh.GUI.Dialogs
{
    public class FileDialog : IFileDialog
    {
        private ILogger _logger;
        private IDialogWindow _rootDialogWindow;
        private OpenFileDialog _openFileDialog;
        private string _savedDirectiory;

        public FileDialog(IDialogWindow rootDialogWindow, ILogger<FileDialog> logger)
        {
            _logger = logger;
            _rootDialogWindow = rootDialogWindow;
            _savedDirectiory = Environment.CurrentDirectory;
            _openFileDialog = new OpenFileDialog();
        }

        public async Task<string[]> OpenFileDialogAudio(Window parent = null)
        {
            _openFileDialog.AllowMultiple = true;
            _openFileDialog.Directory = _savedDirectiory;
            _openFileDialog.Filters = new List<FileDialogFilter>()
            {
                new FileDialogFilter()
                {
                    Extensions = new List<string>()
                    {
                        "brstm", "lopus", "idsp"
                    },
                    Name = "Songs"
                }
            };
            _openFileDialog.Title = "Load Audio Files";

            string[] results;
            if(parent == null)
                results = await _openFileDialog.ShowAsync(_rootDialogWindow.Window);
            else
                results = await _openFileDialog.ShowAsync(parent);

            if(results.Length > 0)
                _savedDirectiory = Path.GetDirectoryName(results[0]);

            return results;
        }
    }
}
