using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.Services;

public interface IStoreAuthenticationSchemeSearchService : ISearchService<StoreAuthenticationSchemeSearchCriteria, StoreAuthenticationSchemeSearchResult, StoreAuthenticationScheme>
{
}
