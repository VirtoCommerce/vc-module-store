using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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
// from StoreSetupSettingsBase.
public class StoreSetupSettings(
    ISettingsRegistrar settingsRegistrar,
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

        // A null returns the setting to its default, which is saved itself: the platform's settings save skips a null for
        // a setting without allowed values. So such a setting without a default has nothing to return to: saving its null
        // would change nothing.
        if (raw is null or JsonElement { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined })
        {
            if (descriptor.DefaultValue == null && descriptor.AllowedValues.IsNullOrEmpty())
            {
                error = $"Setting '{descriptor.Name}' has no default to return to. Send a value; an empty text clears a text setting.";
                return false;
            }

            value = descriptor.DefaultValue;
            return true;
        }

        if (!TryConvertValue(descriptor.ValueType, raw, out var converted))
        {
            error = $"Setting '{descriptor.Name}' takes {GetTypeText(descriptor.ValueType)}.";
            return false;
        }

        // A longer text would be refused only when it is saved, after the approval.
        if (descriptor.ValueType == SettingValueType.ShortText && converted is string { Length: > MaxShortTextLength })
        {
            error = $"Setting '{descriptor.Name}' takes at most {MaxShortTextLength} characters.";
            return false;
        }

        if (!descriptor.AllowedValues.IsNullOrEmpty())
        {
            // The allowed value itself is kept, with its spelling: modules compare these values as they declared them.
            converted = descriptor.AllowedValues.FirstOrDefault(x => SetupValues.AreEqual(x, converted, StringComparison.OrdinalIgnoreCase));

            if (converted == null)
            {
                error = $"Setting '{descriptor.Name}' takes one of: {string.Join(", ", descriptor.AllowedValues)}.";
                return false;
            }
        }

        // The platform saves an empty text, or one of spaces only, as none, and a setting saved as none shows its default:
        // an empty text never replaces a default text, and every later setup would plan the same change again. Without a
        // default text, an empty text clears the setting, and a text of spaces only is planned as the empty text it is
        // saved as, so a later setup finds nothing to change.
        if (converted is string text && string.IsNullOrWhiteSpace(text))
        {
            if (SetupValues.ToInvariantString(descriptor.DefaultValue) is { Length: > 0 } defaultText)
            {
                error = $"Setting '{descriptor.Name}' cannot be empty: the platform keeps no empty text, so it would show its default '{defaultText}'. " +
                    "Send null to return it to its default, or another text.";
                return false;
            }

            converted = string.Empty;
        }

        value = converted;
        return true;
    }

    public override Task<IList<McpSetupSetting>> GetStoreSettingsAsync(Store store)
    {
        ArgumentNullException.ThrowIfNull(store);

        var translation = settingTexts.GetTranslation();
        var values = (store.Settings ?? [])
            .Where(x => !string.IsNullOrEmpty(x.Name))
            .DistinctBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);

        IList<McpSetupSetting> settings = (settingsRegistrar.GetSettingsForType(nameof(Store)) ?? [])
            .Where(x => !x.IsHidden && !IsSecret(x))
            .DistinctBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => Describe(x, values.GetValueOrDefault(x.Name), translation))
            .OrderBy(x => x.ModuleId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult(settings);
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
