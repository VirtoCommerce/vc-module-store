using System.Threading.Tasks;

namespace VirtoCommerce.StoreModule.Core.Services;

public interface IStoreAuthenticationTypeProvider
{
    string Name { get; }

    string DisplayName { get; }

    Task<bool> IsActiveAsync(string storeId);

    Task SetIsActiveAsync(string storeId, bool isActive);
}
