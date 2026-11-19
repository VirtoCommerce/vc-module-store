using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

public static class StoreSetupFindings
{
    // A finding about a store, on the store's screen in the back office or on one of its tabs, such as "Payment methods";
    // the part is its category. The caller fills in what is missing, why it matters, the figures and the fix.
    public static McpFinding ForStore(Store store, string part, string id, string severity, string tab = null)
    {
        var storeName = store.Name ?? store.Id;

        var finding = AbstractTypeFactory<McpFinding>.TryCreateInstance();
        finding.Id = id;
        finding.Category = part;
        finding.Severity = severity;
        finding.Screen = string.IsNullOrEmpty(tab) ? $"Stores > {storeName}" : $"Stores > {storeName} > {tab}";
        finding.ObjectType = nameof(Store);
        finding.ObjectId = store.Id;
        finding.ObjectName = store.Name;

        return finding;
    }
}
