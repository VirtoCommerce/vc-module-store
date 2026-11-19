using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

// A part whose choices are its own module's data, guarded by a permission granted globally only, as the catalogs are:
// lists, then kinds, as the basics list languages, then currencies.
public class TestChoicesPart(int count, int kindCount = 0) : IStoreSetupChoices
{
    public const string ReadPermission = "test:read";

    public string Name => "lists";

    public string Title => "Test lists";

    public McpSetupPermissions Permissions => new() { Read = ReadPermission, Update = "test:update", Create = "test:create" };

    public Task<IList<McpSetupChoice>> GetChoicesAsync()
    {
        return Task.FromResult<IList<McpSetupChoice>>(Enumerable.Range(1, count)
            .Select(x => new McpSetupChoice { Field = "lists.id", Value = $"list-{x:00}", Title = $"List {x:00}" })
            .Concat(Enumerable.Range(1, kindCount).Select(x => new McpSetupChoice { Field = "lists.kind", Value = $"kind-{x}", Title = $"Kind {x}" }))
            .ToList());
    }
}
