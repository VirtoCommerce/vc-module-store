using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model.Search;

namespace VirtoCommerce.StoreModule.Core.Services
{
    public interface IStoreSearchService : ISearchService<StoreSearchCriteria, StoreSearchResult, Store>
    {
    }
}
