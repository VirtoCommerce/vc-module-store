using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CoreModule.Core.Currency;

namespace VirtoCommerce.StoreModule.Core.Services
{
    public interface IStoreCurrencyResolver
    {
        Task<Currency> GetStoreCurrencyAsync(string currencyCode, string storeId, string cultureName = null);
        Task<IEnumerable<Currency>> GetAllStoreCurrenciesAsync(string storeId, string cultureName = null);
    }
}
