using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.EnvironmentsCompare.Core.Models;
using VirtoCommerce.EnvironmentsCompare.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model.Search;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.StoreModule.Data.Services;

public class ComparableStoreSettingsProvider(IStoreSearchService storeSearchService, ILocalizableSettingService localizableSettingService) : IComparableSettingsProvider
{
    protected const int MaxComparableStoresCount = 100;

    public async Task<IList<ComparableSettingScope>> GetComparableSettingsAsync()
    {
        var result = new List<ComparableSettingScope>();

        var storeSearchCriteria = AbstractTypeFactory<StoreSearchCriteria>.TryCreateInstance();
        storeSearchCriteria.Take = MaxComparableStoresCount;

        foreach (var store in await storeSearchService.SearchAllNoCloneAsync(storeSearchCriteria))
        {
            var resultScope = AbstractTypeFactory<ComparableSettingScope>.TryCreateInstance();
            resultScope.ScopeName = $"StoreSettings: {store.Id}";
            result.Add(resultScope);

            await AddSettings(resultScope, store);

            await AddLanguagesAndCurrencies(resultScope, store);
        }

        return result;
    }

    protected virtual async Task AddSettings(ComparableSettingScope resultScope, Store store)
    {
        foreach (var storeSettingGroup in store.Settings.GroupBy(x => x.GroupName))
        {
            var resultGroup = AbstractTypeFactory<ComparableSettingGroup>.TryCreateInstance();
            resultGroup.GroupName = storeSettingGroup.Key;
            resultScope.SettingGroups.Add(resultGroup);

            foreach (var storeSetting in storeSettingGroup)
            {
                var resultSetting = AbstractTypeFactory<ComparableSetting>.TryCreateInstance();
                resultSetting.Name = storeSetting.Name;
                resultSetting.Value = await GetSettingValue(storeSetting, store);
                resultSetting.IsSecret = IsSettingSecret(storeSetting);
                resultGroup.Settings.Add(resultSetting);
            }
        }
    }

    protected virtual async Task<object> GetSettingValue(ObjectSettingEntry setting, Store store)
    {
        if (!setting.IsLocalizable)
        {
            return setting.Value;
        }
        else
        {
            var localizedValuesBuilder = new StringBuilder();

            foreach (var language in store.Languages.OrderBy(x => x))
            {
                localizedValuesBuilder.Append("[").Append(language).Append("]:");
                foreach (var settingValue in await localizableSettingService.GetValuesAsync(setting.Name, language))
                {
                    localizedValuesBuilder.Append(settingValue.Key).Append("=").Append(settingValue.Value).Append(";");
                }
                localizedValuesBuilder.Append(";");
            }

            return localizedValuesBuilder.ToString();
        }
    }

    protected virtual bool IsSettingSecret(ObjectSettingEntry setting)
    {
        return setting.ValueType == SettingValueType.SecureString;
    }

    protected virtual Task AddLanguagesAndCurrencies(ComparableSettingScope resultScope, Store store)
    {
        var languagesAndCurrenciesGroup = AbstractTypeFactory<ComparableSettingGroup>.TryCreateInstance();
        languagesAndCurrenciesGroup.GroupName = "Languages and Currencies";
        resultScope.SettingGroups.Add(languagesAndCurrenciesGroup);

        var languagesSetting = AbstractTypeFactory<ComparableSetting>.TryCreateInstance();
        languagesSetting.Name = nameof(store.Languages);
        languagesSetting.Value = string.Join(";", store.Languages.OrderBy(x => x));
        languagesAndCurrenciesGroup.Settings.Add(languagesSetting);

        var defaultLanguageSetting = AbstractTypeFactory<ComparableSetting>.TryCreateInstance();
        defaultLanguageSetting.Name = nameof(store.DefaultLanguage);
        defaultLanguageSetting.Value = store.DefaultLanguage;
        languagesAndCurrenciesGroup.Settings.Add(defaultLanguageSetting);

        var currenciesSetting = AbstractTypeFactory<ComparableSetting>.TryCreateInstance();
        currenciesSetting.Name = nameof(store.Currencies);
        currenciesSetting.Value = string.Join(";", store.Currencies.OrderBy(x => x));
        languagesAndCurrenciesGroup.Settings.Add(currenciesSetting);

        var defaultCurrencySetting = AbstractTypeFactory<ComparableSetting>.TryCreateInstance();
        defaultCurrencySetting.Name = nameof(store.DefaultCurrency);
        defaultCurrencySetting.Value = store.DefaultCurrency;
        languagesAndCurrenciesGroup.Settings.Add(defaultCurrencySetting);

        return Task.CompletedTask;
    }
}
