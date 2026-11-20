using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

// A part that changes the store's lists in place, as a module's part may.
public class TestListChangingPart : IStoreSetupCheck
{
    public string Name => "lists";

    public string Title => "Test part lists";

    public McpSetupPermissions Permissions => StoreSetupPermissions.ForStore();

    public Task<IList<McpFinding>> CheckAsync(StoreSetupContext context)
    {
        context.Store.Languages.Add("de-DE");
        context.Store.Currencies.Add("EUR");

        return Task.FromResult<IList<McpFinding>>([]);
    }
}
