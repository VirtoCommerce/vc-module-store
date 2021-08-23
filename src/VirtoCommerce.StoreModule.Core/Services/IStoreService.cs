using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Security;


namespace VirtoCommerce.StoreModule.Core.Services
{
    public interface IStoreService
    {
        /// <summary>
        /// Returns list of store ids which passed user can sign in to
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetUserAllowedStoreIdsAsync(ApplicationUser user);
    }
}
