using System;
using System.Collections.Generic;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

public class StoreSetupContext
{
    // A copy of the store as it will be after the sections planned so far, never a cached instance.
    public Store Store { get; set; }

    public bool IsNewStore { get; set; }

    // The plans of the request by part name; empty for the readiness report.
    public IDictionary<string, McpSetupPartPlan> Plans { get; set; } = new Dictionary<string, McpSetupPartPlan>(StringComparer.OrdinalIgnoreCase);

    // The readiness of one currency, or null.
    public string Currency { get; set; }

    // The readiness of one country, or null.
    public string Country { get; set; }
}
