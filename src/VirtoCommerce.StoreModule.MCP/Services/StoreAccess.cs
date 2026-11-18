using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ModelContextProtocol;
using VirtoCommerce.MCP.Core;
using VirtoCommerce.MCP.Core.Extensions;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model.Search;
using VirtoCommerce.StoreModule.Core.Security;
using VirtoCommerce.StoreModule.Core.Services;
using StorePermissions = VirtoCommerce.StoreModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.StoreModule.MCP.Services;

// The stores a user may work with. A permission limited to stores (StoreSelectedScope) is enforced here, inside the
// tools: the tools are visible to such a user, and every call is narrowed to the user's stores.
public class StoreAccess(IStoreService storeService, IStoreSearchService storeSearchService)
{
    public static readonly string StoreScopeType = nameof(StoreSelectedScope);

    // The ids of the stores the user may read, or null when the user may read every store.
    public virtual IList<string> GetReadableStoreIds(ClaimsPrincipal user)
    {
        var grant = user.GetMcpPermissionGrant(StorePermissions.Read);

        if (grant == null)
        {
            return [];
        }

        return grant.IsGlobal ? null : grant.GetScopeValues(StoreScopeType);
    }

    // Searches only the stores the user may read.
    public virtual async Task<StoreSearchResult> SearchReadableStoresAsync(ClaimsPrincipal user, StoreSearchCriteria criteria)
    {
        var storeIds = GetReadableStoreIds(user);

        if (storeIds != null)
        {
            // An empty id list would not filter the search at all.
            if (storeIds.Count == 0)
            {
                return AbstractTypeFactory<StoreSearchResult>.TryCreateInstance();
            }

            criteria.ObjectIds = storeIds;
        }

        return await storeSearchService.SearchAsync(criteria);
    }

    public virtual async Task<Store> GetReadableStoreAsync(ClaimsPrincipal user, string storeId)
    {
        if (string.IsNullOrWhiteSpace(storeId))
        {
            throw new McpException("store_id is required. Use list_stores to find the store's id.");
        }

        EnsureAllowed(user, StorePermissions.Read, StoreScopeType, storeId);

        return await storeService.GetByIdAsync(storeId)
            ?? throw new McpException($"Store '{storeId}' was not found. Use list_stores to find the store's id.");
    }

    // A change needs the update permission for the store, as well as read access to it.
    public virtual async Task<Store> GetUpdatableStoreAsync(ClaimsPrincipal user, string storeId)
    {
        var store = await GetReadableStoreAsync(user, storeId);
        EnsureAllowed(user, StorePermissions.Update, StoreScopeType, store.Id);

        return store;
    }

    // Whether the user may read a part's state on the store: the part's own read permission, limited by its scope type.
    public static bool CanRead(ClaimsPrincipal user, IStoreSetupPart part, string storeId)
    {
        return IsAllowed(user, part.Permissions?.Read, part.Permissions?.StoreScopeType, storeId);
    }

    public static void EnsureAllowed(ClaimsPrincipal user, string permission, string scopeType, string storeId)
    {
        if (!IsAllowed(user, permission, scopeType, storeId))
        {
            throw new McpForbiddenException($"Your {permission} permission does not cover store '{storeId}'.");
        }
    }

    // A permission whose module cannot limit it to stores (no scope type) counts only when it is granted globally.
    public static bool IsAllowed(ClaimsPrincipal user, string permission, string scopeType, string storeId)
    {
        var grant = user.GetMcpPermissionGrant(permission);

        return grant != null && (string.IsNullOrEmpty(scopeType) ? grant.IsGlobal : grant.Allows(scopeType, storeId));
    }
}
