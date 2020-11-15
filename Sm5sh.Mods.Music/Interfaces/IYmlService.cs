namespace Sm5sh.Mods.Music.Interfaces
{
    public interface IYmlService
    {
        T ReadYmlFile<T>(string inputFile) where T : new();
        bool WriteYmlFile<T>(string outputFile, T inputObj);
    }
}
