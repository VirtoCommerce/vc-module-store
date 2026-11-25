using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using VirtoCommerce.MCP.Core;
using VirtoCommerce.MCP.Core.Attributes;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.MCP.Services;
using StorePermissions = VirtoCommerce.StoreModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.StoreModule.MCP.Tools;

// Opening and closing a store with the existing store state. The refusal to open a store that is not ready lives here
// only: the back office and the REST API open stores as before.
[McpServerToolType]
[McpScope(McpScopes.Admin)]
[McpPermission(StorePermissions.Update)]
public class StoreStateTools(StoreAccess access, IStoreService storeService, IStoreSetupService setupService)
{
    [McpServerTool(Name = "open_store", ReadOnly = false, Destructive = false, Idempotent = true)]
    [McpRequiresApproval]
    [Description("Open a closed or restricted store to customers. Refused while the readiness report has blocking " +
        "findings, which the refusal names. The change is submitted for approval: tell the user an approver must approve it.")]
    public async Task<McpChangePreview> OpenStore(
        RequestContext<CallToolRequestParams> context,
        [Description("Store id, as list_stores returns it.")] string store_id,
        bool dryRun = false)
    {
        var store = await access.GetUpdatableStoreAsync(context.User, store_id);

        if (store.StoreState == StoreState.Open)
        {
            throw new McpException($"Store '{store.Id}' is already open.");
        }

        // Checked again when the approved change runs, so a store that stopped being ready in the meantime stays closed.
        var findings = await setupService.CheckAsync(store);

        // Every blocking finding keeps the store closed.
        if (findings.Any(setupService.BlocksGoLive))
        {
            throw new McpException(GetBlockingMessage(context.User, store, findings, dryRun));
        }

        var preview = CreatePreview("open", store, StoreState.Open, dryRun);

        foreach (var warning in GetWarnings(context.User, store, findings, dryRun))
        {
            preview.AddWarning(warning);
        }

        preview.AddWarning("Customers can see the store and buy as soon as it opens.");

        if (!dryRun)
        {
            await SetStateAsync(store.Id, StoreState.Open);
        }

        return preview;
    }

    [McpServerTool(Name = "close_store", ReadOnly = false, Destructive = true, Idempotent = true)]
    [McpRequiresApproval]
    [Description("Close a store: customers can no longer see it or buy. The change is submitted for approval: tell the " +
        "user an approver must approve it.")]
    public async Task<McpChangePreview> CloseStore(
        RequestContext<CallToolRequestParams> context,
        [Description("Store id, as list_stores returns it.")] string store_id,
        [Description("Why the store closes; it stays with the change in the Agent audit log.")] string reason = null,
        bool dryRun = false)
    {
        var store = await access.GetUpdatableStoreAsync(context.User, store_id);

        if (store.StoreState == StoreState.Closed)
        {
            throw new McpException($"Store '{store.Id}' is already closed.");
        }

        var preview = CreatePreview("close", store, StoreState.Closed, dryRun);
        preview.AddWarning("Customers can no longer see the store or buy.");

        if (!string.IsNullOrWhiteSpace(reason))
        {
            preview.AddWarning($"Reason: {reason.Trim()}");
        }

        if (!dryRun)
        {
            await SetStateAsync(store.Id, StoreState.Closed);
        }

        return preview;
    }

    // A dry run names the blocking findings as the caller may read them: one of a part the caller may not read by its part
    // only. An approved change runs as its approver, and the requester reads its answer later (get_pending_change), so it
    // names no finding: get_store_readiness_report lists them for each reader.
    protected virtual string GetBlockingMessage(ClaimsPrincipal user, Store store, IList<McpFinding> findings, bool dryRun)
    {
        if (dryRun)
        {
            var blocking = access.GetBlockingNames(user, store.Id, findings);

            return $"Store '{store.Id}' cannot open while these block it: {string.Join("; ", blocking)}. get_store_readiness_report says how to fix each.";
        }

        var count = findings.Count(setupService.BlocksGoLive);

        return $"Store '{store.Id}' cannot open: {(count == 1 ? "1 finding blocks" : $"{count} findings block")} it. " +
            "get_store_readiness_report lists them and says how to fix each.";
    }

    // The warnings a store opens with, as GetBlockingMessage gives what blocks it: a dry run names them as the caller may
    // read them and counts the others by part, with the permission that shows them; an approved change counts them all by
    // part.
    protected virtual IList<string> GetWarnings(ClaimsPrincipal user, Store store, IList<McpFinding> findings, bool dryRun)
    {
        var warnings = findings.Where(x => McpFindingSeverities.Warning.EqualsIgnoreCase(x.Severity)).ToList();

        if (dryRun)
        {
            var (shown, notShown) = access.SplitReadableFindings(user, store.Id, warnings);
            var previewWarnings = shown.Select(x => $"{x.Title}.").ToList();

            if (notShown.Count > 0)
            {
                previewWarnings.Add($"It opens with warnings: {string.Join("; ", notShown)}.");
            }

            return previewWarnings;
        }

        return warnings.Count == 0
            ? []
            : [$"It opens with warnings: {string.Join("; ", access.CountByPart(warnings))}. get_store_readiness_report lists them."];
    }

    protected virtual McpChangePreview CreatePreview(string action, Store store, StoreState state, bool dryRun)
    {
        var preview = AbstractTypeFactory<McpChangePreview>.TryCreateInstance();
        preview.Action = action;
        preview.ObjectType = "store";
        preview.ObjectId = store.Id;
        preview.ObjectName = store.Name;
        preview.DryRun = dryRun;

        return preview.AddChange("state", StoreStateNames.ToName(store.StoreState), StoreStateNames.ToName(state));
    }

    // The saved store again, so a change made since the preview is not overwritten with old data.
    protected virtual async Task SetStateAsync(string storeId, StoreState state)
    {
        var store = await storeService.GetByIdAsync(storeId)
            ?? throw new McpException($"Store '{storeId}' was not found.");
        store.StoreState = state;
        await storeService.SaveChangesAsync([store]);
    }
}
