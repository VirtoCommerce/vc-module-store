using VirtoCommerce.MCP.Core.Models;

namespace VirtoCommerce.StoreModule.MCP.Models;

// A part's finding as the tools return it (StoreFindingRows): whether it blocks going live, and the tool and the
// back-office link in its fix.
public class StoreFinding : McpFinding
{
    public bool BlocksGoLive { get; set; }
}
