using System.Threading.Tasks;
using VirtoCommerce.EnvironmentsCompare.Core.Models;
using VirtoCommerce.EnvironmentsCompare.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model.Search;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.StoreModule.Data.Services;

public class ComparableStoreSettingsProvider(IStoreSearchService storeSearchService) : IComparableSettingsProvider
{
    public async Task<ComparableSettingProviderResult> GetComparableSettingsAsync()
    {
        var result = AbstractTypeFactory<ComparableSettingProviderResult>.TryCreateInstance();
        result.Scope = "StoreSettings";

        var storeSearchCriteria = AbstractTypeFactory<StoreSearchCriteria>.TryCreateInstance();
        storeSearchCriteria.Take = 100;

        foreach (var store in await storeSearchService.SearchAllNoCloneAsync(storeSearchCriteria))
        {
            var storeGroup = AbstractTypeFactory<ComparableSettingGroup>.TryCreateInstance();
            storeGroup.Name = store.Id;
            result.SettingGroups.Add(storeGroup);

            foreach (var storeSetting in store.Settings)
            {
                var storeGroupSetting = AbstractTypeFactory<ComparableSetting>.TryCreateInstance();
                storeGroupSetting.Name = storeSetting.Name;
                storeGroupSetting.Value = storeSetting.Value;
                storeGroupSetting.IsSecret = IsSettingSecret(storeSetting);
                storeGroup.Settings.Add(storeGroupSetting);
            }
        }

        return result;
    }

    protected virtual bool IsSettingSecret(ObjectSettingEntry setting)
    {
        return setting.ValueType == SettingValueType.SecureString || setting.Name.EqualsIgnoreCase("Shipping.Bopis.GoogleMaps.ApiKey");
    }
}
