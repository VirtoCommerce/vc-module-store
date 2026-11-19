using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.MCP.Core.Extensions;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

namespace VirtoCommerce.StoreModule.MCP.Models;

// A part's finding as the tools return it: whether it blocks going live, and the tool and the back-office link in its fix.
public class StoreFinding : McpFinding
{
    public bool BlocksGoLive => StoreSetupFindings.BlocksGoLive(this);

    // The findings as rows, most severe first.
    public static IList<StoreFinding> CreateList(IEnumerable<McpFinding> findings, IEnumerable<IStoreSetupPart> parts, string backOfficeLink)
    {
        var sectionParts = parts.OfType<IStoreSetupSection>().Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        return findings
            .OrderBySeverity()
            .Select(x => Create(x, sectionParts.Contains(x.Category), backOfficeLink))
            .ToList();
    }

    // The fix names the tool when the part takes a section, and the back-office screen with its link when there is one.
    public static StoreFinding Create(McpFinding finding, bool partTakesSection, string backOfficeLink)
    {
        return new StoreFinding
        {
            Id = finding.Id,
            Severity = finding.Severity,
            Title = finding.Title,
            Why = finding.Why,
            Evidence = finding.Evidence,
            Fix = GetFix(finding, partTakesSection, backOfficeLink),
            Screen = finding.Screen,
            Category = finding.Category,
            ObjectType = finding.ObjectType,
            ObjectId = finding.ObjectId,
            ObjectName = finding.ObjectName,
        };
    }

    private static string GetFix(McpFinding finding, bool partTakesSection, string backOfficeLink)
    {
        var fix = new List<string>();

        if (!string.IsNullOrEmpty(finding.Fix))
        {
            fix.Add(finding.Fix);
        }

        if (partTakesSection)
        {
            fix.Add($"Tool: apply_store_setup with the '{finding.Category}' section.");
        }

        if (!string.IsNullOrEmpty(finding.Screen))
        {
            fix.Add($"Back office: {finding.Screen.WithLink(backOfficeLink)}.");
        }

        return string.Join(" ", fix);
    }
}
