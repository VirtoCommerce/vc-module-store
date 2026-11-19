using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// The settings rules every part follows for an object that has settings: a payment or shipping method, a tax provider,
// or the store itself. Secrets are never described, shown or accepted, so an agent never proposes to send one.
public static class StoreSetupSettingsExtensions
{
    // The settings an object takes, described for a choice, without secrets.
    public static IList<McpSetupSetting> DescribeAll(this IStoreSetupSettings setupSettings, IEnumerable<SettingDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(setupSettings);

        return (descriptors ?? [])
            .Where(x => !setupSettings.IsSecret(x))
            .Select(x => setupSettings.Describe(x, value: null))
            .ToList();
    }

    // The object's values in effect, for a part's state, without secrets.
    public static IDictionary<string, object> GetValues(this IStoreSetupSettings setupSettings, IHasSettings owner, IEnumerable<SettingDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(setupSettings);

        return (descriptors ?? [])
            .Where(x => !setupSettings.IsSecret(x))
            .DistinctBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Name, x => GetValue(owner, x), StringComparer.OrdinalIgnoreCase);
    }

    // The value in effect: the saved one, or the default of a setting never saved.
    public static object GetValue(IHasSettings owner, SettingDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return owner?.Settings?.FirstOrDefault(x => x.Name.EqualsIgnoreCase(descriptor.Name))?.Value ?? descriptor.DefaultValue;
    }

    // Plans a settings map on an object, as every part's section takes one:
    // - a name the object does not take, or a value TryConvert refuses (a secret, among others), is an error;
    // - a null returns the setting to its default: TryConvert turns it into the default value, which is then saved
    //   itself, because the platform's settings save skips a null for most settings; the object keeps today's default;
    // - a value equal to the one in effect changes nothing.
    // The object carries the planned values, and each change is a row of the plan whose field is the prefix and the
    // setting's name. Returns the changed settings with their new values.
    public static IDictionary<string, object> PlanSettings(
        this IStoreSetupSettings setupSettings,
        McpSetupPartPlan plan,
        IHasSettings owner,
        IList<SettingDescriptor> descriptors,
        IDictionary<string, object> values,
        string fieldPrefix,
        string ownerTitle,
        string listedBy)
    {
        ArgumentNullException.ThrowIfNull(setupSettings);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(owner);

        var changed = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        if (values.IsNullOrEmpty())
        {
            return changed;
        }

        // An object the store has never saved starts with an empty list.
        owner.Settings = owner.Settings?.ToList() ?? [];

        foreach (var (name, raw) in values)
        {
            var descriptor = descriptors?.FirstOrDefault(x => x.Name.EqualsIgnoreCase(name));

            if (descriptor == null)
            {
                plan.Errors.Add($"{ownerTitle} has no setting '{name}'. {listedBy} lists the settings.");
                continue;
            }

            if (!setupSettings.TryConvert(descriptor, raw, out var value, out var error))
            {
                plan.Errors.Add(error);
                continue;
            }

            var entry = owner.Settings.FirstOrDefault(x => x.Name.EqualsIgnoreCase(descriptor.Name));
            var current = entry?.Value ?? descriptor.DefaultValue;

            if (McpSetupValues.AreEqual(current, value))
            {
                continue;
            }

            if (entry == null)
            {
                entry = new ObjectSettingEntry(descriptor);
                owner.Settings.Add(entry);
            }

            entry.Value = value;
            plan.AddChange($"{fieldPrefix}{descriptor.Name}", current, value);
            changed[descriptor.Name] = value;
        }

        return changed;
    }
}
