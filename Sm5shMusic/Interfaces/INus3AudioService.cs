namespace Sm5shMusic.Interfaces
{
    public interface INus3AudioService
    {
        bool GenerateNus3Audio(string toneId, string inputMediaPath, string outputMediaPath);
        bool GenerateNus3Bank(string toneId, string inputMediaPath, string outputMediaPath);
    }
}
