using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Data.MCP.StoreSetup;

// Store's own fields: the name, languages, currencies, addresses and the links to a catalog and fulfillment centers.
public class StoreBasicsSetupPart(ISettingsManager settingsManager, ICurrencyService currencyService)
    : IStoreSetupChoices, IStoreSetupState, IStoreSetupCheck
{
    public const string PartName = "basics";

    public virtual string Name => PartName;

    public virtual string Title => "Store basics";

    public virtual StoreSetupPermissions Permissions => StoreSetupPermissions.ForStore();

    public virtual async Task<IList<StoreSetupChoice>> GetChoicesAsync()
    {
        var choices = new List<StoreSetupChoice>();

        var languages = await settingsManager.GetObjectSettingAsync(PlatformConstants.Settings.General.Languages.Name);
        foreach (var language in (languages?.AllowedValues ?? []).Select(x => x?.ToString()).Where(x => !string.IsNullOrEmpty(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            choices.Add(CreateChoice("languages", language, GetLanguageTitle(language)));
        }

        foreach (var currency in await currencyService.GetAllCurrenciesAsync() ?? [])
        {
            choices.Add(CreateChoice("currencies", currency.Code, currency.Name));
        }

        return choices;
    }

    public virtual Task<object> GetStateAsync(Store store)
    {
        ArgumentNullException.ThrowIfNull(store);

        var section = AbstractTypeFactory<StoreBasicsSection>.TryCreateInstance();
        section.Name = store.Name;
        section.Description = store.Description;
        section.DefaultLanguage = store.DefaultLanguage;
        section.Languages = store.Languages?.ToList() ?? [];
        section.DefaultCurrency = store.DefaultCurrency;
        section.Currencies = store.Currencies?.ToList() ?? [];
        section.Url = store.Url;
        section.SecureUrl = store.SecureUrl;
        section.Email = store.Email;
        section.AdminEmail = store.AdminEmail;
        section.TimeZone = store.TimeZone;
        section.Country = store.Country;
        section.Region = store.Region;
        section.CatalogId = store.Catalog;
        section.MainFulfillmentCenterId = store.MainFulfillmentCenterId;
        section.AdditionalFulfillmentCenterIds = store.AdditionalFulfillmentCenterIds?.ToList() ?? [];

        return Task.FromResult<object>(section);
    }

    public virtual Task<IList<StoreSetupFinding>> CheckAsync(StoreSetupContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var store = context.Store;
        var languages = store.Languages ?? [];
        var currencies = store.Currencies ?? [];
        var findings = new List<StoreSetupFinding>();

        if (languages.Count == 0 || !languages.Contains(store.DefaultLanguage, StringComparer.OrdinalIgnoreCase))
        {
            findings.Add(CreateFinding(store, "basics-no-language", StoreSetupFindingSeverity.Blocking,
                "The store has no language, or its default language is not one of its languages",
                "The storefront shows the catalog and the checkout in the store's languages.",
                $"Languages: {Join(languages)}. Default: {store.DefaultLanguage ?? "none"}.",
                "Add the store's languages and choose the default among them."));
        }

        if (currencies.Count == 0 || !currencies.Contains(store.DefaultCurrency, StringComparer.OrdinalIgnoreCase))
        {
            findings.Add(CreateFinding(store, "basics-no-currency", StoreSetupFindingSeverity.Blocking,
                "The store has no currency, or its default currency is not one of its currencies",
                "Prices, carts and orders are in the store's currencies.",
                $"Currencies: {Join(currencies)}. Default: {store.DefaultCurrency ?? "none"}.",
                "Add the store's currencies and choose the default among them."));
        }

        if (!string.IsNullOrEmpty(context.Currency) && !currencies.Contains(context.Currency, StringComparer.OrdinalIgnoreCase))
        {
            findings.Add(CreateFinding(store, "currency-not-in-store", StoreSetupFindingSeverity.Blocking,
                $"{context.Currency} is not one of the store's currencies",
                $"Customers cannot check out in {context.Currency} until the store has it.",
                $"Currencies: {Join(currencies)}.",
                $"Add {context.Currency} to the store's currencies."));
        }

        if (string.IsNullOrEmpty(store.Catalog))
        {
            findings.Add(CreateFinding(store, "basics-no-catalog", StoreSetupFindingSeverity.Warning,
                "No catalog is linked to the store",
                "Without a catalog the store has nothing to sell.",
                "Catalog: none.",
                "Link the catalog the store sells from."));
        }

        if (string.IsNullOrEmpty(store.Url) && string.IsNullOrEmpty(store.SecureUrl))
        {
            findings.Add(CreateFinding(store, "basics-no-storefront-url", StoreSetupFindingSeverity.Warning,
                "The store has no storefront URL",
                "Links in the store's emails and the checkout handoff need the storefront's address.",
                "URL: none. HTTPS URL: none.",
                "Enter the storefront URL."));
        }

        if (string.IsNullOrEmpty(store.Email) && string.IsNullOrEmpty(store.AdminEmail))
        {
            findings.Add(CreateFinding(store, "basics-no-email", StoreSetupFindingSeverity.Warning,
                "The store has no email",
                "The store's notifications need a sender, and the notifications to its staff a recipient.",
                "Email: none. Administrator email: none.",
                "Enter the store email and the administrator email."));
        }

        return Task.FromResult<IList<StoreSetupFinding>>(findings);
    }

    protected virtual StoreSetupChoice CreateChoice(string field, string value, string title)
    {
        var choice = AbstractTypeFactory<StoreSetupChoice>.TryCreateInstance();
        choice.Field = $"{PartName}.{field}";
        choice.Value = value;
        choice.Title = title;

        return choice;
    }

    protected virtual StoreSetupFinding CreateFinding(Store store, string id, StoreSetupFindingSeverity severity, string title, string why, string evidence, string fix)
    {
        var finding = AbstractTypeFactory<StoreSetupFinding>.TryCreateInstance();
        finding.Id = id;
        finding.Part = PartName;
        finding.Severity = severity;
        finding.Title = title;
        finding.Why = why;
        finding.Evidence = evidence;
        finding.Fix = fix;
        finding.Screen = $"Stores > {store.Name ?? store.Id}";
        finding.ObjectType = nameof(Store);
        finding.ObjectId = store.Id;
        finding.ObjectName = store.Name;

        return finding;
    }

    private static string GetLanguageTitle(string code)
    {
        try
        {
            return CultureInfo.GetCultureInfo(code).EnglishName;
        }
        catch (CultureNotFoundException)
        {
            return code;
        }
    }

    private static string Join(ICollection<string> values)
    {
        return values.Count == 0 ? "none" : string.Join(", ", values);
    }
}
