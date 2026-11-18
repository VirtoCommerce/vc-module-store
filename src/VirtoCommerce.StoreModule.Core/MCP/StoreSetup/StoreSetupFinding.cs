namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// One thing a store is missing. It holds sentences only: never a secret, an exception message or a trace.
public class StoreSetupFinding
{
    // A stable id of the kind of finding, such as "payment-no-active-method".
    public string Id { get; set; }

    public string Part { get; set; }

    public StoreSetupFindingSeverity Severity { get; set; }

    // What is missing.
    public string Title { get; set; }

    // Why it matters.
    public string Why { get; set; }

    // The figures behind it, such as "0 of 3 payment methods are active".
    public string Evidence { get; set; }

    // What to do, without tool names.
    public string Fix { get; set; }

    // Where to do it in the back office, such as "Stores > EU store > Payment methods".
    public string Screen { get; set; }

    public string ObjectType { get; set; }

    public string ObjectId { get; set; }

    public string ObjectName { get; set; }
}
