namespace Sma5h.Interfaces
{
    public interface ISma5hMod
    {
        string ModName { get; }
        bool Init();
        string BuildPreCheck();
        bool Build(bool useCache = true);
    }
}
