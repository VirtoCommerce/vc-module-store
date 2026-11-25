using System;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// Applies the part's section of a setup. Planning has no side effects. Applying reads the current data again and updates
// it in place, so applying the same section twice changes nothing the second time.
public interface IStoreSetupSection : IStoreSetupPart
{
    // The class of the section, read from the JSON of a request and described to the agent as a JSON schema.
    Type SectionType { get; }

    bool IsRequiredForNewStore { get; }

    // Any error of the plan refuses the whole request.
    Task<McpSetupPartPlan> PlanAsync(StoreSetupContext context, object section);

    Task ApplyAsync(StoreSetupContext context, McpSetupPartPlan plan);
}
