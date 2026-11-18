using System.Collections.Generic;
using System.Threading.Tasks;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// What the store is missing for the part. When the request has a plan for the part, the check reads the state after the
// change from it (StoreSetupContext.Plans), so the readiness report and the preview of a change share one check.
public interface IStoreSetupCheck : IStoreSetupPart
{
    Task<IList<StoreSetupFinding>> CheckAsync(StoreSetupContext context);
}
