namespace Sm5sh.Interfaces
{
    public interface ISm5shMod
    {
        string ModName { get; }
        bool Init();

        bool SaveChanges();
    }
}
