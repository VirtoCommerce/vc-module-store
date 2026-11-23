using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.MCP.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// The rules every part follows for an object that has settings, built on what a setting means (IsSecret, Describe,
// TryConvert). StoreSetupSettings derives it, and so do the test doubles of the parts' tests, which then run these rules.
// setupValues compares a value from a request with a stored one or a default (IMcpSetupValues).
public abstract class StoreSetupSettingsBase(IMcpSetupValues setupValues) : IStoreSetupSettings
{
    protected IMcpSetupValues SetupValues { get; } = setupValues;

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
            .Where(x => !IsSecret(x) && !IsForced(owner, x))
            .DistinctBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Name, x => GetValue(owner, x), StringComparer.OrdinalIgnoreCase);
    }

    public virtual object GetValue(IHasSettings owner, SettingDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        // The entry's default is a configured default (VirtoCommerce:Settings:Override:DefaultValue) when there is one: the
        // settings manager sets it when it reads the object's settings.
        var entry = FindEntry(owner, descriptor);

        return entry?.Value ?? entry?.DefaultValue ?? descriptor.DefaultValue;
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
            var refusal = GetRefusal(owner, descriptor, name, ownerTitle, listedBy);

            if (refusal != null)
            {
                plan.Errors.Add(refusal);
                continue;
            }

            var entry = FindEntry(owner, descriptor);
            var current = GetValue(owner, descriptor);

            // A null for a setting that has no value changes nothing, so a state read from get_store can be sent back.
            if (current == null && (raw is null or JsonElement { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined }))
            {
                continue;
            }

            // Converted with the object's default, so a null returns the setting to a configured default.
            if (!TryConvert(GetDescriptorWithDefault(owner, descriptor), raw, out var value, out var error))
            {
                plan.Errors.Add(error);
                continue;
            }

            if (SetupValues.AreEqual(current, value))
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

        foreach (var plannedEntry in (planned.Settings ?? []).Where(x => names.ContainsIgnoreCase(x.Name)))
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

    // A setting the platform's configuration sets for the object: a value forced by VirtoCommerce:Settings:Override:
    // CurrentValue, or a fixed setting. The settings manager reads its entry as read-only, and its save keeps the
    // configured value whatever is sent, so the tools neither show nor change the setting.
    protected virtual bool IsForced(IHasSettings owner, SettingDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return FindEntry(owner, descriptor)?.IsReadOnly == true;
    }

    // Why a setting named in a settings map is refused before its value is read: the object does not take it, or the
    // platform's configuration sets it (IsForced). Null when it is neither.
    protected virtual string GetRefusal(IHasSettings owner, SettingDescriptor descriptor, string name, string ownerTitle, string listedBy)
    {
        if (descriptor == null)
        {
            return $"{ownerTitle} has no setting '{name}'. {listedBy} lists the settings.";
        }

        return IsForced(owner, descriptor)
            ? $"Setting '{descriptor.Name}' is set by the platform's configuration, so the tools do not change it."
            : null;
    }

    // The setting with the object's default, for the rules that read a default, such as a null that returns the setting to
    // it: a configured default (VirtoCommerce:Settings:Override:DefaultValue) is the entry's default, which the settings
    // manager sets when it reads the object's settings.
    protected virtual SettingDescriptor GetDescriptorWithDefault(IHasSettings owner, SettingDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        var defaultValue = FindEntry(owner, descriptor)?.DefaultValue;

        if (defaultValue == null || Equals(defaultValue, descriptor.DefaultValue))
        {
            return descriptor;
        }

        var withDefault = descriptor.CloneTyped();
        withDefault.DefaultValue = defaultValue;

        return withDefault;
    }

    private static ObjectSettingEntry FindEntry(IHasSettings owner, SettingDescriptor descriptor)
    {
        return owner?.Settings?.FirstOrDefault(x => x.Name.EqualsIgnoreCase(descriptor.Name));
    }
}
