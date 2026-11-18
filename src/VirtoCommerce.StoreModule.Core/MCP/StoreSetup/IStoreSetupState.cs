using System.Threading.Tasks;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// The store's current state for the part, in the shape of its section when it has one. It never holds a secret.
public interface IStoreSetupState : IStoreSetupPart
{
    Task<object> GetStateAsync(Store store);
}
