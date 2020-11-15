using System.IO;
using System.Text.Json.Serialization;

namespace Sm5sh
{
    public class Sm5shOptions
    {
        //Game Path (Input)
        public string GameResourcesPath { get; set; }

        //Output Path
        public string OutputPath { get; set; }

        //Tools Path
        public string ToolsPath { get; set; }

        //Temp Path
        public string TempPath { get; set; }

        //Helper resource files
        public string ResourcesPath { get; set; }

        //Cache for some mods
        public string CachePath { get; set; }

        //Logs
        public string LogPath { get; set; }

        //ModsPath
        public string ModsPath { get; set; }

        //To skip cleanup confirm
        public bool SkipOutputPathCleanupConfirmation { get; set; }
    }
}
