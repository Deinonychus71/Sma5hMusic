namespace Sm5sh.Mods.Music.Interfaces
{
    public interface INus3AudioService
    {
        bool GenerateNus3Audio(string toneId, string inputMediaFile, string outputMediaFile);
        bool GenerateNus3Bank(string toneId, float volume, string outputMediaFile);
    }
}
