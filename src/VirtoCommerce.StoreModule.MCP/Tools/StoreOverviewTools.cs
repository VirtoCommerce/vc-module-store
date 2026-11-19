using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using VirtoCommerce.MCP.Core;
using VirtoCommerce.MCP.Core.Attributes;
using VirtoCommerce.MCP.Core.Extensions;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.MCP.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model.Search;
using VirtoCommerce.StoreModule.MCP.Services;
using StorePermissions = VirtoCommerce.StoreModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.StoreModule.MCP.Tools;

// Orientation: the stores the user manages, and one store's whole setup.
[McpServerToolType]
[McpScope(McpScopes.Admin)]
[McpPermission(StorePermissions.Read)]
public class StoreOverviewTools(
    StoreAccess access,
    IStoreSetupService setupService,
    IMcpPageSizeResolver pageSizeResolver,
    ILogger<StoreOverviewTools> logger)
{
    private const int TopFindingCount = 3;

    [McpServerTool(Name = "get_store_overview", ReadOnly = true, Destructive = false, Idempotent = true)]
    [Description("Start here. The stores the user manages: how many are open, closed and restricted, and for each store " +
        "of the first page (closed and restricted ones first) whether it is ready to open, its blocking and warning " +
        "counts and its most important finding. Use it for a check of all stores, or to pick the store to set up or launch.")]
    public async Task<object> GetStoreOverview(
        RequestContext<CallToolRequestParams> context,
        [Description("Number of stores to return. Omit it for the default page size; larger values are capped.")] int? take = null)
    {
        var paging = await pageSizeResolver.GetPagingAsync(0, take);

        var criteria = AbstractTypeFactory<StoreSearchCriteria>.TryCreateInstance();
        criteria.Sort = $"{nameof(Store.StoreState)}:desc;{nameof(Store.Name)}";
        var result = await access.SearchReadableStoresAsync(context.User, criteria.WithPaging(paging));

        var stores = new List<object>();

        foreach (var store in result.Results)
        {
            var findings = await setupService.CheckAsync(store);
            var blocking = findings.CountBySeverity(McpFindingSeverities.Critical);

            stores.Add(new
            {
                store_id = store.Id,
                name = store.Name,
                state = StoreStateNames.ToName(store.StoreState),
                ready_to_open = blocking == 0,
                blocking,
                warnings = findings.CountBySeverity(McpFindingSeverities.Warning),
                top_finding = findings.OrderBySeverity().FirstOrDefault()?.Title,
            });
        }

        return new
        {
            stores_by_state = new
            {
                open = await CountAsync(context.User, StoreState.Open),
                closed = await CountAsync(context.User, StoreState.Closed),
                restricted_access = await CountAsync(context.User, StoreState.RestrictedAccess),
            },
            total_count = result.TotalCount,
            stores,
            has_more = result.TotalCount > stores.Count,
            next_step = "get_store_readiness_report for what a store is missing and how to fix it; get_store for its whole setup; " +
                "list_stores to page through every store.",
        };
    }

    [McpServerTool(Name = "list_stores", ReadOnly = true, Destructive = false, Idempotent = true)]
    [Description("List the stores the user manages, with their state, languages, currencies, catalog and storefront URL. " +
        "Filter by a part of the name or id, or by state. Returns one page: when has_more is true, call again with next_skip.")]
    public async Task<object> ListStores(
        RequestContext<CallToolRequestParams> context,
        [Description("A part of the store's name or id.")] string keyword = null,
        [Description("open, closed or restricted_access.")] string state = null,
        [Description("Number of stores to skip.")] int? skip = null,
        [Description("Number of stores to return. Omit it for the default page size; larger values are capped.")] int? take = null)
    {
        var paging = await pageSizeResolver.GetPagingAsync(skip, take);

        var criteria = AbstractTypeFactory<StoreSearchCriteria>.TryCreateInstance();
        criteria.Keyword = keyword;

        if (!string.IsNullOrWhiteSpace(state))
        {
            criteria.StoreStates = [StoreStateNames.Parse(state)];
        }

        var result = await access.SearchReadableStoresAsync(context.User, criteria.WithPaging(paging));

        return result.ToMcpPage(paging, store => new
        {
            store_id = store.Id,
            name = store.Name,
            state = StoreStateNames.ToName(store.StoreState),
            default_language = store.DefaultLanguage,
            languages = store.Languages,
            default_currency = store.DefaultCurrency,
            currencies = store.Currencies,
            catalog_id = store.Catalog,
            url = store.SecureUrl.IfNullOrEmpty(store.Url),
        });
    }

    [McpServerTool(Name = "get_store", ReadOnly = true, Destructive = false, Idempotent = true)]
    [Description("One store with its whole setup: the basics (name, languages, currencies, addresses, the catalog and " +
        "fulfillment center links), the current state of every installed module's part (such as payment and shipping " +
        "methods and the tax provider), the settings changed from their defaults, and a readiness summary. Secrets are " +
        "never shown. Use it before changing a store's setup.")]
    public async Task<object> GetStore(
        RequestContext<CallToolRequestParams> context,
        [Description("Store id, as list_stores returns it.")] string store_id)
    {
        var store = await access.GetReadableStoreAsync(context.User, store_id);

        var parts = new Dictionary<string, object>();

        foreach (var part in setupService.GetParts().OfType<IStoreSetupState>())
        {
            parts[part.Name] = await GetPartStateAsync(context.User, part, store);
        }

        var findings = await setupService.CheckAsync(store);
        var blocking = findings.CountBySeverity(McpFindingSeverities.Critical);

        return new
        {
            store_id = store.Id,
            name = store.Name,
            state = StoreStateNames.ToName(store.StoreState),
            parts,
            // The settings part's state lists exactly the settings changed from their defaults.
            changed_settings = parts.Values.OfType<StoreSettingsSection>().FirstOrDefault()?.Values.Count,
            readiness = new
            {
                ready_to_open = blocking == 0,
                blocking,
                warnings = findings.CountBySeverity(McpFindingSeverities.Warning),
                top_findings = findings.OrderBySeverity().Take(TopFindingCount).Select(x => x.Title).ToList(),
            },
            next_step = "get_store_readiness_report for every finding with its fix; get_store_settings for every setting.",
        };
    }

    // A part's state needs the part's own read permission for the store. A part that fails is reported without its error,
    // which may hold a third party's trace.
    private async Task<object> GetPartStateAsync(ClaimsPrincipal user, IStoreSetupState part, Store store)
    {
        if (!StoreAccess.CanRead(user, part, store.Id))
        {
            return $"Not shown: needs {part.Permissions?.Read}.";
        }

        try
        {
            return await part.GetStateAsync(store.CloneTyped());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "The store setup part {Part} could not read the store {StoreId}", part.Name, store.Id);
            return "Not shown: this part could not be read. Review it in the back office.";
        }
    }


    private async Task<int> CountAsync(ClaimsPrincipal user, StoreState state)
    {
        var criteria = AbstractTypeFactory<StoreSearchCriteria>.TryCreateInstance();
        criteria.StoreStates = [state];
        criteria.Take = 0;

        return (await access.SearchReadableStoresAsync(user, criteria)).TotalCount;
    }
}
