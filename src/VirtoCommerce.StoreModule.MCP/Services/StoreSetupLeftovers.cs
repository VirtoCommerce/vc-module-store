using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.MCP.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.StoreModule.MCP.Services;

// Tells a store left closed by a failed create_configured_store from a store made in the back office or by a create
// that succeeded. Only the first may be continued by another create; the others are changed with apply_store_setup.
public class StoreSetupLeftovers(IMcpPendingChangeSearchService pendingChangeSearchService)
{
    public const string CreateToolName = "create_configured_store";

    // How many of the create tool's latest changes are read to find the last one for a store. A pending change has no
    // store column: the store id is only inside its arguments, so the rows are read and their arguments parsed. A failed
    // create is repeated soon after; past this many creates, a leftover is refused like any store that exists.
    private const int MaxCreateChangesToRead = 100;

    // The failed create that left the store, or null when the last create that ran for it succeeded or none did.
    public virtual async Task<McpPendingChange> FindFailedCreateAsync(string storeId)
    {
        var criteria = AbstractTypeFactory<McpPendingChangeSearchCriteria>.TryCreateInstance();
        criteria.ToolName = CreateToolName;
        criteria.Statuses = [McpPendingChangeStatus.Applied, McpPendingChangeStatus.Failed];
        criteria.Sort = $"{nameof(McpPendingChange.CreatedDate)}:desc";
        criteria.Take = MaxCreateChangesToRead;

        var result = await pendingChangeSearchService.SearchAsync(criteria);
        var lastChange = result.Results.FirstOrDefault(x => GetStoreId(x.Arguments).EqualsIgnoreCase(storeId));

        return lastChange?.Status == McpPendingChangeStatus.Failed ? lastChange : null;
    }

    private static string GetStoreId(string arguments)
    {
        if (string.IsNullOrEmpty(arguments))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(arguments);

            return document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("store_id", out var storeId)
                && storeId.ValueKind == JsonValueKind.String
                    ? storeId.GetString()
                    : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
