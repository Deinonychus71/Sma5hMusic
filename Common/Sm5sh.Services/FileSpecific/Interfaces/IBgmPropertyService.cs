using Sm5sh.Data.Sound.Config;

namespace Sm5sh.Services.FileSpecific.Interfaces
{
    public interface IBgmPropertyService
    {
        BinBgmProperty ReadBgmPropertyFile(string bgmPropertyExeFile, string bgmPropertyHashFile, string bgmPropertyFile, string tempPath);
        bool WriteBgmPropertyFile(string bgmPropertyExeFile, string outputFile, string tempPath, BinBgmProperty inputObj);
    }
}
