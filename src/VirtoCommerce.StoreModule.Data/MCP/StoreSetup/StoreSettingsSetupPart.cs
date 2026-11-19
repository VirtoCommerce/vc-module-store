using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.StoreModule.Data.MCP.StoreSetup;

// The store-level settings of every installed module. Its state is the settings whose values differ from the defaults.
public class StoreSettingsSetupPart(
    IStoreSetupSettings setupSettings,
    ISettingsRegistrar settingsRegistrar,
    IStoreService storeService)
    : IStoreSetupState, IStoreSetupSection
{
    public const string PartName = "settings";

    public virtual string Name => PartName;

    public virtual string Title => "Store settings";

    public virtual McpSetupPermissions Permissions => StoreSetupPermissions.ForStore();

    public virtual Type SectionType => typeof(StoreSettingsSection);

    public virtual bool IsRequiredForNewStore => false;

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

    // Sets the named settings and leaves the others as they are; null returns a setting to its default. An unknown
    // name, a secret and a setting a setup cannot change are refused.
    public virtual Task<McpSetupPartPlan> PlanAsync(StoreSetupContext context, object section)
    {
        ArgumentNullException.ThrowIfNull(context);

        var plan = AbstractTypeFactory<StoreSettingsSetupPlan>.TryCreateInstance();

        // The planned store carries the new values, so the checks of other parts see them.
        var changed = setupSettings.PlanSettings(plan, context.Store, GetDescriptors(), ((StoreSettingsSection)section).Values,
            fieldPrefix: string.Empty, ownerTitle: "The store", listedBy: "get_store_settings");

        foreach (var (name, value) in changed)
        {
            plan.Values[name] = value;
        }

        return Task.FromResult<McpSetupPartPlan>(plan);
    }

    // Saves the values with the store, read again from the database, so the cached store is refreshed with them.
    public virtual async Task ApplyAsync(StoreSetupContext context, McpSetupPartPlan plan)
    {
        ArgumentNullException.ThrowIfNull(context);

        var store = await storeService.GetByIdAsync(context.Store.Id)
            ?? throw new InvalidOperationException($"Store '{context.Store.Id}' was not found.");
        store.Settings = store.Settings?.ToList() ?? [];
        var descriptors = GetDescriptors();

        foreach (var (name, value) in ((StoreSettingsSetupPlan)plan).Values)
        {
            var entry = store.Settings.FirstOrDefault(x => x.Name.EqualsIgnoreCase(name))
                ?? AddEntry(store, descriptors.First(x => x.Name.EqualsIgnoreCase(name)));
            entry.Value = value;
        }

        await storeService.SaveChangesAsync([store]);
    }

    protected virtual IList<SettingDescriptor> GetDescriptors()
    {
        return settingsRegistrar.GetSettingsForType(nameof(Store))?.ToList() ?? [];
    }

    private static ObjectSettingEntry AddEntry(Store store, SettingDescriptor descriptor)
    {
        var entry = new ObjectSettingEntry(descriptor)
        {
            ObjectType = nameof(Store),
            ObjectId = store.Id,
        };
        store.Settings.Add(entry);

        return entry;
    }
}
