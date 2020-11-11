namespace Sm5shMusic.Interfaces
{
    public interface IWorkspaceManager
    {
        bool Init();

        string GetWorkspaceDirectory();
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
