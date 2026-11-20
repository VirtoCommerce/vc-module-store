using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// The rules every part follows for an object that has settings, built on what a setting means (IsSecret, Describe,
// TryConvert). StoreSetupSettings derives it, and so do the test doubles of the parts' tests, which then run these rules.
public abstract class StoreSetupSettingsBase : IStoreSetupSettings
{
    public abstract bool IsSecret(SettingDescriptor descriptor);

    public abstract McpSetupSetting Describe(SettingDescriptor descriptor, object value);

    public abstract bool TryConvert(SettingDescriptor descriptor, object raw, out object value, out string error);

    public abstract Task<IList<McpSetupSetting>> GetStoreSettingsAsync(Store store);

    public virtual IList<McpSetupSetting> DescribeAll(IEnumerable<SettingDescriptor> descriptors)
    {
        return (descriptors ?? [])
            .Where(x => !IsSecret(x))
            .Select(x => Describe(x, value: null))
            .ToList();
    }

    public virtual IDictionary<string, object> GetValues(IHasSettings owner, IEnumerable<SettingDescriptor> descriptors)
    {
        return (descriptors ?? [])
            .Where(x => !IsSecret(x))
            .DistinctBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Name, x => GetValue(owner, x), StringComparer.OrdinalIgnoreCase);
    }

    public virtual object GetValue(IHasSettings owner, SettingDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return owner?.Settings?.FirstOrDefault(x => x.Name.EqualsIgnoreCase(descriptor.Name))?.Value ?? descriptor.DefaultValue;
    }

    public virtual IDictionary<string, object> PlanSettings(McpSetupPartPlan plan, IHasSettings owner, IList<SettingDescriptor> descriptors,
        IDictionary<string, object> values, string fieldPrefix, string ownerTitle, string listedBy)
    {
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

            var entry = owner.Settings.FirstOrDefault(x => x.Name.EqualsIgnoreCase(descriptor.Name));
            var current = entry?.Value ?? descriptor.DefaultValue;

            // A null for a setting that has no value changes nothing, so a state read from get_store can be sent back.
            if (current == null && (raw is null or JsonElement { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined }))
            {
                continue;
            }

            if (!TryConvert(descriptor, raw, out var value, out var error))
            {
                plan.Errors.Add(error);
                continue;
            }

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

    public virtual (IList<string> Names, int SecretCount) GetEmptyRequiredSettings(IHasSettings owner, IEnumerable<SettingDescriptor> descriptors)
    {
        var empty = (descriptors ?? [])
            .Where(x => x.IsRequired && IsEmpty(GetValue(owner, x)))
            .ToList();

        return (empty.Where(x => !IsSecret(x)).Select(x => x.Name).ToList(), empty.Count(IsSecret));
    }

    public virtual void CopySettings(IHasSettings planned, IHasSettings target, ICollection<string> names)
    {
        ArgumentNullException.ThrowIfNull(planned);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(names);

        target.Settings = target.Settings?.ToList() ?? [];

        foreach (var plannedEntry in (planned.Settings ?? []).Where(x => names.Contains(x.Name)))
        {
            var entry = target.Settings.FirstOrDefault(x => x.Name.EqualsIgnoreCase(plannedEntry.Name));

            if (entry == null)
            {
                target.Settings.Add(plannedEntry);
            }
            else
            {
                entry.Value = plannedEntry.Value;
            }
        }
    }

    // A required setting without a value: none, or a text of spaces only.
    protected virtual bool IsEmpty(object value)
    {
        return value == null || value is string text && string.IsNullOrWhiteSpace(text);
    }
}
