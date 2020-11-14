using Sm5sh.Helpers.PrcHelper;

namespace Sm5sh.Services.Interfaces
{
    public interface IPrcService
    {
        T ReadPrcFile<T>(string inputFile) where T : IPrcParsable, new();
        bool WritePrcFile<T>(string outputFile, T inputObj) where T : IPrcParsable;
    }
}
