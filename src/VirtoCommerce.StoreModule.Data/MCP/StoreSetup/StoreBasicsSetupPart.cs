using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.StoreModule.Data.MCP.StoreSetup;

// Store's own fields: the name, languages, currencies, addresses and the links to a catalog and fulfillment centers.
public partial class StoreBasicsSetupPart(
    ISettingsManager settingsManager,
    ICurrencyService currencyService,
    ICountriesService countriesService,
    IStoreService storeService)
    : IStoreSetupChoices, IStoreSetupState, IStoreSetupCheck, IStoreSetupSection
{
    public const string PartName = "basics";

    public virtual string Name => PartName;

    public virtual string Title => "Store basics";

    public virtual StoreSetupPermissions Permissions => StoreSetupPermissions.ForStore();

    public virtual Type SectionType => typeof(StoreBasicsSection);

    // Without basics a store cannot be created: they hold its name.
    public virtual bool IsRequiredForNewStore => true;

    public virtual async Task<IList<StoreSetupChoice>> GetChoicesAsync()
    {
        var choices = new List<StoreSetupChoice>();

        foreach (var language in await GetPlatformLanguagesAsync())
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

    // Plans on the copy of the store in the context, so the parts after it and the checks see the planned basics.
    public virtual async Task<StoreSetupPartPlan> PlanAsync(StoreSetupContext context, object section)
    {
        ArgumentNullException.ThrowIfNull(context);

        var basics = (StoreBasicsSection)section;
        var plan = AbstractTypeFactory<StoreBasicsSetupPlan>.TryCreateInstance();
        plan.Part = PartName;
        plan.Section = basics;

        if (context.IsNewStore)
        {
            if (string.IsNullOrEmpty(context.Store.Id) || !StoreIdRegex().IsMatch(context.Store.Id))
            {
                plan.Errors.Add("The store id may hold letters, digits, '_' and '-' only.");
            }

            if (string.IsNullOrWhiteSpace(basics.Name))
            {
                plan.Errors.Add("A new store needs a name.");
            }
        }

        ApplySection(plan, context.Store, basics);
        await ValidateAsync(plan, context.Store, basics);

        return plan;
    }

    public virtual async Task ApplyAsync(StoreSetupContext context, StoreSetupPartPlan plan)
    {
        ArgumentNullException.ThrowIfNull(context);

        Store store;

        if (context.IsNewStore)
        {
            // The planned store, without its settings: the settings part saves them.
            store = context.Store.CloneTyped();
            store.Settings = [];
        }
        else
        {
            // The saved store again, with the section applied to it: what is applied already changes nothing.
            store = await storeService.GetByIdAsync(context.Store.Id)
                ?? throw new InvalidOperationException($"Store '{context.Store.Id}' was not found.");
            ApplySection(AbstractTypeFactory<StoreBasicsSetupPlan>.TryCreateInstance(), store, ((StoreBasicsSetupPlan)plan).Section);
        }

        await storeService.SaveChangesAsync([store]);
    }

    // Sets the fields the section names and lists each change. A field that is left out changes nothing, an empty text
    // clears a field, and a list is the complete new list.
    protected virtual void ApplySection(StoreSetupPartPlan plan, Store store, StoreBasicsSection section)
    {
        PlanText(plan, "name", store.Name, section.Name, x => store.Name = x);
        PlanText(plan, "description", store.Description, section.Description, x => store.Description = x);
        PlanText(plan, "default_language", store.DefaultLanguage, section.DefaultLanguage, x => store.DefaultLanguage = x);
        PlanList(plan, "languages", store.Languages, section.Languages, x => store.Languages = x);
        PlanText(plan, "default_currency", store.DefaultCurrency, section.DefaultCurrency, x => store.DefaultCurrency = x);
        PlanList(plan, "currencies", store.Currencies, section.Currencies, x => store.Currencies = x);
        PlanText(plan, "url", store.Url, section.Url, x => store.Url = x);
        PlanText(plan, "secure_url", store.SecureUrl, section.SecureUrl, x => store.SecureUrl = x);
        PlanText(plan, "email", store.Email, section.Email, x => store.Email = x);
        PlanText(plan, "admin_email", store.AdminEmail, section.AdminEmail, x => store.AdminEmail = x);
        PlanText(plan, "time_zone", store.TimeZone, section.TimeZone, x => store.TimeZone = x);
        PlanText(plan, "country", store.Country, section.Country, x => store.Country = x);
        PlanText(plan, "region", store.Region, section.Region, x => store.Region = x);
        PlanText(plan, "catalog_id", store.Catalog, section.CatalogId, x => store.Catalog = x);
        PlanText(plan, "main_fulfillment_center_id", store.MainFulfillmentCenterId, section.MainFulfillmentCenterId, x => store.MainFulfillmentCenterId = x);
        PlanList(plan, "additional_fulfillment_center_ids", store.AdditionalFulfillmentCenterIds, section.AdditionalFulfillmentCenterIds, x => store.AdditionalFulfillmentCenterIds = x);
    }

    // The languages, currencies and country must be the platform's; each default must be one of its list. The links to a
    // catalog and fulfillment centers are not checked: their modules own them.
    protected virtual async Task ValidateAsync(StoreSetupPartPlan plan, Store store, StoreBasicsSection section)
    {
        if (section.Languages != null || section.DefaultLanguage != null)
        {
            var platformLanguages = await GetPlatformLanguagesAsync();
            AddUnknownValueErrors(plan, "Language", section.Languages, platformLanguages, "the platform's languages");
            AddDefaultError(plan, "language", "languages", store.DefaultLanguage, store.Languages);
        }

        if (section.Currencies != null || section.DefaultCurrency != null)
        {
            var platformCurrencies = (await currencyService.GetAllCurrenciesAsync() ?? []).Select(x => x.Code).ToList();
            AddUnknownValueErrors(plan, "Currency", section.Currencies, platformCurrencies, "the platform's currencies");
            AddDefaultError(plan, "currency", "currencies", store.DefaultCurrency, store.Currencies);
        }

        if (!string.IsNullOrWhiteSpace(section.Country))
        {
            var countries = await countriesService.GetCountriesAsync() ?? [];

            if (!countries.Any(x => x.Id.EqualsIgnoreCase(section.Country.Trim())))
            {
                plan.Errors.Add($"Country '{section.Country}' is not one of the platform's countries. Use its ISO 3166-1 alpha-3 code, such as DEU.");
            }
        }

        foreach (var (field, url) in new[] { ("url", section.Url), ("secure_url", section.SecureUrl) }.Where(x => !string.IsNullOrWhiteSpace(x.Item2)))
        {
            if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
            {
                plan.Errors.Add($"The {field} '{url}' is not an absolute http or https address.");
            }
        }

        foreach (var (field, email) in new[] { ("email", section.Email), ("admin_email", section.AdminEmail) }.Where(x => !string.IsNullOrWhiteSpace(x.Item2)))
        {
            if (!MailAddress.TryCreate(email.Trim(), out _))
            {
                plan.Errors.Add($"The {field} '{email}' is not an email address.");
            }
        }
    }

    protected virtual async Task<IList<string>> GetPlatformLanguagesAsync()
    {
        var languages = await settingsManager.GetObjectSettingAsync(PlatformConstants.Settings.General.Languages.Name);

        return (languages?.AllowedValues ?? [])
            .Select(x => x?.ToString())
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
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

    private static void PlanText(StoreSetupPartPlan plan, string field, string current, string requested, Action<string> set)
    {
        if (requested == null)
        {
            return;
        }

        var value = requested.Trim();

        if (string.Equals(current ?? string.Empty, value, StringComparison.Ordinal))
        {
            return;
        }

        var newValue = value.Length == 0 ? null : value;
        plan.AddChange(field, current, newValue);
        set(newValue);
    }

    private static void PlanList(StoreSetupPartPlan plan, string field, ICollection<string> current, IList<string> requested, Action<IList<string>> set)
    {
        if (requested == null)
        {
            return;
        }

        var currentValues = current?.ToList() ?? [];
        var values = requested
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (values.Count == currentValues.Count && values.TrueForAll(x => currentValues.Contains(x, StringComparer.OrdinalIgnoreCase)))
        {
            return;
        }

        plan.AddChange(field, currentValues, values);

        var removed = currentValues.Where(x => !values.Contains(x, StringComparer.OrdinalIgnoreCase)).ToList();

        if (removed.Count > 0)
        {
            plan.Warnings.Add($"Removes from {field}: {string.Join(", ", removed)}.");
        }

        set(values);
    }

    private static void AddUnknownValueErrors(StoreSetupPartPlan plan, string kind, IList<string> requested, IList<string> known, string knownName)
    {
        foreach (var value in (requested ?? []).Where(x => !string.IsNullOrWhiteSpace(x) && !known.Contains(x.Trim(), StringComparer.OrdinalIgnoreCase)))
        {
            plan.Errors.Add($"{kind} '{value}' is not one of {knownName}: {Join(known)}.");
        }
    }

    private static void AddDefaultError(StoreSetupPartPlan plan, string kind, string kinds, string defaultValue, ICollection<string> values)
    {
        if (!string.IsNullOrEmpty(defaultValue) && !(values ?? []).Contains(defaultValue, StringComparer.OrdinalIgnoreCase))
        {
            plan.Errors.Add($"The default {kind} '{defaultValue}' is not one of the store's {kinds}. Send the default and the list together.");
        }
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

    [GeneratedRegex("^[a-zA-Z0-9_-]+$")]
    private static partial Regex StoreIdRegex();
}
