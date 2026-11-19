using System.Collections.Generic;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

public class StoreSetupResult
{
    public string StoreId { get; set; }

    // The store as planned, or as saved after applying.
    public Store Store { get; set; }

    // The parts the request has a section for, in their order.
    public IList<McpSetupPartResult> Parts { get; set; } = [];

    // Why the request is refused: a section no part takes, a missing required section, the parts' own errors.
    public IList<string> Errors { get; set; } = [];

    // What the store would miss after the change, for a plan; what it misses now, after applying.
    public IList<McpFinding> Findings { get; set; } = [];
}
