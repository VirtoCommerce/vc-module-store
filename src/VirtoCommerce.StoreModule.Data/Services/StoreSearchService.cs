using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model.Search;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.Data.Model;
using VirtoCommerce.StoreModule.Data.Repositories;

namespace VirtoCommerce.StoreModule.Data.Services
{
    public class StoreSearchService : SearchService<StoreSearchCriteria, StoreSearchResult, Store, StoreEntity>, IStoreSearchService
    {
        public StoreSearchService(
            Func<IStoreRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IStoreService storeService,
            IOptions<CrudOptions> crudOptions)
           : base(repositoryFactory, platformMemoryCache, storeService, crudOptions)
        {
        }

        protected override IQueryable<StoreEntity> BuildQuery(IRepository repository, StoreSearchCriteria criteria)
        {
            var query = ((IStoreRepository)repository).Stores;

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(x => x.Name.Contains(criteria.Keyword) || x.Id.Contains(criteria.Keyword));
            }

            if (!criteria.ObjectIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.ObjectIds.Contains(x.Id));
            }

            if (!criteria.StoreStates.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.StoreStates.Contains((StoreState)x.StoreState));
            }

            if (!criteria.FulfillmentCenterIds.IsNullOrEmpty())
            {
                query = query
                    .Where(x =>
                        criteria.FulfillmentCenterIds.Contains(x.FulfillmentCenterId) ||
                        x.FulfillmentCenters.Any(y => criteria.FulfillmentCenterIds.Contains(y.FulfillmentCenterId)));
            }

            if (!criteria.Domain.IsNullOrEmpty())
            {
                var domainFilter = $"://{criteria.Domain}";
                query = query.Where(c => c.Url.Contains(domainFilter));
            }

            return query;
        }

        protected override IList<SortInfo> BuildSortExpression(StoreSearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;

            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[]
                {
                    new SortInfo
                    {
                        SortColumn = nameof(StoreEntity.Name)
                    }
                };
            }

            return sortInfos;
        }
    }
}
