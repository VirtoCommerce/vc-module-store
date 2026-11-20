using System.Collections.Generic;
using VirtoCommerce.MCP.Core.Models;

namespace VirtoCommerce.StoreModule.MCP.Models;

// The preview of a change that spans parts, in one McpChangePreview: the Agent approvals screen shows its object, its
// changes ("<part>: <field>") and its warnings. The same shape answers the dry run and the real run.
public class StoreSetupPreview : McpChangePreview
{
    public IList<StoreSetupPartRow> Parts { get; set; } = [];

    // What the store would miss after the change, or misses now after it, from the parts the user may read.
    public IList<StoreFinding> MissingAfter { get; set; } = [];

    // The findings of the parts the user may not read, counted per part with the permission that shows them.
    public IList<string> NotShown { get; set; } = [];

    // An applied change: what the store misses, counted by part, instead of MissingAfter and NotShown. It runs as the
    // approver, and its answer is kept for the requester, who may not read every part.
    public IList<string> MissingAfterByPart { get; set; }

    public string BackOffice { get; set; }

    public string NextStep { get; set; }
}
