using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

// A part that takes a section and offers choices for it, as Shipping's part will.
public class TestSectionPart(string name) : IStoreSetupSection, IStoreSetupChoices
{
    public string Name => name;

    public string Title => $"Test part {name}";

    public McpSetupPermissions Permissions => StoreSetupPermissions.ForStore();

    public Type SectionType => typeof(TestSection);

    public bool IsRequiredForNewStore => true;

    public Task<IList<McpSetupChoice>> GetChoicesAsync()
    {
        IList<McpSetupChoice> choices =
        [
            new McpSetupChoice
            {
                Field = $"{name}.methods",
                Value = "FixedRate",
                Title = "Fixed rate",
                Settings = [new McpSetupSetting { Name = "FixedRate.Ground.Rate", Title = "Ground rate", ValueType = "Decimal" }],
            },
        ];

        return Task.FromResult(choices);
    }

    public Task<McpSetupPartPlan> PlanAsync(StoreSetupContext context, object section)
    {
        throw new NotSupportedException();
    }

    public Task ApplyAsync(StoreSetupContext context, McpSetupPartPlan plan)
    {
        throw new NotSupportedException();
    }
}
