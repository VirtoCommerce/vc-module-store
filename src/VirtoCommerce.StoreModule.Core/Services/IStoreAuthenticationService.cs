using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.Services;

public interface IStoreAuthenticationService
{
    Task SaveStoreSchemesAsync(string storeId, IList<StoreAuthenticationScheme> models);
    Task<IList<StoreAuthenticationScheme>> GetStoreSchemesAsync(string storeId, bool clone = true);
}
