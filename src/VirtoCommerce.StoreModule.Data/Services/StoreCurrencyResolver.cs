using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.CoreModule.Data.Currency;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.StoreModule.Data.Services
{
    public class StoreCurrencyResolver : IStoreCurrencyResolver
    {
        private readonly ICurrencyService _currencyService;
        private readonly IStoreService _storeService;
        private readonly IPlatformMemoryCache _platformMemoryCache;
        public StoreCurrencyResolver(
            ICurrencyService currencyService,
            IStoreService storeService,
            IPlatformMemoryCache platformMemoryCache)
        {
            _currencyService = currencyService;
            _storeService = storeService;
            _platformMemoryCache = platformMemoryCache;
        }

        public virtual async Task<IEnumerable<Currency>> GetAllStoreCurrenciesAsync(string storeId, string cultureName = null)
        {
            if (cultureName == null)
            {
                if (storeId == null)
                {
                    throw new ArgumentNullException(nameof(storeId));
                }

                var store = await _storeService.GetNoCloneAsync(storeId);

                cultureName = store.DefaultLanguage ?? Language.InvariantLanguage.CultureName;
            }

            var cacheKey = CacheKey.With(GetType(), nameof(GetAllStoreCurrenciesAsync), cultureName);
            return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(CurrencyCacheRegion.CreateChangeToken());

                //Clone currencies
                var allCurrencies = (await _currencyService.GetAllCurrenciesAsync()).Select(x => x.Clone()).OfType<Currency>().ToArray();

                //Change culture name for all system currencies to requested
                allCurrencies.Apply(x => x.CultureName = cultureName);

                return allCurrencies;
            });
        }

        public virtual async Task<Currency> GetStoreCurrencyAsync(string currencyCode, string storeId, string cultureName = null)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                var store = await _storeService.GetNoCloneAsync(storeId);

                currencyCode = store.DefaultCurrency;
            }

            var allCurrencies = await GetAllStoreCurrenciesAsync(storeId, cultureName);

            var currency = allCurrencies.FirstOrDefault(x => x.Code.EqualsInvariant(currencyCode));
            if (currency == null)
            {
                throw new InvalidOperationException($"requested currency {currencyCode} is not registered in the system");
            }

            return currency;
        }
    }
}
