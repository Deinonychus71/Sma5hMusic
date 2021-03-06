using Microsoft.Extensions.Configuration;
using Sma5h;
using Sma5h.Interfaces;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class Sma5hCoreDependencyInjectionExtensions
    {
        public static IServiceCollection AddSma5hCore(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<Sma5hOptions>(configuration);
            services.AddSingleton<IProcessService, ProcessService>();
            services.AddSingleton<IStateManager, StateManager>();
            return services;
        }
    }
}
