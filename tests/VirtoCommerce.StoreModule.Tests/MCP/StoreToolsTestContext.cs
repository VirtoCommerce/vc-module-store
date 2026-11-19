using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.MCP.Core.Services;
using VirtoCommerce.MCP.Testing;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Localizations;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Web;
using Xunit;

namespace VirtoCommerce.StoreModule.Tests.MCP;

// The store admin tools wired by Store's own Module.Initialize with the MCP module installed, over stores held in memory.
public static class StoreToolsTestContext
{
    public const string StoreA = "store-a";
    public const string StoreB = "store-b";
    public const string MapsApiKeyValue = "AIza-maps-secret";
    public const string PublicOrigin = "https://admin.example.com";

    // What a user without catalog:read gets in place of the finding of CreateCatalogPart.
    public const string CatalogNotShown = "Test part catalog: 1 finding not shown, 1 blocking going live (needs catalog:read)";

    // The role of a store administrator who works through an AI agent, as the README writes it down.
    public static readonly string[] RolePermissions =
    [
        "store:access", "store:read", "store:create", "store:update", "shipping:read", "shipping:update", "tax:read",
        "tax:update", "platform:setting:read", "mcp:audit:read",
    ];

    public static readonly SettingDescriptor TaxCalculation = new() { Name = "Stores.TaxCalculationEnabled", ModuleId = "VirtoCommerce.Store", ValueType = SettingValueType.Boolean, DefaultValue = false };
    public static readonly SettingDescriptor MapsApiKey = new() { Name = "Shipping.Bopis.GoogleMaps.ApiKey", ModuleId = "VirtoCommerce.Shipping", ValueType = SettingValueType.ShortText };

    public static McpToolHarness CreateHarness(params IStoreSetupPart[] extraParts)
    {
        return CreateHarness(new StoreToolsTestData(), extraParts);
    }

    public static McpToolHarness CreateHarness(StoreToolsTestData data, params IStoreSetupPart[] extraParts)
    {
        return McpToolHarness.Create(
            services =>
            {
                CreateModule(mcpInstalled: true).Initialize(services);

                // What the platform, the database and the MCP module provide.
                services.AddSingleton(TestStoreService.Create(data.Stores).Object);
                services.AddSingleton<IStoreSearchService>(new InMemoryStoreSearchService(data.Stores));
                services.AddSingleton(CreatePendingChangeSearchService(data.PendingChanges));
                services.AddSingleton(CreateCountriesService());
                services.AddSingleton<IMcpPageSizeResolver, FixedPageSizeResolver>();
                services.AddSingleton<IMcpPublicOriginResolver>(new McpPublicOriginResolver(Options.Create(new McpOptions { PublicOrigin = PublicOrigin })));
                services.AddSingleton(CreateSettingsRegistrar());
                services.AddSingleton(CreateTranslationService());
                services.AddSingleton(CreateSettingsManager());
                services.AddSingleton(CreateCurrencyService());

                // The Catalog module's part, which a real installation has: a new store's catalog comes from it.
                services.AddSingleton<IStoreSetupPart>(new TestCatalogPart());

                foreach (var part in extraParts)
                {
                    services.AddSingleton(part);
                }
            },
            configureServices: null,
            scope: null);
    }

    public static Task<McpToolCallResult> CallAsync(McpToolHarness harness, ClaimsPrincipal user, string toolName, object arguments = null, bool approved = false)
    {
        var options = approved ? new McpToolCallOptions { AsApprovedChange = true } : null;

        return harness.CallAsync(user, toolName, arguments, options, TestContext.Current.CancellationToken);
    }

    public static Module CreateModule(bool mcpInstalled)
    {
        var moduleService = new Mock<IModuleService>();
        moduleService.Setup(x => x.IsInstalled("VirtoCommerce.MCP")).Returns(mcpInstalled);

        return new Module
        {
            Configuration = new ConfigurationBuilder().Build(),
            ModuleService = moduleService.Object,
        };
    }

    public static ClaimsPrincipal StoreAdministrator()
    {
        return McpTestUser.WithPermissions("store-admin", RolePermissions);
    }

    // A store administrator of store A only: store:read and store:update are limited to it.
    public static ClaimsPrincipal StoreAAdministrator()
    {
        return McpTestUser.Create("store-a-admin")
            .WithPermissions("store:access")
            .WithScopedPermission("store:read", "StoreSelectedScope", StoreA)
            .WithScopedPermission("store:update", "StoreSelectedScope", StoreA);
    }

