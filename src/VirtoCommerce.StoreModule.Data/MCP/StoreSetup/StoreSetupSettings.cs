using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.MCP.Core.Services;
using VirtoCommerce.MCP.Core.Settings;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Data.MCP.StoreSetup;

// What a setting means for a setup, from the platform's settings and the MCP module's rules: secrets (IMcpSecretSettings),
// titles and descriptions (IMcpSettingTexts) and how values compare (IMcpSetupValues). The rules every part follows come
// from StoreSetupSettingsBase. settingsOverrideProvider and fixedSettings read the settings the platform's configuration
// sets.
public class StoreSetupSettings(
    ISettingsRegistrar settingsRegistrar,
    ISettingsOverrideProvider settingsOverrideProvider,
    IOptions<FixedSettings> fixedSettings,
    IMcpSecretSettings secretSettings,
    IMcpSettingTexts settingTexts,
    IMcpSetupValues setupValues)
    : StoreSetupSettingsBase(setupValues)
{
    // The length of the column a short text is saved in.
    public const int MaxShortTextLength = 512;

    public override bool IsSecret(SettingDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return secretSettings.IsSecret(descriptor.Name, descriptor.ValueType);
    }

    public override McpSetupSetting Describe(SettingDescriptor descriptor, object value)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return Describe(descriptor, value, settingTexts.GetTranslation());
    }

    public override bool TryConvert(SettingDescriptor descriptor, object raw, out object value, out string error)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        value = null;
        error = null;

        if (IsSecret(descriptor))
        {
            error = $"Setting '{descriptor.Name}' is a secret. Secrets are entered in the back office only.";
            return false;
        }

        if (!CanChange(descriptor))
        {
            error = $"Setting '{descriptor.Name}' cannot be set by a setup. Change it in the back office.";
            return false;
        }

        if (raw is null or JsonElement { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined })
        {
            return TryGetDefault(descriptor, out value, out error);
        }

        if (!TryConvertValue(descriptor.ValueType, raw, out var converted))
        {
            error = $"Setting '{descriptor.Name}' takes {GetTypeText(descriptor.ValueType)}.";
            return false;
        }

        // The length rule reads the text as sent, and the empty text rule the value to save, after the allowed values.
        error = GetTextLengthError(descriptor, converted);

        if (error != null)
        {
            return false;
        }

        if (!TryMatchAllowedValue(descriptor, converted, out var allowedValue, out error))
        {
            return false;
        }

        return TryConvertEmptyText(descriptor, allowedValue, out value, out error);
    }

    public override Task<IList<McpSetupSetting>> GetStoreSettingsAsync(Store store)
    {
        ArgumentNullException.ThrowIfNull(store);

        var translation = settingTexts.GetTranslation();

        // A setting the platform's configuration sets for the store is left out (IsForced). The others show the store's
        // value and default in effect, a configured default included (GetDescriptorWithDefault).
        IList<McpSetupSetting> settings = (settingsRegistrar.GetSettingsForType(nameof(Store)) ?? [])
            .Where(x => !x.IsHidden && !IsSecret(x) && !IsForced(store, x))
            .DistinctBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => Describe(GetDescriptorWithDefault(store, x), GetValue(store, x), translation))
            .OrderBy(x => x.ModuleId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult(settings);
    }

    // A choice describes a setting without an object, so of the settings the platform's configuration sets, only those it
    // sets for every object are known here, and they are left out. A setting the configuration sets for one object only
    // is refused when a plan for that object names it.
    public override IList<McpSetupSetting> DescribeAll(IEnumerable<SettingDescriptor> descriptors)
    {
        return base.DescribeAll(descriptors?.Where(x => !IsForced(owner: null, x)));
    }

    protected virtual McpSetupSetting Describe(SettingDescriptor descriptor, object value, JObject translation)
    {
        var isSecret = IsSecret(descriptor);
        var (title, description) = settingTexts.Find(translation, descriptor.Name, descriptor.DisplayName);

        var setting = AbstractTypeFactory<McpSetupSetting>.TryCreateInstance();
        setting.Name = descriptor.Name;
        setting.Title = title;
        setting.Description = description;
        setting.ModuleId = descriptor.ModuleId;
        setting.ValueType = descriptor.ValueType.ToString();
        setting.IsRequired = descriptor.IsRequired;
        setting.IsChangeable = !isSecret && CanChange(descriptor);

        // A secret is described without its values, whoever asks.
        if (!isSecret)
        {
            setting.DefaultValue = descriptor.DefaultValue;
            setting.Value = value ?? descriptor.DefaultValue;
            setting.AllowedValues = descriptor.AllowedValues?.ToList();
            setting.IsChanged = !SetupValues.AreEqual(setting.Value, descriptor.DefaultValue);
        }

        return setting;
    }

    // Hidden, dictionary and localizable settings keep values a setup does not describe; the back office changes them.
    protected virtual bool CanChange(SettingDescriptor descriptor)
    {
        return !descriptor.IsHidden && !descriptor.IsDictionary && !descriptor.IsLocalizable;
    }

    // Also a setting the configuration sets for an object whose settings were not read, such as a store a setup creates,
    // and, without an object, one it sets for every object: a fixed setting (the PlatformSettings section), which the
    // settings manager serves read-only and never saves, or a value forced by VirtoCommerce:Settings:Override:CurrentValue,
    // asked by the object's type and id as the settings manager and its save ask it (Global, without an object).
    protected override bool IsForced(IHasSettings owner, SettingDescriptor descriptor)
    {
        return base.IsForced(owner, descriptor) ||
            fixedSettings.Value.Settings?.Any(x => x.Name.EqualsIgnoreCase(descriptor.Name)) == true ||
            settingsOverrideProvider.TryGetCurrentValue(descriptor, owner?.TypeName, owner?.Id, out _);
    }

    // A null returns the setting to its default, which is saved itself: the platform's settings save skips a null for a
    // setting without allowed values. So such a setting without a default has nothing to return to: saving its null would
    // change nothing.
    protected virtual bool TryGetDefault(SettingDescriptor descriptor, out object value, out string error)
    {
        value = null;
        error = null;

        if (descriptor.DefaultValue == null && descriptor.AllowedValues.IsNullOrEmpty())
        {
            error = $"Setting '{descriptor.Name}' has no default to return to. Send a value; an empty text clears a text setting.";
            return false;
        }

        value = descriptor.DefaultValue;
        return true;
    }

    protected virtual bool TryConvertValue(SettingValueType valueType, object raw, out object value)
    {
        value = null;
        var text = SetupValues.ToInvariantString(raw);

        switch (valueType)
        {
            case SettingValueType.Boolean when bool.TryParse(text, out var boolean):
                value = boolean;
                return true;
            case SettingValueType.Integer when int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integer):
                value = integer;
                return true;
            case SettingValueType.PositiveInteger when int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var positive) && positive > 0:
                value = positive;
                return true;
            case SettingValueType.Decimal when decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var number):
                value = number;
                return true;
            case SettingValueType.DateTime when DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var date):
                value = date;
                return true;
            case SettingValueType.ShortText or SettingValueType.LongText or SettingValueType.Json or SettingValueType.Cron:
                value = text;
                return true;
            default:
                return false;
        }
    }

    // A short text longer than its column would be refused only when it is saved, after the approval.
    protected virtual string GetTextLengthError(SettingDescriptor descriptor, object value)
    {
        if (descriptor.ValueType == SettingValueType.ShortText && value is string { Length: > MaxShortTextLength })
        {
            return $"Setting '{descriptor.Name}' takes at most {MaxShortTextLength} characters.";
        }

        return null;
    }

    // The allowed value itself is kept, with its spelling: modules compare these values as they declared them.
    protected virtual bool TryMatchAllowedValue(SettingDescriptor descriptor, object value, out object allowedValue, out string error)
    {
        allowedValue = value;
        error = null;

        if (descriptor.AllowedValues.IsNullOrEmpty())
        {
            return true;
        }

        allowedValue = descriptor.AllowedValues.FirstOrDefault(x => SetupValues.AreEqual(x, value, StringComparison.OrdinalIgnoreCase));

        if (allowedValue == null)
        {
            error = $"Setting '{descriptor.Name}' takes one of: {string.Join(", ", descriptor.AllowedValues)}.";
            return false;
        }

        return true;
    }

    // The platform saves an empty text, or one of spaces only, as none, and a setting saved as none shows its default: an
    // empty text never replaces a default text, and every later setup would plan the same change again. Without a default
    // text, an empty text clears the setting, and a text of spaces only is planned as the empty text it is saved as, so a
    // later setup finds nothing to change.
    protected virtual bool TryConvertEmptyText(SettingDescriptor descriptor, object value, out object converted, out string error)
    {
        converted = null;
        error = null;

        if (value is string text && string.IsNullOrWhiteSpace(text))
        {
            if (SetupValues.ToInvariantString(descriptor.DefaultValue) is { Length: > 0 } defaultText)
            {
                error = $"Setting '{descriptor.Name}' cannot be empty: the platform keeps no empty text, so it would show its default '{defaultText}'. " +
                    "Send null to return it to its default, or another text.";
                return false;
            }

            converted = string.Empty;
            return true;
        }

        converted = value;
        return true;
    }

    // What a refusal says a setting of the type takes.
    protected virtual string GetTypeText(SettingValueType valueType)
    {
        return valueType switch
        {
            SettingValueType.Boolean => "true or false",
            SettingValueType.Integer => "a whole number",
            SettingValueType.PositiveInteger => "a whole number above 0",
            SettingValueType.Decimal => "a number",
            SettingValueType.DateTime => "a date and time, such as 2026-01-31T12:00:00Z",
            _ => "a text",
        };
    }
}
