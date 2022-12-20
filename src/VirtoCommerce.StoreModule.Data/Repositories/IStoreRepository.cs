using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Data.Model;

namespace VirtoCommerce.StoreModule.Data.Repositories
{
    public interface IStoreRepository : IRepository
    {
        IQueryable<StoreEntity> Stores { get; }

        IQueryable<SeoInfoEntity> SeoInfos { get; }
        IQueryable<StoreDynamicPropertyObjectValueEntity> DynamicPropertyObjectValues { get; }
        Task<IEnumerable<StoreEntity>> GetByIdsAsync(IEnumerable<string> ids, string responseGroup = null);

    }
}
