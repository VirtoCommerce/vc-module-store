using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// A store's setup across the parts that modules register. It checks no permissions, as no service does: the tools check
// each part's permissions.
public interface IStoreSetupService
{
    // Every registered part: Store's own parts first, then the others by name.
    IList<IStoreSetupPart> GetParts();

    // What the store is missing, from every part that checks. A part whose check fails gives one blocking finding.
    Task<IList<StoreSetupFinding>> CheckAsync(Store store, string currency = null, string country = null);
}
