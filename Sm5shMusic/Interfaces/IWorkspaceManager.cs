namespace Sm5shMusic.Interfaces
{
    public interface IWorkspaceManager
    {
        bool Init();

        bool IsAudioCacheEnabled { get; }

        string GetWorkspaceDirectory();
        string GetCacheDirectory();
        string GetAudioCacheDirectory();
        string GetCacheForNus3Audio(string toneName);
        string GetWorkspaceOutputForNus3Audio();
        string GetWorkspaceOutputForNus3Audio(string toneName);
        string GetWorkspaceOutputForNus3Bank(string toneName);
        string GetWorkspaceOutputForUiMessage();
        string GetWorkspaceOutputForUiDb();
        string GetWorkspaceOutputForSoundConfig();
        string GetWorkspaceOutputForUiGameTitleDbFile();
        string GetWorkspaceOutputForUiBgmDbFile();
        string GetWorkspaceOutputForBgmPropertyFile();
        string GetWorkspaceOutputForBgmPropertyTempFile();
        string GetWorkspaceOutputForMsbtTitleResource(string locale);
        string GetWorkspaceOutputForMsbtBgmResource(string locale);
    }
}
