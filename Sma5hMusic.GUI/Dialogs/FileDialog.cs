using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Sma5hMusic.GUI.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Sma5hMusic.GUI.Dialogs
{
    public class FileDialog : IFileDialog
    {
        private readonly ILogger _logger;
        private readonly IDialogWindow _rootDialogWindow;
        private readonly OpenFileDialog _openFileDialog;
        private readonly SaveFileDialog _saveFileDialog;
        private readonly OpenFolderDialog _openFolderDialog;
        private string _savedDirectory;
        private string _savedSaveFileDirectory;
        private string _savedSaveFileName;
        private string _savedFolderDirectory;

        public FileDialog(IDialogWindow rootDialogWindow, ILogger<FileDialog> logger)
        {
            _logger = logger;
            _rootDialogWindow = rootDialogWindow;
            _savedSaveFileName = "export_songs.csv";
            _savedDirectory = Environment.CurrentDirectory;
            _savedSaveFileDirectory = Environment.CurrentDirectory;
            _savedFolderDirectory = Environment.CurrentDirectory;
            _saveFileDialog = new SaveFileDialog();
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

        public async Task<string> SaveFileCSVDialog(Window parent = null)
        {
            _logger.LogDebug("Opening SaveDialog...");

            _saveFileDialog.Directory = _savedSaveFileDirectory;
            _saveFileDialog.InitialFileName = _savedSaveFileName;
            _saveFileDialog.Filters = new List<FileDialogFilter>()
            {
                new FileDialogFilter()
                {
                    Extensions = new List<string>()
                    {
                        "csv"
                    },
                    Name = "CSV File"
                }
            };
            _saveFileDialog.Title = "Save CSV";

            string result;
            if (parent == null)
                result = await _saveFileDialog.ShowAsync(_rootDialogWindow.Window);
            else
                result = await _saveFileDialog.ShowAsync(parent);

            if (!string.IsNullOrEmpty(result))
            {
                _savedSaveFileName = Path.GetFileName(result);
                _savedSaveFileDirectory = Path.GetDirectoryName(result);
            }

            return result;
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

        public void OpenFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                var startInfo = new ProcessStartInfo
                {
                    Arguments = folderPath,
                    FileName = "explorer.exe"
                };

                Process.Start(startInfo);
            }
        }
    }
}
