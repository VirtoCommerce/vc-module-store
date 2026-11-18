using System.Threading.Tasks;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

// A part with a state guarded by its own module's permissions, as Shipping's part will be.
public class TestStatePart(string name, StoreSetupPermissions permissions) : IStoreSetupState
{
    public string Name => name;

    public string Title => $"Test part {name}";

    public StoreSetupPermissions Permissions => permissions;

    public Task<object> GetStateAsync(Store store)
    {
        return Task.FromResult<object>(new TestSection { Methods = ["FixedRate"] });
    }
}
