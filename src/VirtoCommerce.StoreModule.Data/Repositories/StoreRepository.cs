using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Data.Model;

namespace VirtoCommerce.StoreModule.Data.Repositories
{
    public class StoreRepository : DbContextRepositoryBase<StoreDbContext>, IStoreRepository
    {
        public StoreRepository(StoreDbContext dbContext) : base(dbContext)
        {
        }

        public virtual async Task<IEnumerable<StoreEntity>> GetByIdsAsync(IEnumerable<string> ids, string responseGroup = null)
        {
            var storeResponseGroup = EnumUtility.SafeParseFlags(responseGroup, StoreResponseGroup.Full);

            var retVal = await Stores.Where(x => ids.Contains(x.Id))
                               .Include(x => x.Languages)
                               .Include(x => x.Currencies)
                               .Include(x => x.TrustedGroups)
                               .ToArrayAsync();

            if (storeResponseGroup.HasFlag(StoreResponseGroup.StoreFulfillmentCenters))
            {
                await StoreFulfillmentCenters.Where(x => ids.Contains(x.StoreId)).LoadAsync();
            }

            if (storeResponseGroup.HasFlag(StoreResponseGroup.StoreSeoInfos))
            {
                await SeoInfos.Where(x => ids.Contains(x.StoreId)).LoadAsync();
            }

            if (storeResponseGroup.HasFlag(StoreResponseGroup.DynamicProperties))
            {
                await DynamicPropertyObjectValues.Where(x => ids.Contains(x.ObjectId)).LoadAsync();
            }

            return retVal;
        }

        public IQueryable<StoreEntity> Stores => DbContext.Set<StoreEntity>();
        public IQueryable<StoreFulfillmentCenterEntity> StoreFulfillmentCenters => DbContext.Set<StoreFulfillmentCenterEntity>();

        public IQueryable<SeoInfoEntity> SeoInfos => DbContext.Set<SeoInfoEntity>();
        public IQueryable<StoreDynamicPropertyObjectValueEntity> DynamicPropertyObjectValues => DbContext.Set<StoreDynamicPropertyObjectValueEntity>();

    }
}
