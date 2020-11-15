namespace Sm5sh.Interfaces
{
    public interface IResourceProvider
    {
        T ReadFile<T>(string inputFile) where T : IStateManagerDb, new();
        bool WriteFile<T>(string inputFile, string outputFile, T inputObj) where T : IStateManagerDb;
    }
}
