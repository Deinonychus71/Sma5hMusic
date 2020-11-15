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
        }

        public T LoadResource<T>(string gameRelativeResourcePath, bool optional) where T : IStateManagerDb, new()
        {
            _logger.LogDebug("Load Resource {GameResource}", gameRelativeResourcePath);

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

                _logger.LogInformation("Loaded Resource {GameResource} with provider {ResourceProvider}", gameRelativeResourcePath, resourceProvider.GetType().Name);

                return newResource;
            }

            if (optional)
                return default;

            throw new Exception("Resource could not be loaded. Make sure that a proper resource provider is configured");
        }

        public bool WriteChanges()
        {
            _logger.LogDebug("Write State Changes to {OutputPath}", _config.Value.OutputPath);

            foreach(var resource in _resources)
            {
                var outputResourceFile = Path.Combine(_config.Value.OutputPath, resource.Key);
                var inputResourceFile = Path.Combine(_config.Value.GameResourcesPath, resource.Key);
                Directory.CreateDirectory(Path.GetDirectoryName(outputResourceFile));
                var resourceProvider = GetResourceProvider(resource.Key);

                _logger.LogInformation("Write State Changes for Resource {GameResource} to {OutputPath} using provider {ResourceProvider}", resource.Key, outputResourceFile, resourceProvider.GetType().Name);

                resourceProvider.WriteFile(inputResourceFile, outputResourceFile, resource.Value);
            }

            return true;
        }

        public bool Init()
        {
            _logger.LogInformation("Resources Path: {ResourcesPath}", _config.Value.ResourcesPath);
            _logger.LogInformation("Output Path: {OutputPath}", _config.Value.OutputPath);
            _logger.LogInformation("Game Resource Path: {GameResourcesPath}", _config.Value.GameResourcesPath);
            _logger.LogInformation("Log Path: {LogPath}", _config.Value.LogPath);
            _logger.LogInformation("Temp Path: {TempPath}", _config.Value.TempPath);
            _logger.LogInformation("Tools Path: {ToolsPath}", _config.Value.ToolsPath);

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
            try
            {
                foreach (var service in _serviceProvider.GetServices<IResourceProvider>())
                {
                    var attribute = service.GetType().GetCustomAttributes(true).FirstOrDefault(p => p.GetType() == typeof(ResourceProviderMatchAttribute)) as ResourceProviderMatchAttribute;
                    _logger.LogDebug("Initialize Resource Provider {ResourceProvider} for the following match: {ProviderMatch}", service.GetType().Name, attribute.ExtensionOrPath);
                    providers.Add(attribute.ExtensionOrPath, service);
                }
            }
            catch(Exception e)
            {
                _logger.LogError(e, "A Resource Provider could not load. Please make sure that all the file paths are correct.");
            }

            return providers;
        }
    }
}
