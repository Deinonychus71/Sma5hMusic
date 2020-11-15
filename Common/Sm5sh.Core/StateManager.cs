using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Sm5sh.Interfaces;
using Sm5sh.Attributes;

namespace Sm5sh
{
    public class StateManager : IStateManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly IOptions<Sm5shOptions> _config;
        private Dictionary<string, IResourceProvider> _resourceProviders;
        private Dictionary<string, IStateManagerDb> _resources;

        public IEnumerable<IStateManagerDb> Resources { get { return _resources.Values; } }

        public StateManager(IServiceProvider serviceProvider, IOptions<Sm5shOptions> config, ILogger<IStateManager> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _config = config;
            _resources = new Dictionary<string, IStateManagerDb>();
            _resourceProviders = InitializeResourceProviders();
            //BgmProperty = _bgmPropertyService.ReadBgmPropertyFile(_config.Value.ToolsBgmPropertyExeFile, _config.Value.ToolsBgmPropertyHashesFile, _config.Value.BinBgmPropertyResourceFile, _config.Value.TempPath);
        }

        public T LoadResource<T>(string gameRelativeResourcePath, bool optional) where T : IStateManagerDb, new()
        {
            if (_resources.ContainsKey(gameRelativeResourcePath))
                return (T)_resources[gameRelativeResourcePath];

            var resourceProvider = GetResourceProvider(gameRelativeResourcePath);
            if (resourceProvider != null)
            {
                var gameResourceFile = Path.Combine(_config.Value.GameResourcesPath, gameRelativeResourcePath);
                if (!File.Exists(gameResourceFile))
                {
                    if (optional)
                        return default;
                    throw new Exception($"Resource could not be loaded. The resource {gameResourceFile} doesn't exist.");
                }

                var newResource = resourceProvider.ReadFile<T>(gameResourceFile);
                _resources.Add(gameRelativeResourcePath, newResource);
                return newResource;
            }

            if (optional)
                return default;

            throw new Exception("Resource could not be loaded. Make sure that a proper resource provider is configured");
        }

        public bool WriteChanges()
        {
            _logger.LogInformation("Write State Changes to {OutputPath}", _config.Value.OutputPath);

            foreach(var resource in _resources)
            {
                var outputResourceFile = Path.Combine(_config.Value.OutputPath, resource.Key);
                var inputResourceFile = Path.Combine(_config.Value.GameResourcesPath, resource.Key);
                Directory.CreateDirectory(Path.GetDirectoryName(outputResourceFile));
                var resourceProvider = GetResourceProvider(resource.Key);
                resourceProvider.WriteFile(inputResourceFile, outputResourceFile, resource.Value);
            }

            return true;
        }

        private IResourceProvider GetResourceProvider(string gameRelativeResourcePath)
        {
            if (_resourceProviders.ContainsKey(gameRelativeResourcePath))
                return _resourceProviders[gameRelativeResourcePath];

            var ext = Path.GetExtension(gameRelativeResourcePath);
            if (_resourceProviders.ContainsKey(ext))
                return _resourceProviders[ext];

            throw new Exception("Resource could not be loaded. Make sure that a proper resource provider is configured");
        }

        private Dictionary<string, IResourceProvider> InitializeResourceProviders()
        {
            var providers = new Dictionary<string, IResourceProvider>();
            foreach(var service in _serviceProvider.GetServices<IResourceProvider>())
            {
                var attribute = service.GetType().GetCustomAttributes(true).FirstOrDefault(p => p.GetType() == typeof(ResourceProviderMatchAttribute)) as ResourceProviderMatchAttribute;
                providers.Add(attribute.ExtensionOrPath, service);
            }

            return providers;
        }
    }
}
