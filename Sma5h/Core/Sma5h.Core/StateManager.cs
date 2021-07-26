using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sma5h.Attributes;
using Sma5h.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sma5h
{
    public class StateManager : IStateManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly IOptionsMonitor<Sma5hOptions> _config;
        private readonly Dictionary<string, IResourceProvider> _resourceProviders;
        private readonly Dictionary<string, IStateManagerDb> _originalResources;
        private readonly Dictionary<string, IStateManagerDb> _resources;

        public IEnumerable<IStateManagerDb> Resources { get { return _resources.Values; } }

        public StateManager(IServiceProvider serviceProvider, IOptionsMonitor<Sma5hOptions> config, ILogger<IStateManager> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _config = config;
            _resources = new Dictionary<string, IStateManagerDb>();
            _originalResources = new Dictionary<string, IStateManagerDb>();
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
                var gameResourceFile = Path.Combine(_config.CurrentValue.GameResourcesPath, gameRelativeResourcePath);
                if (!File.Exists(gameResourceFile))
                {
                    if (optional)
                        return default;
                    throw new Exception($"Resource could not be loaded. The resource {gameResourceFile} doesn't exist.");
                }

                var newResource = resourceProvider.ReadFile<T>(gameResourceFile);
                _originalResources.Add(gameRelativeResourcePath, JsonClone(newResource));
                _resources.Add(gameRelativeResourcePath, newResource);

                _logger.LogInformation("Loaded Resource {GameResource} with provider {ResourceProvider}", gameRelativeResourcePath, resourceProvider.GetType().Name);

                return JsonClone(newResource);
            }

            if (optional)
                return default;

            throw new Exception("Resource could not be loaded. Make sure that a proper resource provider is configured");
        }

        public void UnloadResources()
        {
            _resources.Clear();
            _originalResources.Clear();
        }

        public void ResetResource()
        {
            //Cannot be used until JsonClone is restored
            _resources.Clear();
            foreach (var resource in _originalResources)
            {
                _resources.Add(resource.Key, JsonClone(resource.Value));
            }
        }


        public bool WriteChanges()
        {
            var outputPath = _config.CurrentValue.OutputPath;
            var gameResourcesPath = _config.CurrentValue.GameResourcesPath;

            _logger.LogDebug("Write State Changes to {OutputPath}", _config.CurrentValue.OutputPath);

            foreach (var resource in _resources)
            {
                var outputResourceFile = Path.Combine(outputPath, resource.Key);
                var inputResourceFile = Path.Combine(gameResourcesPath, resource.Key);
                Directory.CreateDirectory(Path.GetDirectoryName(outputResourceFile));
                var resourceProvider = GetResourceProvider(resource.Key);

                _logger.LogInformation("Write State Changes for Resource {GameResource} to {OutputPath} using provider {ResourceProvider}", resource.Key, outputResourceFile, resourceProvider.GetType().Name);

                if (!resourceProvider.WriteFile(inputResourceFile, outputResourceFile, resource.Value))
                    return false;
            }

            return true;
        }

        public bool Init()
        {
            _logger.LogInformation("Resources Path: {ResourcesPath}", _config.CurrentValue.ResourcesPath);
            _logger.LogInformation("Output Path: {OutputPath}", _config.CurrentValue.OutputPath);
            _logger.LogInformation("Game Resource Path: {GameResourcesPath}", _config.CurrentValue.GameResourcesPath);
            _logger.LogInformation("Log Path: {LogPath}", _config.CurrentValue.LogPath);
            _logger.LogInformation("Temp Path: {TempPath}", _config.CurrentValue.TempPath);
            _logger.LogInformation("Tools Path: {ToolsPath}", _config.CurrentValue.ToolsPath);

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

        private T JsonClone<T>(T input)
        {
            //Removed for now because resources are not being reset through ResetResource
            return input; // JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(input));
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
                    if (!providers.ContainsKey(attribute.ExtensionOrPath))
                        providers.Add(attribute.ExtensionOrPath, service);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "A Resource Provider could not load. Please make sure that all the file paths are correct.");
            }

            return providers;
        }
    }
}
