using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Localizations;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Data.MCP.StoreSetup;

public partial class StoreSetupSettings(
    ISettingsRegistrar settingsRegistrar,
    ITranslationService translationService,
    IOptions<StoreSetupSecretOptions> secretOptions)
    : IStoreSetupSettings
{
    // Titles and descriptions come from the English localization files of the modules, as in the back office.
    private const string Language = "en";

    // Compared with the whole words of a setting name: "ApiKey" has the word Key, "Keyword" does not.
    private static readonly HashSet<string> _secretWords = new(StringComparer.OrdinalIgnoreCase) { "Key", "Secret", "Password", "Token", "Credential", "Credentials" };

    public virtual bool IsSecret(SettingDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return descriptor.ValueType == SettingValueType.SecureString
            || secretOptions.Value.SettingNames.Contains(descriptor.Name)
            || GetNameWords(descriptor.Name).Any(_secretWords.Contains);
    }

    public virtual StoreSetupSetting Describe(SettingDescriptor descriptor, object value)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return Describe(descriptor, value, translationService.GetTranslationDataForLanguage(Language));
    }

    public virtual bool TryConvert(SettingDescriptor descriptor, object raw, out object value, out string error)
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
        // most settings.
        if (raw is null or JsonElement { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined })
        {
            value = descriptor.DefaultValue;
            return true;
        }

        if (!TryConvertValue(descriptor.ValueType, raw, out var converted))
        {
            error = $"Setting '{descriptor.Name}' takes {GetTypeText(descriptor.ValueType)}.";
            return false;
        }

        if (!descriptor.AllowedValues.IsNullOrEmpty())
        {
            // The allowed value itself is kept, with its spelling: modules compare these values as they declared them.
            converted = descriptor.AllowedValues.FirstOrDefault(x => StoreSetupValues.AreEqual(x, converted));

            if (converted == null)
            {
                error = $"Setting '{descriptor.Name}' takes one of: {string.Join(", ", descriptor.AllowedValues)}.";
                return false;
            }
        }

        value = converted;
        return true;
    }

    public virtual Task<IList<StoreSetupSetting>> GetStoreSettingsAsync(Store store)
    {
        ArgumentNullException.ThrowIfNull(store);

        var translation = translationService.GetTranslationDataForLanguage(Language);
        var values = (store.Settings ?? [])
            .Where(x => !string.IsNullOrEmpty(x.Name))
            .DistinctBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);

        IList<StoreSetupSetting> settings = (settingsRegistrar.GetSettingsForType(nameof(Store)) ?? [])
            .Where(x => !x.IsHidden && !IsSecret(x))
            .DistinctBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => Describe(x, values.GetValueOrDefault(x.Name), translation))
            .OrderBy(x => x.ModuleId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult(settings);
    }

    protected virtual StoreSetupSetting Describe(SettingDescriptor descriptor, object value, JObject translation)
    {
        var isSecret = IsSecret(descriptor);
        var texts = FindNode(translation?["settings"], descriptor.Name.Split('.'), 0);

        var setting = AbstractTypeFactory<StoreSetupSetting>.TryCreateInstance();
        setting.Name = descriptor.Name;
        setting.Title = GetText(texts, "title") ?? descriptor.DisplayName ?? descriptor.Name;
        setting.Description = GetText(texts, "description");
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
            setting.IsChanged = !StoreSetupValues.AreEqual(setting.Value, descriptor.DefaultValue);
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
        var text = StoreSetupValues.ToInvariantString(raw);

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

    private static string GetTypeText(SettingValueType valueType)
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

    private static string GetText(JToken texts, string key)
    {
        var text = texts?[key]?.ToString();

        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    // The localization files nest a setting name by its dots ("Stores" > "TaxCalculationEnabled"), but a key may also
    // hold several segments with their dots, so the longest key is tried first.
    private static JToken FindNode(JToken node, string[] segments, int index)
    {
        if (node is not JObject obj)
        {
            return null;
        }

        if (index == segments.Length)
        {
            return obj;
        }

        for (var end = segments.Length; end > index; end--)
        {
            if (obj.TryGetValue(string.Join('.', segments, index, end - index), out var child) && FindNode(child, segments, end) is { } found)
            {
                return found;
            }
        }

        return null;
    }

    private static IEnumerable<string> GetNameWords(string name)
    {
        return string.IsNullOrEmpty(name) ? [] : NameWordRegex().Matches(name).Select(x => x.Value);
    }

    [GeneratedRegex("[A-Z]+(?=[A-Z][a-z])|[A-Z]?[a-z0-9]+|[A-Z]+")]
    private static partial Regex NameWordRegex();
}
