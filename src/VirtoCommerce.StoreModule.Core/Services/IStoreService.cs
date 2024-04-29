using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.Services
{
    public interface IStoreService : ICrudService<Store>
    {
        /// <summary>
        /// Returns list of store ids which passed user can sign in to
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<IList<string>> GetUserAllowedStoreIdsAsync(ApplicationUser user);

        Task<Store> GetByDomainAsync(string domain, string responseGroup = null, bool clone = true);
    }
}