    // The part of a module whose permissions cannot be limited to stores, so catalog:read counts only when it is granted
    // globally. Its critical finding keeps every store from opening.
    public static TestCheckPart CreateCatalogPart()
    {
        return new TestCheckPart("catalog") { Permissions = new McpSetupPermissions { Read = "catalog:read" } }
            .WithFinding("catalog-no-products", McpFindingSeverities.Critical);
    }

    // Store A is closed and ready to open; store B is open and has no catalog.
    public static List<Store> CreateStores()
    {
        return
        [
            new Store
            {
                Id = StoreA,
                Name = "Store A",
                StoreState = StoreState.Closed,
                DefaultLanguage = "en-US",
                Languages = ["en-US"],
                DefaultCurrency = "USD",
                Currencies = ["USD"],
                Catalog = "catalog-a",
                Url = "https://a.example.com",
                Email = "a@example.com",
                Settings =
                [
                    new ObjectSettingEntry(TaxCalculation) { Value = true },
                    new ObjectSettingEntry(MapsApiKey) { Value = MapsApiKeyValue },
                ],
            },
            new Store
            {
                Id = StoreB,
                Name = "Store B",
                StoreState = StoreState.Open,
                DefaultLanguage = "en-US",
                Languages = ["en-US"],
                DefaultCurrency = "USD",
                Currencies = ["USD"],
                Url = "https://b.example.com",
                Email = "b@example.com",
                Settings = [],
            },
        ];
    }

    // The MCP module's pending changes, the newest first, filtered as its search filters them.
    private static IMcpPendingChangeSearchService CreatePendingChangeSearchService(List<McpPendingChange> changes)
    {
        var searchService = new Mock<IMcpPendingChangeSearchService>();
        searchService
            .Setup(x => x.SearchAsync(It.IsAny<McpPendingChangeSearchCriteria>(), It.IsAny<bool>()))
            .ReturnsAsync((McpPendingChangeSearchCriteria criteria, bool _) =>
            {
                var found = changes
                    .Where(x => criteria.ToolName == null || x.ToolName == criteria.ToolName)
                    .Where(x => criteria.Statuses == null || criteria.Statuses.Contains(x.Status))
                    .OrderByDescending(x => x.CreatedDate)
                    .ToList();

                return new McpPendingChangeSearchResult { TotalCount = found.Count, Results = found.Take(criteria.Take).ToList() };
            });

        return searchService.Object;
    }

    internal static ICountriesService CreateCountriesService()
    {
        var countriesService = new Mock<ICountriesService>();
        countriesService
            .Setup(x => x.GetCountriesAsync())
            .ReturnsAsync([new Country { Id = "USA", Name = "United States" }, new Country { Id = "DEU", Name = "Germany" }]);

        return countriesService.Object;
    }

    private static ISettingsRegistrar CreateSettingsRegistrar()
    {
        var registrar = new Mock<ISettingsRegistrar>();
        registrar.Setup(x => x.GetSettingsForType(nameof(Store))).Returns([TaxCalculation, MapsApiKey]);

        return registrar.Object;
    }

    private static ITranslationService CreateTranslationService()
    {
        var translationService = new Mock<ITranslationService>();
        translationService
            .Setup(x => x.GetTranslationDataForLanguage("en"))
            .Returns(JObject.Parse("""{ "settings": { "Stores": { "TaxCalculationEnabled": { "title": "Tax calculation" } } } }"""));

        return translationService.Object;
    }

    internal static ISettingsManager CreateSettingsManager()
    {
        var settingsManager = new Mock<ISettingsManager>();
        settingsManager
            .Setup(x => x.GetObjectSettingAsync(PlatformConstants.Settings.General.Languages.Name, null, null))
            .ReturnsAsync(new ObjectSettingEntry { Name = PlatformConstants.Settings.General.Languages.Name, AllowedValues = ["en-US", "de-DE"] });

        return settingsManager.Object;
    }

    private static ICurrencyService CreateCurrencyService()
    {
        var currencyService = new Mock<ICurrencyService>();
        currencyService.Setup(x => x.GetAllCurrenciesAsync()).ReturnsAsync([new Currency { Code = "USD", Name = "US dollar" }]);

        return currencyService.Object;
    }
}
