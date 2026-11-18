using System.Collections.Generic;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

public class StoreSetupPartResult
{
    public string Part { get; set; }

    public string Title { get; set; }

    public StoreSetupPartStatus Status { get; set; }

    public IList<StoreSetupChange> Changes { get; set; } = [];

    public IList<string> Warnings { get; set; } = [];

    // Why the part failed to apply, without the exception's text, which goes to the log.
    public string Error { get; set; }
}
