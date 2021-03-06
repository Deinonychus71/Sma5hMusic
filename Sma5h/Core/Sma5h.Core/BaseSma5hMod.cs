using Sma5h.Interfaces;

namespace Sma5h
{
    public abstract class BaseSma5hMod : ISma5hMod
    {
        protected IStateManager _state;
        protected string _modPath;

        public BaseSma5hMod(IStateManager state)
        {
            _state = state;
        }

        public abstract string ModName { get; }

        public virtual bool Init() { return true; }

        public virtual bool Run() { return true; }

        public virtual bool Build(bool useCache) { return true; }
    }
}
