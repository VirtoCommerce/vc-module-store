using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// A store's setup across the parts that modules register. It checks no permissions, as no service does: the tools check
// each part's permissions.
public interface IStoreSetupService
{
    // Every registered part: Store's own parts first, then the others by name.
    IList<IStoreSetupPart> GetParts();

    // What the store is missing, from every part that checks. A part whose check fails gives one critical finding.
    Task<IList<McpFinding>> CheckAsync(Store store, string currency = null, string country = null);

    // What the request would change and what the store would miss afterwards. Changes nothing.
    Task<StoreSetupResult> PlanAsync(StoreSetupRequest request);

    // Plans the request again and applies it. With any error, nothing is applied. Otherwise the parts are applied in
    // their order until one fails: an apply across modules is not a transaction, and the result says which parts were
    // applied and which were not.
    Task<StoreSetupResult> ApplyAsync(StoreSetupRequest request);
}
