using System;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

// A part whose plan names the permissions it needs (RequiredPermissions), as the Catalog module's part does: what the plan
// does decides them, not whether the store is new.
public class TestPlannedPermissionsPart(string name, params string[] requiredPermissions) : IStoreSetupSection
{
    public string Name => name;

    public string Title => $"Test part {name}";

    public McpSetupPermissions Permissions { get; init; } = new() { Read = "catalog:read", Update = "catalog:update", Create = "catalog:create" };

    public Type SectionType => typeof(TestCatalogSection);

    public bool IsRequiredForNewStore => false;

    // An error that names the part's data, as a refusal may.
    public string Error { get; init; }

    public Task<McpSetupPartPlan> PlanAsync(StoreSetupContext context, object section)
    {
        // A new store's row needs a catalog.
        context.Store.Catalog ??= "catalog-1";

        var plan = new McpSetupPartPlan().AddChange("name", null, ((TestCatalogSection)section).Name);

        foreach (var permission in requiredPermissions)
        {
            plan.RequirePermission(permission);
        }

        if (Error != null)
        {
            plan.Errors.Add(Error);
        }

        return Task.FromResult(plan);
    }

    public Task ApplyAsync(StoreSetupContext context, McpSetupPartPlan plan)
    {
        return Task.CompletedTask;
    }
}
