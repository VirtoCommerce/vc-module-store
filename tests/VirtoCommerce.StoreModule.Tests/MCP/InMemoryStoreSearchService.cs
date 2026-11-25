using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model.Search;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.StoreModule.Tests.MCP;

// Store search over stores held in memory, with the filters of StoreSearchService. Like it, an empty ObjectIds list
// does not filter at all.
public class InMemoryStoreSearchService(IList<Store> stores) : IStoreSearchService
{
    public Task<StoreSearchResult> SearchAsync(StoreSearchCriteria criteria, bool clone = true)
    {
        var query = stores.AsEnumerable();

        if (!criteria.ObjectIds.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.ObjectIds.Contains(x.Id, StringComparer.OrdinalIgnoreCase));
        }

        if (!criteria.StoreStates.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.StoreStates.Contains(x.StoreState));
        }

        if (!string.IsNullOrEmpty(criteria.Keyword))
        {
            query = query.Where(x => x.Name.Contains(criteria.Keyword, StringComparison.OrdinalIgnoreCase) || x.Id.Contains(criteria.Keyword, StringComparison.OrdinalIgnoreCase));
        }

        query = criteria.Sort?.StartsWith($"{nameof(Store.StoreState)}:desc", StringComparison.OrdinalIgnoreCase) == true
            ? query.OrderByDescending(x => x.StoreState).ThenBy(x => x.Name)
            : query.OrderBy(x => x.Name);

        var found = query.ToList();

        return Task.FromResult(new StoreSearchResult
        {
            TotalCount = found.Count,
            Results = found.Skip(criteria.Skip).Take(criteria.Take).Select(x => x.CloneTyped()).ToList(),
        });
    }
}
