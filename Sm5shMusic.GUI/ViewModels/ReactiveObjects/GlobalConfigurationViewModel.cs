using AutoMapper;
using ReactiveUI.Fody.Helpers;
using Sm5sh.Mods.Music;

namespace Sm5shMusic.GUI.ViewModels
{
    public class GlobalConfigurationViewModel : ReactiveObjectBaseViewModel
    {
        private readonly ApplicationSettings _applicationSettingsRef;
        private readonly IMapper _mapper;

        public bool Advanced { get; set; }
        public string UIScale { get; set; }
        public string UITheme { get; set; }
        public bool SkipOutputPathCleanupConfirmation { get; set; }
        public bool EnableAudioCaching { get; set; }
        public string AudioConversionFormat { get; set; }
        public string AudioConversionFormatFallBack { get; set; }
        public string DefaultSm5shMusicLocale { get; set; } //TODO, remove?
        public string DefaultGUILocale { get; set; }
        public string DefaultMSBTLocale { get; set; }

        [Reactive]
        public ushort PlaylistIncidenceDefault { get; set; }
        [Reactive]
        public bool CopyToEmptyLocales { get; set; }
        [Reactive]
        public string ModOverridePath { get; set; }
        [Reactive]
        public string ModPath { get; set; }
        [Reactive]
        public string CachePath { get; set; }
        [Reactive]
        public string GameResourcesPath { get; set; }
        [Reactive]
        public string OutputPath { get; set; }
        [Reactive]
        public string ToolsPath { get; set; }
        [Reactive]
        public string TempPath { get; set; }
        [Reactive]
        public string ResourcesPath { get; set; }
        [Reactive]
        public string LogPath { get; set; }

        public ApplicationSettings GetReference()
        {
            return _applicationSettingsRef;
        }

        public GlobalConfigurationViewModel(IMapper mapper, ApplicationSettings appSettings)
        {
            _applicationSettingsRef = appSettings;
            _mapper = mapper;

            _mapper.Map(appSettings, this);
        }

        public override ReactiveObjectBaseViewModel GetCopy()
        {
            return this; //not needed
        }

        public override ReactiveObjectBaseViewModel SaveChanges()
        {
            _mapper.Map(this, _applicationSettingsRef);
            return this;
        }
    }
}
