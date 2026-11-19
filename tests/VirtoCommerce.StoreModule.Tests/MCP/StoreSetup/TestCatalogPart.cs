using System;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

// The Catalog module's part as Store's tools meet it: a new store's catalog is created by its name, so the store is saved
// with a catalog id, which its row requires.
public class TestCatalogPart : IStoreSetupSection
{
    public const string PartName = "catalog";

    public string Name => PartName;

    public string Title => "Catalog";

    public McpSetupPermissions Permissions => StoreSetupPermissions.ForStore();

    public Type SectionType => typeof(TestCatalogSection);

    public bool IsRequiredForNewStore => true;

    public Task<McpSetupPartPlan> PlanAsync(StoreSetupContext context, object section)
    {
        var name = ((TestCatalogSection)section).Name;
        var plan = new McpSetupPartPlan();

        if (string.IsNullOrEmpty(context.Store.Catalog) && !string.IsNullOrEmpty(name))
        {
            context.Store.Catalog = $"catalog-{name}";
            plan.AddChange("name", null, name);
        }

        return Task.FromResult(plan);
    }

    public Task ApplyAsync(StoreSetupContext context, McpSetupPartPlan plan)
    {
        return Task.CompletedTask;
    }
}
