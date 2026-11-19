using System.ComponentModel;
using System.Linq;
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
        // Every blocking finding keeps the store closed; one of a part the user may not read is named by its part only.
        var (shownBlocking, notShownBlocking) = access.SplitReadableFindings(context.User, store.Id, findings.Where(setupService.BlocksGoLive));

        if (shownBlocking.Count + notShownBlocking.Count > 0)
        {
            var blocking = shownBlocking.Select(x => x.Title).Concat(notShownBlocking);
            throw new McpException($"Store '{store.Id}' cannot open while these block it: {string.Join("; ", blocking)}. " +
                "get_store_readiness_report says how to fix each.");
        }

        var preview = CreatePreview("open", store, StoreState.Open, dryRun);
        var (shownWarnings, _) = access.SplitReadableFindings(context.User, store.Id, findings.Where(x => McpFindingSeverities.Warning.EqualsIgnoreCase(x.Severity)));

        foreach (var finding in shownWarnings)
        {
            preview.AddWarning($"{finding.Title}.");
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

    private static McpChangePreview CreatePreview(string action, Store store, StoreState state, bool dryRun)
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
    private async Task SetStateAsync(string storeId, StoreState state)
    {
        var store = await storeService.GetByIdAsync(storeId)
            ?? throw new McpException($"Store '{storeId}' was not found.");
        store.StoreState = state;
        await storeService.SaveChangesAsync([store]);
    }
}
