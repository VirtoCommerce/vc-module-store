using System.Collections.Generic;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// The findings of a store's setup as every part writes them. Store registers StoreSetupFindings; a solution changes how
// every part writes them by registering its own.
public interface IStoreSetupFindings
{
    // A finding about a store, on the store's screen in the back office or on one of its tabs, such as "Payment methods";
    // the part is its category. The caller fills in what is missing, why it matters, the figures and the fix.
    McpFinding ForStore(Store store, string part, string id, string severity, string tab = null);

    // The evidence of empty required settings (IStoreSetupSettings.GetEmptyRequiredSettings), with secrets counted, never
    // named: "Empty: Merchant name. 1 secret setting is empty."
    string GetEmptySettingsEvidence(IList<string> names, int secretCount);
}
