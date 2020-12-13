using ReactiveUI;
using Sm5shMusic.GUI.Helpers;
using Sm5shMusic.GUI.Interfaces;
using Sm5shMusic.GUI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace Sm5shMusic.GUI.ViewModels
{
    public class GlobalSettingsModalWindowViewModel : ModalBaseViewModel<GlobalConfigurationViewModel>
    {
        private readonly IGUIStateManager _guiStateManager;
        private readonly IFileDialog _fileDialog;

        public List<string> UIThemes => new List<string>() { "Dark", "Light" };
        public List<string> UIScales => new List<string>() { "Normal", "Small" };
        public List<string> ConversionFormats => new List<string>() { "lopus", "idsp" };
        public List<string> FallBackConversionFormats => new List<string>() { "lopus", "idsp" };
        public List<ComboItem> Locales => Constants.CONVERTER_LOCALE.Select(p => new ComboItem(p.Key, p.Value)).ToList();

        public ReactiveCommand<Unit, Unit> ActionWipeAudioCache { get; }
        public ReactiveCommand<string, Unit> ActionOpenFileDialog { get; }

        public ComboItem SelectedLocale { get; set; }

        public GlobalSettingsModalWindowViewModel(IGUIStateManager guiStateManager, IFileDialog fileDialog)
        {
            _guiStateManager = guiStateManager;
            _fileDialog = fileDialog;

            ActionWipeAudioCache = ReactiveCommand.CreateFromTask(OnWipeAudioCache);
            ActionOpenFileDialog = ReactiveCommand.CreateFromTask<string>(OnChooseFolder);
        }

        public async Task OnWipeAudioCache()
        {
            await _guiStateManager.WipeAudioCache();
        }

        public async Task OnChooseFolder(string param)
        {
            var result = await _fileDialog.OpenFolderDialog();
            if (!string.IsNullOrEmpty(result))
            {
                switch (param)
                {
                    case "OutputPath":
                        SelectedItem.OutputPath = result;
                        break;
                    case "ModPath":
                        SelectedItem.ModPath = result;
                        break;
                    case "ModOverridePath":
                        SelectedItem.ModOverridePath = result;
                        break;
                    case "GameResourcesPath":
                        SelectedItem.GameResourcesPath = result;
                        break;
                    case "ResourcesPath":
                        SelectedItem.ResourcesPath = result;
                        break;
                    case "ToolsPath":
                        SelectedItem.ToolsPath = result;
                        break;
                    case "CachePath":
                        SelectedItem.CachePath = result;
                        break;
                    case "TempPath":
                        SelectedItem.TempPath = result;
                        break;
                    case "LogPath":
                        SelectedItem.LogPath = result;
                        break;
                }
            }
        }

        protected override void LoadItem(GlobalConfigurationViewModel item)
        {
            SelectedLocale = Locales.FirstOrDefault(p => p.Id == item?.DefaultLocale);
        }
    }
}
