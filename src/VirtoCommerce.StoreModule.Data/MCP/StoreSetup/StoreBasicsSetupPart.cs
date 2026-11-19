using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.Data.Services.Validation;

namespace VirtoCommerce.StoreModule.Data.MCP.StoreSetup;

// Store's own fields: the name, languages, currencies, addresses and the links to fulfillment centers. The store's
// catalog is the Catalog module's part ("catalog" section), as Store cannot create a catalog.
public class StoreBasicsSetupPart(
    ISettingsManager settingsManager,
    ICurrencyService currencyService,
    ICountriesService countriesService,
    IStoreService storeService)
    : IStoreSetupChoices, IStoreSetupState, IStoreSetupCheck, IStoreSetupSection
{
    public const string PartName = "basics";

    public virtual string Name => PartName;

    public virtual string Title => "Store basics";

    public virtual McpSetupPermissions Permissions => StoreSetupPermissions.ForStore();

    public virtual Type SectionType => typeof(StoreBasicsSection);

    // Without basics a store cannot be created: they hold its name.
    public virtual bool IsRequiredForNewStore => true;

    public virtual async Task<IList<McpSetupChoice>> GetChoicesAsync()
    {
        var choices = new List<McpSetupChoice>();

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
        section.MainFulfillmentCenterId = store.MainFulfillmentCenterId;
        section.AdditionalFulfillmentCenterIds = store.AdditionalFulfillmentCenterIds?.ToList() ?? [];

        return Task.FromResult<object>(section);
    }

    public virtual Task<IList<McpFinding>> CheckAsync(StoreSetupContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var store = context.Store;
        var languages = store.Languages ?? [];
        var currencies = store.Currencies ?? [];
        var findings = new List<McpFinding>();

        if (languages.Count == 0 || !languages.ContainsIgnoreCase(store.DefaultLanguage))
        {
            findings.Add(CreateFinding(store, "basics-no-language", McpFindingSeverities.Critical,
                "The store has no language, or its default language is not one of its languages",
                "The storefront shows the catalog and the checkout in the store's languages.",
                $"Languages: {Join(languages)}. Default: {store.DefaultLanguage ?? "none"}.",
                "Add the store's languages and choose the default among them."));
        }

        if (currencies.Count == 0 || !currencies.ContainsIgnoreCase(store.DefaultCurrency))
        {
            findings.Add(CreateFinding(store, "basics-no-currency", McpFindingSeverities.Critical,
                "The store has no currency, or its default currency is not one of its currencies",
                "Prices, carts and orders are in the store's currencies.",
                $"Currencies: {Join(currencies)}. Default: {store.DefaultCurrency ?? "none"}.",
                "Add the store's currencies and choose the default among them."));
        }

        if (!string.IsNullOrEmpty(context.Currency) && !currencies.ContainsIgnoreCase(context.Currency))
        {
            findings.Add(CreateFinding(store, "currency-not-in-store", McpFindingSeverities.Critical,
                $"{context.Currency} is not one of the store's currencies",
                $"Customers cannot check out in {context.Currency} until the store has it.",
                $"Currencies: {Join(currencies)}.",
                $"Add {context.Currency} to the store's currencies."));
        }

        if (string.IsNullOrEmpty(store.Url) && string.IsNullOrEmpty(store.SecureUrl))
        {
            findings.Add(CreateFinding(store, "basics-no-storefront-url", McpFindingSeverities.Warning,
                "The store has no storefront URL",
                "Links in the store's emails and the checkout handoff need the storefront's address.",
                "URL: none. HTTPS URL: none.",
                "Enter the storefront URL."));
        }

        if (string.IsNullOrEmpty(store.Email) && string.IsNullOrEmpty(store.AdminEmail))
        {
            findings.Add(CreateFinding(store, "basics-no-email", McpFindingSeverities.Warning,
                "The store has no email",
                "The store's notifications need a sender, and the notifications to its staff a recipient.",
                "Email: none. Administrator email: none.",
                "Enter the store email and the administrator email."));
        }

        return Task.FromResult<IList<McpFinding>>(findings);
    }

    // Plans on the copy of the store in the context, so the parts after it and the checks see the planned basics.
    public virtual async Task<McpSetupPartPlan> PlanAsync(StoreSetupContext context, object section)
    {
        ArgumentNullException.ThrowIfNull(context);

        var basics = (StoreBasicsSection)section;
        var plan = AbstractTypeFactory<StoreBasicsSetupPlan>.TryCreateInstance();
        plan.Section = basics;

        if (context.IsNewStore && string.IsNullOrWhiteSpace(basics.Name))
        {
            plan.Errors.Add("A new store needs a name.");
        }

        await PlanCodesAsync(plan, basics);
        ApplySection(plan, context.Store, basics);

        if (!context.IsNewStore && string.IsNullOrWhiteSpace(context.Store.Name))
        {
            plan.Errors.Add("A store needs a name: it cannot be cleared.");
        }

        if (context.IsNewStore)
        {
            AddValidationErrors(plan, context.Store);
        }
        AddLengthErrors(plan, context.Store);
        Validate(plan, context.Store, basics);

        return plan;
    }

    // The lengths of the store's columns: a longer text would be refused only when the store is saved, after the approval.
    protected virtual void AddLengthErrors(McpSetupPartPlan plan, Store store)
    {
        var fields = new (string Field, string Value, int MaxLength)[]
        {
            ("name", store.Name, 128),
            ("description", store.Description, 256),
            ("url", store.Url, 256),
            ("secure_url", store.SecureUrl, 128),
            ("email", store.Email, 128),
            ("admin_email", store.AdminEmail, 128),
            ("time_zone", store.TimeZone, 128),
            ("country", store.Country, 128),
            ("region", store.Region, 128),
            ("main_fulfillment_center_id", store.MainFulfillmentCenterId, 128),
        };

        foreach (var (field, value, maxLength) in fields.Where(x => x.Value?.Length > x.MaxLength))
        {
            plan.Errors.Add($"{field} takes at most {maxLength} characters.");
        }
    }

    public virtual async Task ApplyAsync(StoreSetupContext context, McpSetupPartPlan plan)
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
    protected virtual void ApplySection(McpSetupPartPlan plan, Store store, StoreBasicsSection section)
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
        PlanText(plan, "main_fulfillment_center_id", store.MainFulfillmentCenterId, section.MainFulfillmentCenterId, x => store.MainFulfillmentCenterId = x);
        PlanList(plan, "additional_fulfillment_center_ids", store.AdditionalFulfillmentCenterIds, section.AdditionalFulfillmentCenterIds, x => store.AdditionalFulfillmentCenterIds = x);
    }

    // Before the section is applied: each language, currency and country code takes the platform's spelling, as pricing
    // and the cart compare codes exactly ("eur" becomes "EUR"), and a code the platform does not have is refused. The
    // section is the request's, read for this plan.
    protected virtual async Task PlanCodesAsync(McpSetupPartPlan plan, StoreBasicsSection section)
    {
        if (section.Languages != null || section.DefaultLanguage != null)
        {
            var platformLanguages = await GetPlatformLanguagesAsync();
            section.Languages = ToPlatformSpelling(section.Languages, platformLanguages);
            section.DefaultLanguage = ToPlatformSpelling(section.DefaultLanguage, platformLanguages);
            AddUnknownValueErrors(plan, "Language", section.Languages, platformLanguages, "the platform's languages");
        }

        if (section.Currencies != null || section.DefaultCurrency != null)
        {
            var platformCurrencies = (await currencyService.GetAllCurrenciesAsync() ?? []).Select(x => x.Code).ToList();
            section.Currencies = ToPlatformSpelling(section.Currencies, platformCurrencies);
            section.DefaultCurrency = ToPlatformSpelling(section.DefaultCurrency, platformCurrencies);
            AddUnknownValueErrors(plan, "Currency", section.Currencies, platformCurrencies, "the platform's currencies");
        }

        if (!string.IsNullOrWhiteSpace(section.Country))
        {
            var countries = (await countriesService.GetCountriesAsync() ?? []).Select(x => x.Id).ToList();
            section.Country = ToPlatformSpelling(section.Country, countries);

            if (!countries.ContainsIgnoreCase(section.Country.Trim()))
            {
                plan.Errors.Add($"Country '{section.Country}' is not one of the platform's countries. Use its ISO 3166-1 alpha-3 code, such as DEU.");
            }
        }
    }

    // After the section is applied: each default must be one of its list, and the addresses must be addresses. The links
    // to fulfillment centers are not checked: their module owns them.
    protected virtual void Validate(McpSetupPartPlan plan, Store store, StoreBasicsSection section)
    {
        if (section.Languages != null || section.DefaultLanguage != null)
        {
            AddDefaultError(plan, "language", "languages", store.DefaultLanguage, store.Languages);
        }

        if (section.Currencies != null || section.DefaultCurrency != null)
        {
            AddDefaultError(plan, "currency", "currencies", store.DefaultCurrency, store.Currencies);
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
            if (!email.Trim().IsValidEmail())
            {
                plan.Errors.Add($"The {field} '{email}' is not an email address.");
            }
        }
    }

    // A new store must pass the checks StoreService runs when it saves a store, here before anything is saved.
    protected virtual void AddValidationErrors(McpSetupPartPlan plan, Store store)
    {
        var errors = new StoreValidator().Validate(store).Errors
            .Select(x => x.PropertyName == nameof(Store.Id) ? "The store id may hold letters, digits, '_' and '-' only." : x.ErrorMessage)
            .Distinct();

        foreach (var error in errors)
        {
            plan.Errors.Add(error);
        }
    }

    protected virtual async Task<IList<string>> GetPlatformLanguagesAsync()
    {
        var languages = await settingsManager.GetObjectSettingAsync(PlatformConstants.Settings.General.Languages.Name);

        return (languages?.AllowedValues ?? [])
            .Select(x => x?.ToString())
            .Where(x => !string.IsNullOrEmpty(x))
            .DistinctIgnoreCase()
            .ToList();
    }

    protected virtual McpSetupChoice CreateChoice(string field, string value, string title)
    {
        var choice = AbstractTypeFactory<McpSetupChoice>.TryCreateInstance();
        choice.Field = $"{PartName}.{field}";
        choice.Value = value;
        choice.Title = title;

        return choice;
    }

    protected virtual McpFinding CreateFinding(Store store, string id, string severity, string title, string why, string evidence, string fix)
    {
        var finding = StoreSetupFindings.ForStore(store, PartName, id, severity);
        finding.Title = title;
        finding.Why = why;
        finding.Evidence = evidence;
        finding.Fix = fix;

        return finding;
    }

    // The platform's spelling of a code, or the code as sent when the platform does not have it.
    protected virtual string ToPlatformSpelling(string code, IList<string> platformCodes)
    {
        return string.IsNullOrWhiteSpace(code) ? code : platformCodes.FirstOrDefault(x => x.EqualsIgnoreCase(code.Trim())) ?? code;
    }

    protected virtual IList<string> ToPlatformSpelling(IList<string> codes, IList<string> platformCodes)
    {
        return codes?.Select(x => ToPlatformSpelling(x, platformCodes)).ToList();
    }

    protected virtual void PlanText(McpSetupPartPlan plan, string field, string current, string requested, Action<string> set)
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

        var newValue = value.EmptyToNull();
        plan.AddChange(field, current, newValue);
        set(newValue);
    }

    protected virtual void PlanList(McpSetupPartPlan plan, string field, ICollection<string> current, IList<string> requested, Action<IList<string>> set)
    {
        if (requested == null)
        {
            return;
        }

        var currentValues = current?.ToList() ?? [];
        var values = requested
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .DistinctIgnoreCase()
            .ToList();

        if (values.Count == currentValues.Count && values.TrueForAll(x => currentValues.ContainsIgnoreCase(x)))
        {
            return;
        }

        plan.AddChange(field, currentValues, values);

        var removed = currentValues.Where(x => !values.ContainsIgnoreCase(x)).ToList();

        if (removed.Count > 0)
        {
            plan.Warnings.Add($"Removes from {field}: {string.Join(", ", removed)}.");
        }

        set(values);
    }

    protected virtual void AddUnknownValueErrors(McpSetupPartPlan plan, string kind, IList<string> requested, IList<string> known, string knownName)
    {
        foreach (var value in (requested ?? []).Where(x => !string.IsNullOrWhiteSpace(x) && !known.ContainsIgnoreCase(x.Trim())))
        {
            plan.Errors.Add($"{kind} '{value}' is not one of {knownName}: {Join(known)}.");
        }
    }

    protected virtual void AddDefaultError(McpSetupPartPlan plan, string kind, string kinds, string defaultValue, ICollection<string> values)
    {
        if (!string.IsNullOrEmpty(defaultValue) && !(values ?? []).ContainsIgnoreCase(defaultValue))
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
}
