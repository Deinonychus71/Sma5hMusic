using Avalonia.Threading;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Sma5h.Mods.Music.Interfaces;
using Sma5hMusic.GUI.Interfaces;
using Sma5hMusic.GUI.Models.DevTools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sma5hMusic.GUI.Services
{
    public class DevToolsService : IDevToolsService
    {
        private readonly ILogger _logger;
        private readonly IViewModelManager _viewModelManager;
        private readonly IMessageDialog _messageDialog;

        public DevToolsService(IViewModelManager viewModelManager, IMessageDialog messageDialog, ILogger<IDevToolsService> logger)
        {
            _logger = logger;
            _messageDialog = messageDialog;
            _viewModelManager = viewModelManager;
        }

        public async Task<bool> ExportToCSV(string exportPath)
        {
            try
            {
                _logger.LogInformation("Exporting BGM Songs to CSV: {ExportPath}...", exportPath);
                var vmBgmPropertyEntries = _viewModelManager.GetBgmDbRootEntriesViewModels();
                var csvOutputSet = new List<CSVSongExportEntry>();

                foreach (var vmBgmPropertyEntry in vmBgmPropertyEntries)
                {
                    csvOutputSet.Add(new CSVSongExportEntry()
                    {
                        DbRoot = vmBgmPropertyEntry,
                        BgmProperty = vmBgmPropertyEntry.BgmPropertyViewModel,
                        AssignedInfo = vmBgmPropertyEntry.AssignedInfoViewModel,
                        StreamProperty = vmBgmPropertyEntry.StreamPropertyViewModel,
                        StreamSet = vmBgmPropertyEntry.StreamSetViewModel,
                        GameTitle = vmBgmPropertyEntry.GameTitle1ViewModel
                    });
                }

                using (var writer = new StreamWriter(exportPath))
                {
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture);
                    using (var csv = new CsvWriter(writer, config))
                    {
                        csv.Context.RegisterClassMap<CSVSongExportEntryMap>();
                        csv.WriteRecords(csvOutputSet);
                    }
                }

                _logger.LogInformation("Exported BGM Songs to CSV: {ExportPath}.", exportPath);
            }
            catch(Exception e)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messageDialog.ShowError("Exported BGM Songs to CSV", "There was an error while exporting to CSV. Please check the logs.", e);
                }, DispatcherPriority.Background);
                return false;
            }

            return true;
        }
    }
}
