using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Core.Settings;
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
        public StoreSearchService(Func<IStoreRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache,
            IStoreService storeService)
           : base(repositoryFactory, platformMemoryCache, (ICrudService<Store>)storeService)
        {
        }

        protected override IQueryable<StoreEntity> BuildQuery(IRepository repository, StoreSearchCriteria criteria)
        {
            var query = ((IStoreRepository)repository).Stores;
            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(x => x.Name.Contains(criteria.Keyword) || x.Id.Contains(criteria.Keyword));
            }
            if (!criteria.StoreIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.StoreIds.Contains(x.Id));
            }
            if (!criteria.StoreStates.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.StoreStates.Contains((StoreState)x.StoreState));
            }

            if (!criteria.FulfillmentCenterIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.FulfillmentCenterIds.Contains(x.FulfillmentCenterId) ||
                                         x.FulfillmentCenters.Any(y => criteria.FulfillmentCenterIds.Contains(y.FulfillmentCenterId)));
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
