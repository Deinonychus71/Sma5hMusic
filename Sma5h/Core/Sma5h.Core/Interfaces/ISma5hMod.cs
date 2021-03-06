namespace Sma5h.Interfaces
{
    public interface ISma5hMod
    {
        string ModName { get; }
        bool Init();

        bool Build(bool useCache = true);
    }
}
