using Sm5sh.Interfaces;

namespace Sm5sh
{
    public abstract class BaseSm5shMod : ISm5shMod
    {
        public virtual void Init() { }

        public abstract bool SaveChanges();
    }
}
