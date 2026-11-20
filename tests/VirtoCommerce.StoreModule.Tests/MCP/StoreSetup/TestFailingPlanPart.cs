using System;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

// A part whose plan fails, as a part whose module or third party fails, with a message that must not reach the answer.
public class TestFailingPlanPart(string name) : IStoreSetupSection
{
    public const string ExceptionMessage = "Gateway rejected token sk_live_4242";

    public string Name => name;

    public string Title => $"Test part {name}";

    public McpSetupPermissions Permissions => StoreSetupPermissions.ForStore();

    public Type SectionType => typeof(TestSection);

    public bool IsRequiredForNewStore => false;

    public Task<McpSetupPartPlan> PlanAsync(StoreSetupContext context, object section)
    {
        throw new InvalidOperationException(ExceptionMessage);
    }

    public Task ApplyAsync(StoreSetupContext context, McpSetupPartPlan plan)
    {
        throw new NotSupportedException();
    }
}
