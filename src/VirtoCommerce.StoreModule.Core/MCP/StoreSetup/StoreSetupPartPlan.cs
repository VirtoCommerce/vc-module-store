using System.Collections.Generic;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// What applying a part's section would change. A part derives it to carry the state after the change for its check.
public class StoreSetupPartPlan
{
    public string Part { get; set; }

    public IList<StoreSetupChange> Changes { get; set; } = [];

    // Why the section cannot be applied. Any error refuses the whole request.
    public IList<string> Errors { get; set; } = [];

    // What the approver should know, such as a method being switched off.
    public IList<string> Warnings { get; set; } = [];
}
