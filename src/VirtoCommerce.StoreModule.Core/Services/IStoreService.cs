using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.Services
{
    public interface IStoreService
    {
        [Obsolete(@"Need to remove after inheriting IStoreService from ICrudService<Store>.")]
        Task<Store[]> GetByIdsAsync(string[] ids, string responseGroup = null);
        [Obsolete(@"Need to remove after inheriting IStoreService from ICrudService<Store>.")]
        Task<Store> GetByIdAsync(string id, string responseGroup = null);
        [Obsolete(@"Need to remove after inheriting IStoreService from ICrudService<Store>.")]
        Task SaveChangesAsync(Store[] stores);
        [Obsolete(@"Need to remove after inheriting IStoreService from ICrudService<Store>.")]
        Task DeleteAsync(string[] ids);

        /// <summary>
        /// Returns list of store ids which passed user can sign in to
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetUserAllowedStoreIdsAsync(ApplicationUser user);
    }
}
