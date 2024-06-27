using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.Services;

public interface IPublicStoreSettings
{
    Task<IList<ModulePublicStoreSettings>> GetSettings(string storeId);
}
