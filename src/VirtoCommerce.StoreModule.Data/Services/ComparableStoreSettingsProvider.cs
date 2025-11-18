using System.Collections.Generic;
using System.Linq;
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
    public async Task<IList<ComparableSettingScope>> GetComparableSettingsAsync()
    {
        var result = new List<ComparableSettingScope>();

        var storeSearchCriteria = AbstractTypeFactory<StoreSearchCriteria>.TryCreateInstance();
        storeSearchCriteria.Take = 100;

        foreach (var store in await storeSearchService.SearchAllNoCloneAsync(storeSearchCriteria))
        {
            var resultScope = AbstractTypeFactory<ComparableSettingScope>.TryCreateInstance();
            resultScope.ScopeName = $"StoreSettings: {store.Id}";
            result.Add(resultScope);

            foreach (var storeSettingGroup in store.Settings.GroupBy(x => x.GroupName))
            {
                var resultGroup = AbstractTypeFactory<ComparableSettingGroup>.TryCreateInstance();
                resultGroup.GroupName = storeSettingGroup.Key;
                resultScope.SettingGroups.Add(resultGroup);

                foreach (var storeSetting in storeSettingGroup)
                {
                    var resultSetting = AbstractTypeFactory<ComparableSetting>.TryCreateInstance();
                    resultSetting.Name = storeSetting.Name;
                    resultSetting.Value = storeSetting.Value;
                    resultSetting.IsSecret = IsSettingSecret(storeSetting);
                    resultGroup.Settings.Add(resultSetting);
                }
            }
        }

        return result;
    }

    protected virtual bool IsSettingSecret(ObjectSettingEntry setting)
    {
        return setting.ValueType == SettingValueType.SecureString;
    }
}
