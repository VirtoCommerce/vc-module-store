using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.MCP.Core.Extensions;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

namespace VirtoCommerce.StoreModule.MCP.Models;

// A part's finding as the tools return it: whether it blocks going live, and the tool and the back-office link in its fix.
public class StoreFinding : McpFinding
{
    public bool BlocksGoLive { get; set; }

    // The findings as rows, most severe first, by the setup service's parts and its rule for going live.
    public static IList<StoreFinding> CreateList(IEnumerable<McpFinding> findings, IStoreSetupService setupService, string backOfficeLink)
    {
        ArgumentNullException.ThrowIfNull(setupService);

        var sectionParts = setupService.GetParts().OfType<IStoreSetupSection>().Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        return findings
            .OrderBySeverity()
            .Select(x => Create(x, sectionParts.Contains(x.Category), setupService.BlocksGoLive(x), backOfficeLink))
            .ToList();
    }

    // The fix names the tool when the part takes a section, and the back-office screen with its link when there is one.
    public static StoreFinding Create(McpFinding finding, bool partTakesSection, bool blocksGoLive, string backOfficeLink)
    {
        var row = AbstractTypeFactory<StoreFinding>.TryCreateInstance();
        row.Id = finding.Id;
        row.Severity = finding.Severity;
        row.Title = finding.Title;
        row.Why = finding.Why;
        row.Evidence = finding.Evidence;
        row.Fix = GetFix(finding, partTakesSection, backOfficeLink);
        row.Screen = finding.Screen;
        row.ManualOnly = finding.ManualOnly;
        row.Category = finding.Category;
        row.ObjectType = finding.ObjectType;
        row.ObjectId = finding.ObjectId;
        row.ObjectName = finding.ObjectName;
        row.BlocksGoLive = blocksGoLive;

        return row;
    }

    private static string GetFix(McpFinding finding, bool partTakesSection, string backOfficeLink)
    {
        var fix = new List<string>();

        if (!string.IsNullOrEmpty(finding.Fix))
        {
            fix.Add(finding.Fix);
        }

        // A finding only a person can fix, such as an empty secret, names no tool.
        if (partTakesSection && !finding.ManualOnly)
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
