namespace Sma5h
{
    public class Sma5hOptions
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

        //Backup
        public string BackupPath { get; set; }

        //Logs
        public string LogPath { get; set; }

        //To skip cleanup confirm
        public bool SkipOutputPathCleanupConfirmation { get; set; }
    }
}
