using System.Collections.Generic;
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

    // The evidence of empty required settings (IStoreSetupSettings.GetEmptyRequiredSettings), with secrets counted, never
    // named: "Empty: Merchant name. 1 secret setting is empty."
    public static string GetEmptySettingsEvidence(IList<string> names, int secretCount)
    {
        var parts = new List<string>();

        if (names?.Count > 0)
        {
            parts.Add($"Empty: {string.Join(", ", names)}.");
        }

        if (secretCount > 0)
        {
            parts.Add(secretCount == 1 ? "1 secret setting is empty." : $"{secretCount} secret settings are empty.");
        }

        return string.Join(" ", parts);
    }
}
