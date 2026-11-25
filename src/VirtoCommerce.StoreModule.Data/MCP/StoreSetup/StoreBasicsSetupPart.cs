using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.MCP.Core.Extensions;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.MCP.Core.Services;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model.Search;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.Data.Services.Validation;

namespace VirtoCommerce.StoreModule.Data.MCP.StoreSetup;

// Store's own fields: the name, languages, currencies, addresses and the links to fulfillment centers. The store's
// catalog is the Catalog module's part ("catalog" section), as Store cannot create a catalog.
public class StoreBasicsSetupPart(
    ISettingsManager settingsManager,
    ICurrencyService currencyService,
    ICountriesService countriesService,
    IStoreService storeService,
    IStoreSearchService storeSearchService,
    IEnumerable<IMcpLinkResolver> linkResolvers,
    IStoreSetupFindings setupFindings)
    : IStoreSetupChoices, IStoreSetupState, IStoreSetupCheck, IStoreSetupSection
{
    public const string PartName = "basics";

    // Where a person adds what the tools cannot: a platform language, or a currency (the Core module's widget).
    public const string LanguagesScreen = "Settings > Platform > General, setting Languages";
    public const string CurrenciesScreen = "Modules > Installed > Commerce core module > Currencies";
    public const string SettingsRoute = "/#!/workspace/settings";
    public const string ModulesRoute = "/#!/workspace/modules";

    // The length of the column a fulfillment center id is saved in.
    public const int MaxFulfillmentCenterIdLength = 128;

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

    // One step per check, in this order: languages, currencies, the market's currency, the storefront URL, a shared host,
    // a URL that leads to the platform itself, and the email.
    public virtual async Task<IList<McpFinding>> CheckAsync(StoreSetupContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var store = context.Store;
        var findings = new List<McpFinding> { CheckLanguages(store), CheckCurrencies(store), CheckMarketCurrency(context), CheckStorefrontUrl(store) };
        findings.AddRange(await GetSharedHostFindingsAsync(store));
        findings.Add(await CheckPlatformUrlAsync(store));
        findings.Add(CheckEmail(store));

        return findings.Where(x => x != null).ToList();
    }

    protected virtual McpFinding CheckLanguages(Store store)
    {
        var languages = store.Languages ?? [];

        return languages.Count == 0 || !languages.ContainsIgnoreCase(store.DefaultLanguage)
            ? CreateFinding(store, "basics-no-language", McpFindingSeverities.Critical,
                "The store has no language, or its default language is not one of its languages",
                "The storefront shows the catalog and the checkout in the store's languages.",
                $"Languages: {Join(languages)}. Default: {store.DefaultLanguage ?? "none"}.",
                "Add the store's languages and choose the default among them.")
            : null;
    }

    protected virtual McpFinding CheckCurrencies(Store store)
    {
        var currencies = store.Currencies ?? [];

        return currencies.Count == 0 || !currencies.ContainsIgnoreCase(store.DefaultCurrency)
            ? CreateFinding(store, "basics-no-currency", McpFindingSeverities.Critical,
                "The store has no currency, or its default currency is not one of its currencies",
                "Prices, carts and orders are in the store's currencies.",
                $"Currencies: {Join(currencies)}. Default: {store.DefaultCurrency ?? "none"}.",
                "Add the store's currencies and choose the default among them.")
            : null;
    }

    // The currency of the market the report is asked for (StoreSetupContext.Currency), when there is one.
    protected virtual McpFinding CheckMarketCurrency(StoreSetupContext context)
    {
        var store = context.Store;
        var currencies = store.Currencies ?? [];

        return !string.IsNullOrEmpty(context.Currency) && !currencies.ContainsIgnoreCase(context.Currency)
            ? CreateFinding(store, "currency-not-in-store", McpFindingSeverities.Critical,
                $"{context.Currency} is not one of the store's currencies",
                $"Customers cannot check out in {context.Currency} until the store has it.",
                $"Currencies: {Join(currencies)}.",
                $"Add {context.Currency} to the store's currencies.")
            : null;
    }

    // A storefront looks the store up by its URL; the secure URL plays no part in it.
    protected virtual McpFinding CheckStorefrontUrl(Store store)
    {
        if (!string.IsNullOrEmpty(store.Url))
        {
            return null;
        }

        var evidence = string.IsNullOrEmpty(store.SecureUrl) ? "URL: none." : $"URL: none. Secure URL: {store.SecureUrl}, which storefronts do not look up.";

        return CreateFinding(store, "basics-no-storefront-url", McpFindingSeverities.Warning,
            "The store has no storefront URL",
            "A storefront shows the store whose URL holds its host name, so a store without one shows on no address of " +
                "its own. Links in its emails and the checkout handoff need the address too.",
            evidence,
            "Ask the user for the address customers open the store at, and send it as basics.url.");
    }

    protected virtual McpFinding CheckEmail(Store store)
    {
        return string.IsNullOrEmpty(store.Email) && string.IsNullOrEmpty(store.AdminEmail)
            ? CreateFinding(store, "basics-no-email", McpFindingSeverities.Warning,
                "The store has no email",
                "The store's notifications need a sender, and the notifications to its staff a recipient.",
                "Email: none. Administrator email: none.",
                "Enter the store email and the administrator email.")
            : null;
    }

    // Plans on the copy of the store in the context, so the parts after it and the checks see the planned basics.
    public virtual async Task<McpSetupPartPlan> PlanAsync(StoreSetupContext context, object section)
    {
        ArgumentNullException.ThrowIfNull(context);

        var basics = (StoreBasicsSection)section;
        var plan = AbstractTypeFactory<StoreBasicsSetupPlan>.TryCreateInstance();
        plan.Section = basics;

        await PlanCodesAsync(plan, basics);
        PlanFields(plan, context.Store, basics);

        // A new store starts without a name, and a store's name cannot be cleared.
        if (string.IsNullOrWhiteSpace(context.Store.Name))
        {
            plan.Errors.Add(context.IsNewStore ? "A new store needs a name." : "A store needs a name: it cannot be cleared.");
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
            ("store_id", store.Id, 128),
            ("name", store.Name, 128),
            ("description", store.Description, 256),
            ("url", store.Url, 256),
            ("secure_url", store.SecureUrl, 128),
            ("email", store.Email, 128),
            ("admin_email", store.AdminEmail, 128),
            ("time_zone", store.TimeZone, 128),
            ("country", store.Country, 128),
            ("region", store.Region, 128),
            ("main_fulfillment_center_id", store.MainFulfillmentCenterId, MaxFulfillmentCenterIdLength),
        };

        foreach (var (field, _, maxLength) in fields.Where(x => x.Value?.Length > x.MaxLength))
        {
            plan.Errors.Add($"{field} takes at most {maxLength} characters.");
        }

        if (store.AdditionalFulfillmentCenterIds?.Any(x => x?.Length > MaxFulfillmentCenterIdLength) == true)
        {
            plan.Errors.Add($"Each of additional_fulfillment_center_ids takes at most {MaxFulfillmentCenterIdLength} characters.");
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
            // The saved store again, with the fields set on it as the plan set them: what is applied already changes nothing.
            store = await storeService.GetByIdAsync(context.Store.Id)
                ?? throw new InvalidOperationException($"Store '{context.Store.Id}' was not found.");
            PlanFields(AbstractTypeFactory<StoreBasicsSetupPlan>.TryCreateInstance(), store, ((StoreBasicsSetupPlan)plan).Section);
        }

        await storeService.SaveChangesAsync([store]);
    }

    // The fields a plan shows and an apply saves: the section's, then the defaults that a left-out default takes from its
    // list.
    protected virtual void PlanFields(McpSetupPartPlan plan, Store store, StoreBasicsSection section)
    {
        ApplySection(plan, store, section);
        PlanDefaults(plan, store, section);
    }

    // Sets the fields the section names and lists each change. A field that is left out changes nothing, an empty text
    // clears a field, and a list is the complete new list.
    protected virtual void ApplySection(McpSetupPartPlan plan, Store store, StoreBasicsSection section)
    {
        PlanText(plan, "name", store.Name, section.Name, x => store.Name = x);
        PlanText(plan, "description", store.Description, section.Description, x => store.Description = x);
        PlanText(plan, "default_language", store.DefaultLanguage, section.DefaultLanguage, x => store.DefaultLanguage = x);
        PlanList(plan, "languages", store.Languages, section.Languages, x => store.Languages = x, StringComparer.Ordinal);
        PlanText(plan, "default_currency", store.DefaultCurrency, section.DefaultCurrency, x => store.DefaultCurrency = x);
        PlanList(plan, "currencies", store.Currencies, section.Currencies, x => store.Currencies = x, StringComparer.Ordinal);
        PlanText(plan, "url", store.Url, section.Url, x => store.Url = x);
        PlanText(plan, "secure_url", store.SecureUrl, section.SecureUrl, x => store.SecureUrl = x);
        PlanText(plan, "email", store.Email, section.Email, x => store.Email = x);
        PlanText(plan, "admin_email", store.AdminEmail, section.AdminEmail, x => store.AdminEmail = x);
        PlanText(plan, "time_zone", store.TimeZone, section.TimeZone, x => store.TimeZone = x);
        PlanText(plan, "country", store.Country, section.Country, x => store.Country = x);
        PlanText(plan, "region", store.Region, section.Region, x => store.Region = x);
        PlanText(plan, "main_fulfillment_center_id", store.MainFulfillmentCenterId, section.MainFulfillmentCenterId, x => store.MainFulfillmentCenterId = x);
        PlanList(plan, "additional_fulfillment_center_ids", store.AdditionalFulfillmentCenterIds, section.AdditionalFulfillmentCenterIds,
            x => store.AdditionalFulfillmentCenterIds = x, StringComparer.OrdinalIgnoreCase);
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
            await AddUnknownValueErrorsAsync(plan, "Language", section.Languages, platformLanguages, "the platform's languages",
                LanguagesScreen, $"{SettingsRoute}?setting={Uri.EscapeDataString(PlatformConstants.Settings.General.Languages.Name)}");
        }

        if (section.Currencies != null || section.DefaultCurrency != null)
        {
            var platformCurrencies = (await currencyService.GetAllCurrenciesAsync() ?? []).Select(x => x.Code).ToList();
            section.Currencies = ToPlatformSpelling(section.Currencies, platformCurrencies);
            section.DefaultCurrency = ToPlatformSpelling(section.DefaultCurrency, platformCurrencies);
            await AddUnknownValueErrorsAsync(plan, "Currency", section.Currencies, platformCurrencies, "the platform's currencies",
                CurrenciesScreen, ModulesRoute);
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
        var finding = setupFindings.ForStore(store, PartName, id, severity);
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

    // comparer: whether a value in another case is a change. Codes take the platform's spelling (PlanCodesAsync) and are
    // compared exactly, as the defaults are (PlanText), so "eur" saved by hand becomes "EUR"; ids ignore the case. A
    // value in another spelling is not reported as removed.
    protected virtual void PlanList(McpSetupPartPlan plan, string field, ICollection<string> current, IList<string> requested, Action<IList<string>> set,
        StringComparer comparer)
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

        if (values.Count == currentValues.Count && values.TrueForAll(x => currentValues.Contains(x, comparer)))
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

    // No tool adds a language or a currency to the platform: the refusal names the screen where a person does, with its
    // link when the MCP module can build one.
    protected virtual async Task AddUnknownValueErrorsAsync(McpSetupPartPlan plan, string kind, IList<string> requested, IList<string> known,
        string knownName, string screen, string route)
    {
        var unknown = (requested ?? []).Where(x => !string.IsNullOrWhiteSpace(x) && !known.ContainsIgnoreCase(x.Trim())).ToList();

        if (unknown.Count == 0)
        {
            return;
        }

        var where = screen.WithLink(await GetLinkAsync(route));

        foreach (var value in unknown)
        {
            plan.Errors.Add($"{kind} '{value}' is not one of {knownName}: {Join(known)}. A person adds it in the back office: {where}.");
        }
    }

    // A default that is left out becomes the first of its list, as a row, on a store without a default whose list the
    // request sends: a new store, whose lists come only from the request, or a store that lost its default. A store that
    // has a default keeps it.
    protected virtual void PlanDefaults(McpSetupPartPlan plan, Store store, StoreBasicsSection section)
    {
        if (section.DefaultLanguage == null && string.IsNullOrEmpty(store.DefaultLanguage) && section.Languages != null)
        {
            PlanText(plan, "default_language", store.DefaultLanguage, store.Languages?.FirstOrDefault(), x => store.DefaultLanguage = x);
        }

        if (section.DefaultCurrency == null && string.IsNullOrEmpty(store.DefaultCurrency) && section.Currencies != null)
        {
            PlanText(plan, "default_currency", store.DefaultCurrency, store.Currencies?.FirstOrDefault(), x => store.DefaultCurrency = x);
        }
    }

    // A storefront asks the platform for the store of its host name and gets one store: the first by name whose URL holds
    // "://<host>", whatever the port, unless the platform's configuration maps the host to a store. Another store found
    // at this store's host, or this store found at another store's host, competes for the same storefront. The finding
    // names no other store, nor its address: a user may read some stores only.
    protected virtual async Task<IList<McpFinding>> GetSharedHostFindingsAsync(Store store)
    {
        var host = GetHost(store.Url);

        if (host == null)
        {
            return [];
        }

        var stores = await storeSearchService.SearchAllNoCloneAsync(AbstractTypeFactory<StoreSearchCriteria>.TryCreateInstance());
        var others = stores.Where(x => !x.Id.EqualsIgnoreCase(store.Id)).ToList();
        var atOwnHost = others.Any(x => HoldsHost(x.Url, host));
        var atOtherHost = others.Any(x => GetHost(x.Url) is { } otherHost && !otherHost.EqualsIgnoreCase(host) && HoldsHost(store.Url, otherHost));

        if (!atOwnHost && !atOtherHost)
        {
            return [];
        }

        var evidence = new List<string>();

        if (atOwnHost)
        {
            evidence.Add($"Another store's URL holds {host}.");
        }

        if (atOtherHost)
        {
            evidence.Add("This store's URL holds another store's host, so a storefront there can show this store.");
        }

        return
        [
            CreateFinding(store, "basics-host-shared", McpFindingSeverities.Warning,
                "Another store uses this storefront address",
                "A storefront shows one store for its host name: the first by name whose URL holds it, unless the platform's " +
                    "configuration maps the host to a store, which this check cannot see.",
                string.Join(" ", evidence),
                "Give each store an address of its own in basics.url, or map the host to its store in the platform's configuration."),
        ];
    }

    // A URL that leads to the platform itself opens the back office, whose start page is its sign-in, not a storefront. The
    // platform's address is the back office's link, so without the MCP module's links the check says nothing.
    protected virtual async Task<McpFinding> CheckPlatformUrlAsync(Store store)
    {
        if (string.IsNullOrEmpty(store.Url))
        {
            return null;
        }

        var platformUrl = await GetLinkAsync("/");

        return platformUrl != null && LeadsToPlatform(store.Url, platformUrl)
            ? CreateFinding(store, "basics-url-is-the-platform", McpFindingSeverities.Warning,
                "The store's URL is the platform's address, not a storefront's",
                "The address opens the back office, whose start page is its sign-in, so customers who open it never see the store.",
                $"URL: {store.Url}. The back office: {platformUrl}.",
                "Ask the user for the address of the storefront that shows the store, and send it as basics.url.")
            : null;
    }

    // Whether the URL leads to the platform: the host and the port of the platform's address. Every name under localhost,
    // and a loopback address, leads to this computer, so with the platform's port such a URL leads to the platform too.
    protected virtual bool LeadsToPlatform(string url, string platformUrl)
    {
        if (!Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var target) || !Uri.TryCreate(platformUrl, UriKind.Absolute, out var platform) ||
            target.Port != platform.Port)
        {
            return false;
        }

        return target.Host.EqualsIgnoreCase(platform.Host) || (IsThisComputer(target.Host) && IsThisComputer(platform.Host));
    }

    // The MCP module's link to a screen of the classic back office; null without the MCP module or outside its requests.
    protected virtual async Task<string> GetLinkAsync(string route)
    {
        var linkResolver = linkResolvers?.LastOrDefault();

        return linkResolver == null ? null : await linkResolver.GetLinkAsync(route);
    }

    protected virtual void AddDefaultError(McpSetupPartPlan plan, string kind, string kinds, string defaultValue, ICollection<string> values)
    {
        if (!string.IsNullOrEmpty(defaultValue) && !(values ?? []).ContainsIgnoreCase(defaultValue))
        {
            plan.Errors.Add($"The default {kind} '{defaultValue}' is not one of the store's {kinds}. Send the default and the list together.");
        }
    }

    // The host a storefront finds the store by; null for a URL that is not absolute.
    protected virtual string GetHost(string url)
    {
        return Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri) && !string.IsNullOrEmpty(uri.Host) ? uri.Host : null;
    }

    // The platform's rule for a storefront's host: the URL holds "://<host>".
    protected virtual bool HoldsHost(string url, string host)
    {
        return url?.Contains($"://{host}", StringComparison.OrdinalIgnoreCase) == true;
    }

    protected virtual string GetLanguageTitle(string code)
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

    private static bool IsThisComputer(string host)
    {
        return host.EqualsIgnoreCase("localhost") || host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase) ||
            (IPAddress.TryParse(host.Trim('[', ']'), out var address) && IPAddress.IsLoopback(address));
    }

    private static string Join(ICollection<string> values)
    {
        return values.Count == 0 ? "none" : string.Join(", ", values);
    }
}
