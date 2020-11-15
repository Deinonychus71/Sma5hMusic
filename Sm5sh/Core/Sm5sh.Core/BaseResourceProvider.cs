using Microsoft.Extensions.Options;
using Sm5sh.Interfaces;

namespace Sm5sh
{
    public abstract class BaseResourceProvider : IResourceProvider
    {
        protected IOptions<Sm5shOptions> _config;

        public BaseResourceProvider(IOptions<Sm5shOptions> config)
        {
            _config = config;
        }

        public abstract T ReadFile<T>(string inputFile) where T : IStateManagerDb, new();

        public abstract bool WriteFile<T>(string inputFile, string outputFile, T inputObj) where T : IStateManagerDb;
    }
}
