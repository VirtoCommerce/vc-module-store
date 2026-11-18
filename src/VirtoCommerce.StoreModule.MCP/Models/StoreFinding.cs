using System.Collections.Generic;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

namespace VirtoCommerce.StoreModule.MCP.Models;

// A store setup finding in the MCP module's shape, with whether it blocks going live and why it matters.
public class StoreFinding : McpFinding
{
    public bool BlocksGoLive { get; set; }

    public string Why { get; set; }

    // The fix names the tool when the part takes a section, and the back-office screen with its link when there is one.
    public static StoreFinding Create(StoreSetupFinding finding, bool partTakesSection, string backOfficeLink)
    {
        return new StoreFinding
        {
            Id = finding.Id,
            Severity = finding.Severity switch
            {
                StoreSetupFindingSeverity.Blocking => McpFindingSeverities.Critical,
                StoreSetupFindingSeverity.Warning => McpFindingSeverities.Warning,
                _ => McpFindingSeverities.Info,
            },
            Title = finding.Title,
            Why = finding.Why,
            Evidence = finding.Evidence,
            Fix = GetFix(finding, partTakesSection, backOfficeLink),
            Category = finding.Part,
            BlocksGoLive = finding.Severity == StoreSetupFindingSeverity.Blocking,
            ObjectType = finding.ObjectType,
            ObjectId = finding.ObjectId,
            ObjectName = finding.ObjectName,
        };
    }

    private static string GetFix(StoreSetupFinding finding, bool partTakesSection, string backOfficeLink)
    {
        var fix = new List<string>();

        if (!string.IsNullOrEmpty(finding.Fix))
        {
            fix.Add(finding.Fix);
        }

        if (partTakesSection)
        {
            fix.Add($"Tool: apply_store_setup with the '{finding.Part}' section.");
        }

        if (!string.IsNullOrEmpty(finding.Screen))
        {
            fix.Add(string.IsNullOrEmpty(backOfficeLink)
                ? $"Back office: {finding.Screen}."
                : $"Back office: {finding.Screen} ({backOfficeLink}).");
        }

        return string.Join(" ", fix);
    }
}
