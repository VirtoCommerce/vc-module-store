using System.Collections.Generic;
using VirtoCommerce.MCP.Core.Models;

namespace VirtoCommerce.StoreModule.MCP.Models;

// The preview of a change that spans parts, in one McpChangePreview: the Agent approvals screen shows its object, its
// changes ("<part>: <field>") and its warnings. The same shape answers the dry run and the real run.
public class StoreSetupPreview : McpChangePreview
{
    public IList<StoreSetupPartRow> Parts { get; set; } = [];

    // What the store would miss after the change, or misses now after it.
    public IList<StoreFinding> MissingAfter { get; set; } = [];

    public string BackOffice { get; set; }

    public string NextStep { get; set; }
}
