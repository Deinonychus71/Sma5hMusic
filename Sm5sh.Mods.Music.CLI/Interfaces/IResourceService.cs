namespace Sm5shMusic.Interfaces
{
    public interface IResourceService
    {
        //Executable Paths
        string GetNus3AudioCLIExe();

        string GetBgmPropertyExe();

        string GetBgmPropertyHashes();

        //Resources Paths
        string GetBgmPropertyYmlResource();

        string GetBgmDbLabelsCsvResource();

        string GetNusBankIdsCsvResource();



        string GetBgmPropertyResource();

        string GetBgmDbResource();

        string GetGameTitleDbResource();

        string GetStageDbResource();

        string GetNusBankTemplateResource();

        string GetMsbtTitleResource(string locale);

        string GetMsbtBgmResource(string locale);

        //Temp
        string GetTemporaryAudioConversionFile();

        string GetTemporaryBgmPropertiesYmlFile();
    }
}
