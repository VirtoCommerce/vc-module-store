using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Seo.Core.Models;
using VirtoCommerce.Seo.Core.Services;
using VirtoCommerce.StoreModule.Data.Caching;
using VirtoCommerce.StoreModule.Data.Repositories;

namespace VirtoCommerce.StoreModule.Data.Services
{
    public class StoreSeoResolver(Func<IStoreRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache)
        : ISeoResolver
    {
        #region ISeoResolver members
        public async Task<IList<SeoInfo>> FindSeoAsync(SeoSearchCriteria criteria)
        {
            var slug = criteria.Permalink ?? criteria.Slug;
            var cacheKey = CacheKey.With(GetType(), nameof(FindSeoAsync), slug);
            return await platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(StoreSeoInfoCacheRegion.CreateChangeToken());
                var result = new List<SeoInfo>();
                using (var repository = repositoryFactory())
                {
                    // Find seo entries for specified keyword. Also add other seo entries related to found object.
                    result = (await repository.SeoInfos.Where(x => x.Keyword == slug)
                                                               .Join(repository.SeoInfos, x => x.StoreId, y => y.StoreId, (x, y) => y)
                                                               .ToArrayAsync()).Select(x => x.ToModel(AbstractTypeFactory<SeoInfo>.TryCreateInstance())).ToList();
                }
                return result.ToArray();
            });
        }
        #endregion
    }
}
