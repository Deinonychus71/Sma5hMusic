using Sm5sh.Helpers.YmlHelper;

namespace Sm5sh.Services.Interfaces
{
    public interface IYmlService
    {
        T ReadYmlFile<T>(string inputFile) where T : IYmlParsable, new();
        bool WriteYmlFile<T>(string outputFile, T inputObj) where T : IYmlParsable;
    }
}
