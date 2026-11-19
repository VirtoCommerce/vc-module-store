using System;
using System.Collections.Generic;
using VirtoCommerce.MCP.Core.Models;

namespace VirtoCommerce.StoreModule.Data.MCP.StoreSetup;

// The plan of the settings section: the values to save, by setting name.
public class StoreSettingsSetupPlan : McpSetupPartPlan
{
    public IDictionary<string, object> Values { get; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
}
