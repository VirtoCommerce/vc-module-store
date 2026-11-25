using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.MCP.Core.Extensions;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.MCP.Models;

namespace VirtoCommerce.StoreModule.MCP.Services;

// The parts' findings as the tools return them (StoreFinding), by the setup service's parts and its rule for going live.
public class StoreFindingRows(IStoreSetupService setupService)
{
    // The findings as rows, most severe first.
    // inCreate: before a create the store does not exist, so the fix of a part that takes a section sends it in the same
    // create call.
    public virtual IList<StoreFinding> CreateList(IEnumerable<McpFinding> findings, string backOfficeLink, bool inCreate = false)
    {
        var sectionParts = setupService.GetParts().OfType<IStoreSetupSection>().Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        return findings
            .OrderBySeverity()
            .Select(x => Create(x, sectionParts.Contains(x.Category), setupService.BlocksGoLive(x), backOfficeLink, inCreate))
            .ToList();
    }

    // The fix names the tool when the part takes a section, and the back-office screen with its link when there is one.
    public virtual StoreFinding Create(McpFinding finding, bool partTakesSection, bool blocksGoLive, string backOfficeLink, bool inCreate = false)
    {
        ArgumentNullException.ThrowIfNull(finding);

        var row = AbstractTypeFactory<StoreFinding>.TryCreateInstance();
        CopyFinding(finding, row);
        row.Fix = GetFix(finding, partTakesSection, backOfficeLink, inCreate);
        row.BlocksGoLive = blocksGoLive;

        return row;
    }

    // Every field of the part's finding: a test sets each one, so a field McpFinding gains is not left behind.
    protected virtual void CopyFinding(McpFinding finding, StoreFinding row)
    {
        row.Id = finding.Id;
        row.Severity = finding.Severity;
        row.Title = finding.Title;
        row.Why = finding.Why;
        row.Evidence = finding.Evidence;
        row.Fix = finding.Fix;
        row.Screen = finding.Screen;
        row.ManualOnly = finding.ManualOnly;
        row.Category = finding.Category;
        row.ObjectType = finding.ObjectType;
        row.ObjectId = finding.ObjectId;
        row.ObjectName = finding.ObjectName;
    }

    protected virtual string GetFix(McpFinding finding, bool partTakesSection, string backOfficeLink, bool inCreate)
    {
        var fix = new List<string>();

        if (!string.IsNullOrEmpty(finding.Fix))
        {
            fix.Add(finding.Fix);
        }

        // A finding only a person can fix, such as an empty secret, names no tool.
        if (partTakesSection && !finding.ManualOnly)
        {
            fix.Add(inCreate
                ? $"Send the '{finding.Category}' section in the same create_configured_store call."
                : $"Tool: apply_store_setup with the '{finding.Category}' section.");
        }

        if (!string.IsNullOrEmpty(finding.Screen))
        {
            fix.Add($"Back office: {finding.Screen.WithLink(backOfficeLink)}.");
        }

        return string.Join(" ", fix);
    }
}
