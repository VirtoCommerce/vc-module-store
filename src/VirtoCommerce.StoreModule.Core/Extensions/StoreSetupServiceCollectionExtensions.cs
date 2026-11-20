using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

namespace VirtoCommerce.StoreModule.Core.Extensions;

public static class StoreSetupServiceCollectionExtensions
{
    // Adds a module's part of a store's setup. A module that is not installed registers nothing, so it has no part.
    // A part's name is its key: a part registered later with the same name replaces it, which is how a module overrides
    // another module's part with a subclass.
    public static IServiceCollection AddStoreSetupPart<TPart>(this IServiceCollection services)
        where TPart : class, IStoreSetupPart
    {
        services.AddTransient<IStoreSetupPart, TPart>();
        return services;
    }
}
