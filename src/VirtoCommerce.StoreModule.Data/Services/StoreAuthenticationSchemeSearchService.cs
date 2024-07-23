using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.Data.Model;
using VirtoCommerce.StoreModule.Data.Repositories;

namespace VirtoCommerce.StoreModule.Data.Services;

public class StoreAuthenticationSchemeSearchService : SearchService<StoreAuthenticationSchemeSearchCriteria, StoreAuthenticationSchemeSearchResult, StoreAuthenticationScheme, StoreAuthenticationSchemeEntity>, IStoreAuthenticationSchemeSearchService
{
    public StoreAuthenticationSchemeSearchService(
        Func<IStoreRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IStoreAuthenticationSchemeService crudCrudService,
        IOptions<CrudOptions> crudOptions)
        : base(repositoryFactory, platformMemoryCache, crudCrudService, crudOptions)
    {
    }

    protected override IQueryable<StoreAuthenticationSchemeEntity> BuildQuery(IRepository repository, StoreAuthenticationSchemeSearchCriteria criteria)
    {
        var query = ((IStoreRepository)repository).StoreAuthenticationSchemes;

        if (!string.IsNullOrEmpty(criteria.StoreId))
        {
            query = query.Where(x => x.StoreId == criteria.StoreId);
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(StoreAuthenticationSchemeSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
            [
                new SortInfo { SortColumn = nameof(StoreAuthenticationSchemeEntity.CreatedDate), SortDirection = SortDirection.Descending },
                new SortInfo { SortColumn = nameof(StoreAuthenticationSchemeEntity.Id) },
            ];
        }

        return sortInfos;
    }
}
