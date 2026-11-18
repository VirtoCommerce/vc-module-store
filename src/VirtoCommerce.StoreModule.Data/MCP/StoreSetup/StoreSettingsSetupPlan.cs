using System;
using System.Collections.Generic;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

namespace VirtoCommerce.StoreModule.Data.MCP.StoreSetup;

// The plan of the settings section: the values to save, by setting name.
public class StoreSettingsSetupPlan : StoreSetupPartPlan
{
    public IDictionary<string, object> Values { get; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
}
