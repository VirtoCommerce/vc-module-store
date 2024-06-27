using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.StoreModule.Data.Services;

public class PublicStoreSettings : IPublicStoreSettings
{
    private readonly IStoreService _storeService;

    public PublicStoreSettings(IStoreService storeService)
    {
        _storeService = storeService;
    }

    public virtual async Task<IList<ModulePublicStoreSettings>> GetSettings(string storeId)
    {
        ArgumentNullException.ThrowIfNull(storeId);

        var store = await _storeService.GetNoCloneAsync(storeId, StoreResponseGroup.Full.ToString());

        if (store == null)
        {
            return [];
        }

        var result = new List<ModulePublicStoreSettings>();

        foreach (var settingByModule in store.Settings.Where(s => s.IsPublic).GroupBy(s => s.ModuleId))
        {
            var moduleSettings = ToModulePublicStoreSettings(settingByModule);

            if (moduleSettings.Settings.Length > 0)
            {
                result.Add(moduleSettings);
            }
        }

        return result;
    }

    protected virtual ModulePublicStoreSettings ToModulePublicStoreSettings(IGrouping<string, ObjectSettingEntry> settingByModule)
    {
        return new ModulePublicStoreSettings
        {
            ModuleId = settingByModule.Key,
            Settings = settingByModule.Select(s => new PublicStoreSetting
            {
                Name = s.Name,
                Value = ToSettingValue(s)
            }).ToArray(),
        };
    }

    protected virtual object ToSettingValue(ObjectSettingEntry s)
    {
        var result = s.Value ?? s.DefaultValue;

        if (result == null)
        {
            switch (s.ValueType)
            {
                case SettingValueType.Boolean:
                    result = false;
                    break;
            }
        }

        return result;
    }
}
