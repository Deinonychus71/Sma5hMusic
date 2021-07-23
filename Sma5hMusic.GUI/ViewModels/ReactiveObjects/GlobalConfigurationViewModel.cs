using AutoMapper;
using ReactiveUI.Fody.Helpers;
using Sma5h.Mods.Music;
using System.Collections.Generic;

namespace Sma5hMusic.GUI.ViewModels
{
    public class GlobalConfigurationViewModel : ReactiveObjectBaseViewModel
    {
        private readonly ApplicationSettings _applicationSettingsRef;
        private readonly IMapper _mapper;

        public bool Advanced { get; set; }
        public bool PlaylistAdvanced { get; set; }
        public string UIScale { get; set; }
        public string UITheme { get; set; }
        public bool SkipOutputPathCleanupConfirmation { get; set; }
        public bool InGameVolume { get; set; }
        public bool EnableAudioCaching { get; set; }
        public string AudioConversionFormat { get; set; }
        public string AudioConversionFormatFallBack { get; set; }
        public string DefaultSma5hMusicLocale { get; set; } //TODO, remove?
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
        [Reactive]
        public bool SkipWarningGameVersion { get; set; }
        [Reactive]
        public ushort PlaylistAutoMappingIncidence { get; set; }
        public Dictionary<string, List<string>> PlaylistAutoMapping { get; set; }
        public PlaylistGeneration PlaylistGenerationMode { get; set; }

        [Reactive]
        public bool HideIndexColumn { get; set; }
        [Reactive]
        public bool HideSeriesColumn { get; set; }
        [Reactive]
        public bool HideRecordColumn { get; set; }
        [Reactive]
        public bool HideModColumn { get; set; }

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

    public class PlaylistGenerationItem
    {
        public int Id { get; set; }
        public string Label { get; set; }

        public PlaylistGenerationItem(PlaylistGeneration id, string label)
        {
            Id = (int)id;
            Label = label;
        }

        public override bool Equals(object obj)
        {
            return obj is PlaylistGenerationItem toCompare && toCompare.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }

    public enum PlaylistGeneration
    {
        Manual = 0,
        OnlyMissingSongs = 1,
        AllSongs = 2
    }
}
