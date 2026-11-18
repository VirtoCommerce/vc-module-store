using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    public virtual StoreSetupPermissions Permissions => StoreSetupPermissions.ForStore();

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
    public virtual Task<StoreSetupPartPlan> PlanAsync(StoreSetupContext context, object section)
    {
        ArgumentNullException.ThrowIfNull(context);

        var plan = AbstractTypeFactory<StoreSettingsSetupPlan>.TryCreateInstance();
        plan.Part = PartName;

        var store = context.Store;
        store.Settings = store.Settings?.ToList() ?? [];
        var descriptors = GetDescriptors();

        foreach (var (name, raw) in ((StoreSettingsSection)section).Values ?? new Dictionary<string, object>())
        {
            var descriptor = descriptors.FirstOrDefault(x => x.Name.EqualsIgnoreCase(name));

            if (descriptor == null)
            {
                plan.Errors.Add($"Store setting '{name}' does not exist. get_store_settings lists the names.");
                continue;
            }

            if (!setupSettings.TryConvert(descriptor, raw, out var value, out var error))
            {
                plan.Errors.Add(error);
                continue;
            }

            var entry = store.Settings.FirstOrDefault(x => x.Name.EqualsIgnoreCase(descriptor.Name));
            var current = entry?.Value ?? descriptor.DefaultValue;

            // A null saves the default value itself: the platform's settings save skips a null for every setting without
            // allowed values (SettingsManager keeps only entries that have values), so passing the null on would change
            // nothing. The store then keeps today's default; following a later default would need the stored value deleted.
            var target = value ?? descriptor.DefaultValue;

            if (StoreSetupValues.AreEqual(current, target))
            {
                continue;
            }

            plan.AddChange(descriptor.Name, current, target);
            plan.Values[descriptor.Name] = target;

            // The planned store carries the new value, so the checks of other parts see it.
            entry ??= AddEntry(store, descriptor);
            entry.Value = target;
        }

        return Task.FromResult<StoreSetupPartPlan>(plan);
    }

    // Saves the values with the store, read again from the database, so the cached store is refreshed with them.
    public virtual async Task ApplyAsync(StoreSetupContext context, StoreSetupPartPlan plan)
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
