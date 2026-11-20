using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ModelContextProtocol;
using VirtoCommerce.MCP.Core;
using VirtoCommerce.MCP.Core.Extensions;
using VirtoCommerce.MCP.Core.Models;
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
public class StoreAccess(IStoreService storeService, IStoreSearchService storeSearchService, IStoreSetupService setupService)
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

    // Searches only the stores the user may read: ids the criteria name already are narrowed to those.
    public virtual async Task<StoreSearchResult> SearchReadableStoresAsync(ClaimsPrincipal user, StoreSearchCriteria criteria)
    {
        var storeIds = GetReadableStoreIds(user);

        if (storeIds != null)
        {
            var readableIds = criteria.ObjectIds.IsNullOrEmpty() ? storeIds : criteria.ObjectIds.Where(storeIds.ContainsIgnoreCase).ToList();

            // An empty id list would not filter the search at all.
            if (readableIds.Count == 0)
            {
                return AbstractTypeFactory<StoreSearchResult>.TryCreateInstance();
            }

            criteria.ObjectIds = readableIds;
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
    public virtual bool CanRead(ClaimsPrincipal user, IStoreSetupPart part, string storeId)
    {
        return IsAllowed(user, part.Permissions?.Read, part.Permissions?.ScopeType, storeId);
    }

    // Whether the user may read the part for some store, as the setup options name no store: a global grant, or, for a
    // part limited to stores, a grant for any of them.
    public virtual bool CanReadForSomeStore(ClaimsPrincipal user, IStoreSetupPart part)
    {
        var scopeType = part.Permissions?.ScopeType;
        var grant = user.GetMcpPermissionGrant(part.Permissions?.Read);

        return grant != null && (grant.IsGlobal || (!string.IsNullOrEmpty(scopeType) && grant.GetScopeValues(scopeType).Count > 0));
    }

    // What a user needs to see a part's state, choices and findings: "needs tax:read". A part whose module names no read
    // permission shows to no one.
    public virtual string GetReadRequirement(IStoreSetupPart part)
    {
        var permission = part.Permissions?.Read;

        return string.IsNullOrEmpty(permission) ? "its module names no permission to read it" : $"needs {permission}";
    }

    // The findings the user may read on the store: a finding belongs to the part named by its category and shows when the
    // part's state does (CanRead). The others are counted per part, with the permission that shows them, and are still
    // counted wherever readiness is decided.
    public virtual (IList<T> Shown, IList<string> NotShown) SplitReadableFindings<T>(ClaimsPrincipal user, string storeId, IEnumerable<T> findings)
        where T : McpFinding
    {
        var parts = setupService.GetParts();
        // All the findings of a part show, or none do, so each part is decided once.
        var readableParts = parts.Where(x => CanRead(user, x, storeId)).ToHashSet();
        var shown = new List<T>();
        var hidden = new List<(IStoreSetupPart Part, T Finding)>();

        foreach (var finding in findings)
        {
            var part = FindPart(parts, finding.Category);

            if (part == null || readableParts.Contains(part))
            {
                shown.Add(finding);
            }
            else
            {
                hidden.Add((part, finding));
            }
        }

        var notShown = hidden
            .GroupBy(x => x.Part)
            .Select(x => GetNotShownNote(x.Key, x.Select(y => (McpFinding)y.Finding).ToList()))
            .ToList();

        return (shown, notShown);
    }

    // The findings that block going live, as the user may read them: the titles of the shown ones, and a note per part
    // for the others.
    public virtual IList<string> GetBlockingNames<T>(ClaimsPrincipal user, string storeId, IEnumerable<T> findings)
        where T : McpFinding
    {
        var (shown, notShown) = SplitReadableFindings(user, storeId, findings.Where(x => setupService.BlocksGoLive(x)));

        return shown.Select(x => x.Title).Concat(notShown).ToList();
    }

    // "Tax provider: 2 findings, 1 blocking going live", a line per part with findings: an applied change names no
    // finding, as its requester reads its answer later and may not read every part.
    public virtual IList<string> CountByPart<T>(IEnumerable<T> findings)
        where T : McpFinding
    {
        var parts = setupService.GetParts();

        return findings
            .GroupBy(x => x.Category, StringComparer.OrdinalIgnoreCase)
            .Select(x => GetCountText(FindPart(parts, x.Key)?.Title ?? x.Key, x.ToList<McpFinding>(), string.Empty))
            .ToList();
    }

    public virtual void EnsureAllowed(ClaimsPrincipal user, string permission, string scopeType, string storeId)
    {
        if (!IsAllowed(user, permission, scopeType, storeId))
        {
            throw new McpForbiddenException($"Your {permission} permission does not cover store '{storeId}'.");
        }
    }

    // A permission whose module cannot limit it to stores (no scope type) counts only when it is granted globally.
    public virtual bool IsAllowed(ClaimsPrincipal user, string permission, string scopeType, string storeId)
    {
        var grant = user.GetMcpPermissionGrant(permission);

        return grant != null && (string.IsNullOrEmpty(scopeType) ? grant.IsGlobal : grant.Allows(scopeType, storeId));
    }

    // "Tax provider: 2 findings not shown, 1 blocking going live (needs tax:read)".
    protected virtual string GetNotShownNote(IStoreSetupPart part, IList<McpFinding> findings)
    {
        return $"{GetCountText(part.Title, findings, " not shown")} ({GetReadRequirement(part)})";
    }

    // "Tax provider: 2 findings<suffix>, 1 blocking going live"
    protected virtual string GetCountText(string title, IList<McpFinding> findings, string suffix)
    {
        var count = findings.Count == 1 ? "1 finding" : $"{findings.Count} findings";
        var blocking = findings.Count(setupService.BlocksGoLive);
        var blockingText = blocking > 0 ? $", {blocking} blocking going live" : string.Empty;

        return $"{title}: {count}{suffix}{blockingText}";
    }

    protected virtual IStoreSetupPart FindPart(IEnumerable<IStoreSetupPart> parts, string name)
    {
        return parts.FirstOrDefault(x => x.Name.EqualsIgnoreCase(name));
    }
}
