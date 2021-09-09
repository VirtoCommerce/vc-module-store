using System;
using System.Threading.Tasks;
using VirtoCommerce.StoreModule.Core.Model.Search;

namespace VirtoCommerce.StoreModule.Core.Services
{
    public interface IStoreSearchService
    {
        [Obsolete(@"Need to remove after inheriting StoreSearchService from SearchService.")]
        Task<StoreSearchResult> SearchStoresAsync(StoreSearchCriteria criteria);
    }
}
