using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// What the store is missing for the part. When the request has a plan for the part, the check reads the state after the
// change from it (StoreSetupContext.Plans), so the readiness report and the preview of a change share one check.
// A finding holds sentences only: never a secret, an exception message or a trace. Its category is the part's name, which
// the setup service sets whatever the part sends, so the part's read permission decides who sees it. Its fix names no
// tool: the tools add the tool and the back-office link. A critical finding means the store cannot go live until it is
// fixed.
public interface IStoreSetupCheck : IStoreSetupPart
{
    Task<IList<McpFinding>> CheckAsync(StoreSetupContext context);
}
