using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

// A part that takes a section and offers choices for it, as Shipping's part will.
public class TestSectionPart(string name) : IStoreSetupSection, IStoreSetupChoices
{
    public string Name => name;

    public string Title => $"Test part {name}";

    public StoreSetupPermissions Permissions => StoreSetupPermissions.ForStore();

    public Type SectionType => typeof(TestSection);

    public bool IsRequiredForNewStore => true;

    public Task<IList<StoreSetupChoice>> GetChoicesAsync()
    {
        IList<StoreSetupChoice> choices =
        [
            new StoreSetupChoice
            {
                Field = $"{name}.methods",
                Value = "FixedRate",
                Title = "Fixed rate",
                Settings = [new StoreSetupSetting { Name = "FixedRate.Ground.Rate", Title = "Ground rate", ValueType = "Decimal" }],
            },
        ];

        return Task.FromResult(choices);
    }

    public Task<StoreSetupPartPlan> PlanAsync(StoreSetupContext context, object section)
    {
        throw new NotSupportedException();
    }

    public Task ApplyAsync(StoreSetupContext context, StoreSetupPartPlan plan)
    {
        throw new NotSupportedException();
    }
}
