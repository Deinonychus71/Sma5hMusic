using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Sma5hMusic.GUI.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Sma5hMusic.GUI.Dialogs
{
    public class FileDialog : IFileDialog
    {
        private readonly ILogger _logger;
        private readonly IDialogWindow _rootDialogWindow;
        private readonly OpenFileDialog _openFileDialog;
        private readonly OpenFolderDialog _openFolderDialog;
        private string _savedDirectory;
        private string _savedFolderDirectory;

        public FileDialog(IDialogWindow rootDialogWindow, ILogger<FileDialog> logger)
        {
            _logger = logger;
            _rootDialogWindow = rootDialogWindow;
            _savedDirectory = Environment.CurrentDirectory;
            _savedFolderDirectory = Environment.CurrentDirectory;
            _openFileDialog = new OpenFileDialog();
            _openFolderDialog = new OpenFolderDialog();
        }

        public async Task<string[]> OpenFileDialogAudioMultiple(Window parent = null)
        {
            _logger.LogDebug("Opening FileDialog...");

            _openFileDialog.AllowMultiple = true;
            _openFileDialog.Directory = _savedDirectory;
            _openFileDialog.Filters = new List<FileDialogFilter>()
            {
                new FileDialogFilter()
                {
                    Extensions = new List<string>()
                    {
                        "brstm", "lopus", "idsp", "nus3audio"
                    },
                    Name = "Songs"
                }
            };
            _openFileDialog.Title = "Load Audio Files";

            string[] results;
            if (parent == null)
                results = await _openFileDialog.ShowAsync(_rootDialogWindow.Window);
            else
                results = await _openFileDialog.ShowAsync(parent);

            if (results.Length > 0)
                _savedDirectory = Path.GetDirectoryName(results[0]);

            _logger.LogDebug("Selected {NbrItems} items", results?.Length);

            return results;
        }

        public async Task<string> OpenFileDialogAudioSingle(Window parent = null)
        {
            _logger.LogDebug("Opening FileDialog...");

            _openFileDialog.AllowMultiple = false;
            _openFileDialog.Directory = _savedDirectory;
            _openFileDialog.Filters = new List<FileDialogFilter>()
            {
                new FileDialogFilter()
                {
                    Extensions = new List<string>()
                    {
                        "brstm", "lopus", "idsp", "nus3audio"
                    },
                    Name = "Songs"
                }
            };
            _openFileDialog.Title = "Load Audio Files";

            string[] results;
            if (parent == null)
                results = await _openFileDialog.ShowAsync(_rootDialogWindow.Window);
            else
                results = await _openFileDialog.ShowAsync(parent);

            if (results.Length > 0)
            {
                _savedDirectory = Path.GetDirectoryName(results[0]);
                return results[0];
            }

            return null;
        }

        public async Task<string> OpenFolderDialog(Window parent = null)
        {
            _logger.LogDebug("Opening FolderDialog...");

            _openFolderDialog.Title = "Choose Directory";
            _openFolderDialog.Directory = _savedFolderDirectory;

            string result;
            if (parent == null)
                result = await _openFolderDialog.ShowAsync(_rootDialogWindow.Window);
            else
                result = await _openFolderDialog.ShowAsync(parent);

            if (!string.IsNullOrEmpty(result))
                _savedFolderDirectory = result;

            _logger.LogDebug("Selected {Directory}", result);

            return result;
        }
    }
}
