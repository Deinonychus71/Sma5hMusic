using Microsoft.Extensions.Options;
using Sma5h.Interfaces;

namespace Sma5h
{
    public abstract class BaseResourceProvider : IResourceProvider
    {
        protected IOptions<Sma5hOptions> _config;

        public BaseResourceProvider(IOptions<Sma5hOptions> config)
        {
            _config = config;
        }

        public abstract T ReadFile<T>(string inputFile) where T : IStateManagerDb, new();

        public abstract bool WriteFile<T>(string inputFile, string outputFile, T inputObj) where T : IStateManagerDb;
    }
}
