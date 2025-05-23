using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Data.Caching;
using VirtoCommerce.StoreModule.Data.Repositories;

namespace VirtoCommerce.StoreModule.Data.Services
{
    [Obsolete("Use VirtoCommerce.Seo.Core.Services.ISeoResolver", DiagnosticId = "VC0010", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
    public class StoreSeoBySlugResolver : ISeoBySlugResolver
    {
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly Func<IStoreRepository> _repositoryFactory;
        public StoreSeoBySlugResolver(Func<IStoreRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache)
        {
            _repositoryFactory = repositoryFactory;
            _platformMemoryCache = platformMemoryCache;
        }
        #region ISeoBySlugResolver members
        public async Task<SeoInfo[]> FindSeoBySlugAsync(string slug)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(FindSeoBySlugAsync), slug);
            return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(StoreSeoInfoCacheRegion.CreateChangeToken());
                var result = new List<SeoInfo>();
                using (var repository = _repositoryFactory())
                {
                    // Find seo entries for specified keyword. Also add other seo entries related to found object.
                    result = (await repository.SeoInfos.Where(x => x.Keyword == slug)
                                                               .Join(repository.SeoInfos, x => x.StoreId, y => y.StoreId, (x, y) => y)
                                                               .ToArrayAsync()).Select(x =>
                    {
                        // this is obsolete code, but we need to keep it for backward compatibility
                        // the old ToModel method cannot be used
                        var result = AbstractTypeFactory<SeoInfo>.TryCreateInstance();

                        result.Id = x.Id;
                        result.CreatedBy = x.CreatedBy;
                        result.CreatedDate = x.CreatedDate;
                        result.ModifiedBy = x.ModifiedBy;
                        result.ModifiedDate = x.ModifiedDate;
                        result.LanguageCode = x.Language;
                        result.SemanticUrl = x.Keyword;
                        result.PageTitle = x.Title;
                        result.ImageAltDescription = x.ImageAltDescription;
                        result.IsActive = x.IsActive;
                        result.MetaDescription = x.MetaDescription;
                        result.MetaKeywords = x.MetaKeywords;
                        result.ObjectId = x.StoreId;
                        result.ObjectType = "Store";
                        result.StoreId = x.StoreId;

                        return result;
                    }).ToList();
                }
                return result.ToArray();
            });
        }
        #endregion
    }
}
