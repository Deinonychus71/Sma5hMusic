using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using Sm5sh.Mods.Music;
using Sm5sh.Mods.Music.Models;
using Sm5shMusic.GUI.Interfaces;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sm5shMusic.GUI.ViewModels
{
    public class ModPropertiesModalWindowViewModel : ModalBaseViewModel<ModEntryViewModel>
    {
        private readonly IOptions<Sm5shMusicOptions> _config;
        private const string REGEX_REPLACE = @"[^a-zA-Z0-9\-_ ]";
        private readonly string REGEX_VALIDATION = @"^[\w\-. ]+$";
        private readonly ILogger _logger;
        private readonly IGUIStateManager _guiStateManager;
        private readonly IViewModelManager _viewModelManager;

        [Reactive]
        public string ModName { get; set; }
        [Reactive]
        public string ModPath { get; set; }
        [Reactive]
        public string ModWebsite { get; set; }
        [Reactive]
        public string ModAuthor { get; set; }
        [Reactive]
        public string ModDescription { get; set; }

        [Reactive]
        public bool IsEdit { get; set; }

        public ModPropertiesModalWindowViewModel(ILogger<ModPropertiesModalWindowViewModel> logger, IViewModelManager viewModelManager,
            IGUIStateManager guiStateManager, IOptions<Sm5shMusicOptions> config)
        {
            _config = config;
            _logger = logger;
            _guiStateManager = guiStateManager;
            _viewModelManager = viewModelManager;

            this.WhenAnyValue(p => p.ModName).Subscribe((o) => { FormatModPath(o); });

            this.ValidationRule(p => p.ModPath,
                p => !string.IsNullOrEmpty(p) && ((Regex.IsMatch(p, REGEX_VALIDATION) && !Directory.Exists(Path.Combine(_config.Value.Sm5shMusic.ModPath, p))) || IsEdit),
                $"The folder name is invalid or the folder already exists.");

            //Validation
            this.ValidationRule(p => p.ModName,
                p => !string.IsNullOrEmpty(p),
                $"Please enter a Title.");
        }

        private void FormatModPath(string modName)
        {
            if (!IsEdit)
            {
                if (string.IsNullOrEmpty(modName))
                    ModPath = string.Empty;
                else
                    ModPath = Regex.Replace(modName, REGEX_REPLACE, string.Empty).ToLower();
            }
        }

        protected override void LoadItem(ModEntryViewModel item)
        {
            _logger.LogDebug("Load Item");

            if (item?.MusicMod == null)
            {
                IsEdit = false;
                ModName = string.Empty;
                ModPath = string.Empty;
                ModWebsite = string.Empty;
                ModAuthor = string.Empty;
                ModDescription = string.Empty;
            }
            else
            {
                IsEdit = true;
                ModName = item.MusicMod.Mod.Name;
                ModPath = item.MusicMod.ModPath;
                ModWebsite = item.MusicMod.Mod.Website;
                ModAuthor = item.MusicMod.Mod.Author;
                ModDescription = item.MusicMod.Mod.Description;
            }
        }

        protected override async Task SaveChanges()
        {
            _logger.LogDebug("Save Changes");

            if (!IsEdit)
            {
                var modId = await _guiStateManager.CreateNewModEntry(new MusicModInformation()
                {
                    Name = this.ModName,
                    Author = this.ModAuthor,
                    Website = this.ModWebsite,
                    Description = this.ModDescription
                }, this.ModPath);
                _refSelectedItem = _viewModelManager.GetModEntryViewModel(modId);
            }
            else
            {
                _refSelectedItem.Name = this.ModName;
                _refSelectedItem.Author = this.ModAuthor;
                _refSelectedItem.Website = this.ModWebsite;
                _refSelectedItem.Description = this.ModDescription;
            }
        }
    }
}
