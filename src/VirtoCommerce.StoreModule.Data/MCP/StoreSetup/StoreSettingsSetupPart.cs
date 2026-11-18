using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Data.MCP.StoreSetup;

// The store-level settings of every installed module. Its state is the settings whose values differ from the defaults.
public class StoreSettingsSetupPart(IStoreSetupSettings setupSettings) : IStoreSetupState
{
    public const string PartName = "settings";

    public virtual string Name => PartName;

    public virtual string Title => "Store settings";

    public virtual StoreSetupPermissions Permissions => StoreSetupPermissions.ForStore();

    public virtual async Task<object> GetStateAsync(Store store)
    {
        ArgumentNullException.ThrowIfNull(store);

        var section = AbstractTypeFactory<StoreSettingsSection>.TryCreateInstance();

        foreach (var setting in (await setupSettings.GetStoreSettingsAsync(store)).Where(x => x.IsChanged))
        {
            section.Values[setting.Name] = setting.Value;
        }

        return section;
    }
}
