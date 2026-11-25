using System.Collections.Generic;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// Store's way of writing the findings of a store's setup (IStoreSetupFindings).
public class StoreSetupFindings : IStoreSetupFindings
{
    public virtual McpFinding ForStore(Store store, string part, string id, string severity, string tab = null)
    {
        var finding = AbstractTypeFactory<McpFinding>.TryCreateInstance();
        finding.Id = id;
        finding.Category = part;
        finding.Severity = severity;
        finding.Screen = GetScreen(store, tab);
        finding.ObjectType = nameof(Store);
        finding.ObjectId = store.Id;
        finding.ObjectName = store.Name;

        return finding;
    }

    public virtual string GetEmptySettingsEvidence(IList<string> names, int secretCount)
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

    // "Stores > EU store", or one of its tabs: "Stores > EU store > Payment methods".
    protected virtual string GetScreen(Store store, string tab)
    {
        var storeName = store.Name ?? store.Id;

        return string.IsNullOrEmpty(tab) ? $"Stores > {storeName}" : $"Stores > {storeName} > {tab}";
    }
}
