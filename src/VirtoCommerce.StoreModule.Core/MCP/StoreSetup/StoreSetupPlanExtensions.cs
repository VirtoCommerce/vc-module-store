using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.MCP.Core.Models;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

public static class StoreSetupPlanExtensions
{
    // The fields a plan changes on one object, from its rows: the field of each row whose field starts with the prefix,
    // such as "active" or a setting's name for "methods.<code>.".
    public static ISet<string> GetChangedFields(this McpSetupPartPlan plan, string fieldPrefix)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(fieldPrefix);

        return plan.Changes
            .Where(x => x.Field.StartsWith(fieldPrefix, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Field[fieldPrefix.Length..])
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
