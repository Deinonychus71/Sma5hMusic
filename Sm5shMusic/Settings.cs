using System.Dynamic;

namespace Sm5shMusic
{
    public class Settings
    {
        public string MusicModPath { get; set; }

        public string LibsPath { get; set; }

        public string ResourcesPath { get; set; }

        public string WorkspacePath { get; set; }

        public string CachePath { get; set; }

        public bool EnableAudioCaching { get; set; }

        public bool SkipWorkspaceCleanupConfirmation { get; set; }

        public string DefaultLocale { get; set; }
    }
}
