using Microsoft.Extensions.Configuration;
using Sm5sh;
using Sm5sh.Interfaces;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class Sm5shCoreDependencyInjectionExtensions
    {
        public static IServiceCollection AddSm5shCore(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<Sm5shOptions>(configuration);
            services.AddSingleton<IProcessService, ProcessService>();
            services.AddSingleton<IStateManager, StateManager>();
            return services;
        }
    }
}
