using Sm5sh.Interfaces;

namespace Sm5sh
{
    public abstract class BaseSm5shMod : ISm5shMod
    {
        protected IStateManager _state;
        protected string _modPath;

        public BaseSm5shMod(IStateManager state)
        {
            _state = state;
        }

        public abstract string ModName { get ; }

        public virtual void Init() { }

        public abstract bool SaveChanges();
    }
}
