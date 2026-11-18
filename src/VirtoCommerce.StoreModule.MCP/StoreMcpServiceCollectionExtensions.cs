using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.StoreModule.MCP.Services;

namespace VirtoCommerce.StoreModule.MCP;

public static class StoreMcpServiceCollectionExtensions
{
    // The services the store admin tools use. The tools themselves are added by AddMcpCapabilities.
    public static IServiceCollection AddStoreMcpServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddTransient<StoreAccess>();
        services.AddTransient<StoreBackOfficeLinks>();

        return services;
    }
}
