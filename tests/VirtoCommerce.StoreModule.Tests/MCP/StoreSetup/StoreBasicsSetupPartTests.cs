using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.MCP.Core.Services;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.Data.MCP.StoreSetup;
using Xunit;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

public class StoreBasicsSetupPartTests
{
    [Fact]
    public async Task CheckAsync_CompleteStoreWithoutFulfillmentCenters_HasNoFindings()
    {
        //Arrange
        var store = CreateCompleteStore();

        //Act
        var findings = await CreatePart().CheckAsync(new StoreSetupContext { Store = store });

        //Assert
        Assert.Null(store.MainFulfillmentCenterId);
        Assert.Empty(findings);
    }

    [Theory]
    [InlineData("no-languages", "basics-no-language", McpFindingSeverities.Critical)]
    [InlineData("default-language-not-among-them", "basics-no-language", McpFindingSeverities.Critical)]
    [InlineData("no-currencies", "basics-no-currency", McpFindingSeverities.Critical)]
    [InlineData("default-currency-not-among-them", "basics-no-currency", McpFindingSeverities.Critical)]
    [InlineData("no-storefront-url", "basics-no-storefront-url", McpFindingSeverities.Warning)]
    [InlineData("no-email", "basics-no-email", McpFindingSeverities.Warning)]
    public async Task CheckAsync_ReportsWhatTheStoreIsMissing(string gap, string expectedId, string expectedSeverity)
    {
        //Arrange
        var store = CreateCompleteStore();

        switch (gap)
        {
            case "no-languages":
                store.Languages = [];
                break;
            case "default-language-not-among-them":
                store.DefaultLanguage = "fr-FR";
                break;
            case "no-currencies":
                store.Currencies = [];
                break;
            case "default-currency-not-among-them":
                store.DefaultCurrency = "JPY";
                break;
            case "no-storefront-url":
                store.Url = null;
                store.SecureUrl = null;
                break;
            case "no-email":
                store.Email = null;
                store.AdminEmail = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(gap));
        }

        //Act
        var findings = await CreatePart().CheckAsync(new StoreSetupContext { Store = store });

        //Assert
        var finding = Assert.Single(findings);
        Assert.Equal(expectedId, finding.Id);
        Assert.Equal(expectedSeverity, finding.Severity);
        Assert.Equal("basics", finding.Category);
        Assert.Equal(store.Id, finding.ObjectId);
        Assert.False(string.IsNullOrEmpty(finding.Why));
        Assert.False(string.IsNullOrEmpty(finding.Fix));
        Assert.False(string.IsNullOrEmpty(finding.Screen));
    }

    [Theory]
    [InlineData("EUR", false)]
    [InlineData("eur", false)]
    [InlineData("JPY", true)]
    public async Task CheckAsync_WithACurrency_BlocksACurrencyTheStoreDoesNotHave(string currency, bool blocked)
    {
        //Act
        var findings = await CreatePart().CheckAsync(new StoreSetupContext { Store = CreateCompleteStore(), Currency = currency });

        //Assert
        Assert.Equal(blocked, findings.Any(x => x.Id == "currency-not-in-store" && x.Severity == McpFindingSeverities.Critical));
    }

    [Fact]
    public async Task GetChoicesAsync_ListsThePlatformLanguagesAndTheCurrencies_AsBasicsFields()
    {
        //Act
        var choices = await CreatePart().GetChoicesAsync();

        //Assert
        Assert.Equal(["en-US", "de-DE"], choices.Where(x => x.Field == "basics.languages").Select(x => x.Value));
        Assert.Equal(["USD", "EUR"], choices.Where(x => x.Field == "basics.currencies").Select(x => x.Value));
        Assert.All(choices, x => Assert.False(string.IsNullOrEmpty(x.Title)));
    }

    [Fact]
    public async Task GetStateAsync_ReturnsTheStoreInTheShapeOfItsSection()
    {
        //Act
        var state = await CreatePart().GetStateAsync(CreateCompleteStore());

        //Assert
        var section = Assert.IsType<StoreBasicsSection>(state);
        Assert.Equal("EU store", section.Name);
        Assert.Equal(["de-DE", "en-US"], section.Languages);
        Assert.Equal("EUR", section.DefaultCurrency);
    }

    // Saving the store would refuse these after the approval; the plan refuses them first.
    [Theory]
    [InlineData("cleared name", "A store needs a name: it cannot be cleared.")]
    [InlineData("long name", "name takes at most 128 characters.")]
    [InlineData("long url", "url takes at most 256 characters.")]
    public async Task PlanAsync_RefusesWhatTheStoresColumnsCannotHold(string gap, string expectedError)
    {
        //Arrange
        var section = gap switch
        {
            "cleared name" => new StoreBasicsSection { Name = "" },
            "long name" => new StoreBasicsSection { Name = new string('n', 129) },
            _ => new StoreBasicsSection { Url = $"https://{new string('u', 250)}.example.com" },
        };

        //Act
        var plan = await CreatePart().PlanAsync(new StoreSetupContext { Store = CreateCompleteStore() }, section);

        //Assert
        Assert.Contains(expectedError, plan.Errors);
    }

    // Pricing and the cart compare codes exactly, so each code takes the platform's spelling before it is planned.
    [Fact]
    public async Task PlanAsync_TakesThePlatformsSpellingOfEachCode()
    {
        //Arrange
        var context = new StoreSetupContext { Store = CreateCompleteStore() };
        var section = new StoreBasicsSection { DefaultLanguage = "en-us", Languages = ["EN-US"], DefaultCurrency = "usd", Currencies = ["Usd", "eur"], Country = " deu " };

        //Act
        var plan = await CreatePart().PlanAsync(context, section);

        //Assert
        Assert.Empty(plan.Errors);
        Assert.Equal("en-US", context.Store.DefaultLanguage);
        Assert.Equal(["en-US"], context.Store.Languages);
        Assert.Equal("USD", context.Store.DefaultCurrency);
        Assert.Equal(["USD", "EUR"], context.Store.Currencies);
        Assert.Equal("DEU", context.Store.Country);
        Assert.Contains(plan.Changes, x => x.Field == "default_currency" && Equals(x.To, "USD"));
    }

    // A code sent in another case is the code the store has, so it changes nothing; an unknown code is refused as sent.
    [Fact]
    public async Task PlanAsync_ACodeInAnotherCase_ChangesNothing_AndAnUnknownCodeIsRefusedAsSent()
    {
        //Act
        var sameCodes = await CreatePart().PlanAsync(new StoreSetupContext { Store = CreateCompleteStore() }, new StoreBasicsSection { DefaultCurrency = "eur", Currencies = ["eur"] });
        var unknown = await CreatePart().PlanAsync(new StoreSetupContext { Store = CreateCompleteStore() }, new StoreBasicsSection { Currencies = ["eur", "gbp"] });

        //Assert
        Assert.Empty(sameCodes.Errors);
        Assert.Empty(sameCodes.Changes);
        Assert.Equal(["Currency 'gbp' is not one of the platform's currencies: USD, EUR. A person adds it in the back office: Modules > Installed > Commerce core module > Currencies."], unknown.Errors);
    }

    // A new store's default that is left out is the first of its list, as a row: its catalog is created with it.
    [Fact]
    public async Task PlanAsync_ANewStore_TakesTheFirstOfEachListAsItsDefault()
    {
        //Arrange
        var context = new StoreSetupContext { IsNewStore = true, Store = new Store { Id = "new-store", Languages = [], Currencies = [], Settings = [] } };

        //Act
        var plan = await CreatePart().PlanAsync(context, new StoreBasicsSection { Name = "New store", Languages = ["de-DE", "en-US"], Currencies = ["EUR", "USD"] });

        //Assert
        Assert.Empty(plan.Errors);
        Assert.Equal(("de-DE", "EUR"), (context.Store.DefaultLanguage, context.Store.DefaultCurrency));
        Assert.Contains(plan.Changes, x => x.Field == "default_language" && Equals(x.To, "de-DE"));
        Assert.Contains(plan.Changes, x => x.Field == "default_currency" && Equals(x.To, "EUR"));
    }

    // An existing store takes the first of a list only when it has no default and the request sends the list.
    [Fact]
    public async Task PlanAsync_AnExistingStore_TakesTheFirstOfAList_OnlyWithoutADefault()
    {
        //Arrange
        var withoutDefault = CreateCompleteStore();
        withoutDefault.DefaultCurrency = null;
        var withDefault = CreateCompleteStore();
        var listNotSent = CreateCompleteStore();
        listNotSent.DefaultLanguage = null;

        //Act
        await CreatePart().PlanAsync(new StoreSetupContext { Store = withoutDefault }, new StoreBasicsSection { Currencies = ["USD", "EUR"] });
        await CreatePart().PlanAsync(new StoreSetupContext { Store = withDefault }, new StoreBasicsSection { Currencies = ["USD", "EUR"] });
        var notSent = await CreatePart().PlanAsync(new StoreSetupContext { Store = listNotSent }, new StoreBasicsSection { Email = "eu@example.com" });

        //Assert
        Assert.Equal("USD", withoutDefault.DefaultCurrency);
        Assert.Equal("EUR", withDefault.DefaultCurrency);
        Assert.Null(listNotSent.DefaultLanguage);
        Assert.DoesNotContain(notSent.Changes, x => x.Field == "default_language");
    }

    // A storefront looks a store up by its URL: a store with a secure URL only shows on no address of its own.
    [Fact]
    public async Task CheckAsync_AStoreWithASecureUrlOnly_HasNoStorefrontUrl()
    {
        //Arrange
        var store = CreateCompleteStore();
        store.Url = null;
        store.SecureUrl = "https://eu.example.com";

        //Act
        var findings = await CreatePart().CheckAsync(new StoreSetupContext { Store = store });

        //Assert
        var finding = Assert.Single(findings, x => x.Id == "basics-no-storefront-url");
        Assert.Equal("URL: none. Secure URL: https://eu.example.com, which storefronts do not look up.", finding.Evidence);
    }

    // Matched as a storefront's lookup does, both ways: a URL that holds "://<host>", whatever the port. The store shown
    // there is the first by name.
    [Theory]
    [InlineData("https://shop.com", "https://shop.com.au", "shop.com")]
    [InlineData("https://shop.com.au", "https://shop.com", "shop.com")]
    [InlineData("http://localhost:3000", "http://localhost:3001/", "localhost")]
    public async Task CheckAsync_AnotherStoreAtTheSameHost_IsAWarning_NamingTheStoreShown(string url, string otherUrl, string host)
    {
        //Arrange
        var store = CreateCompleteStore();
        store.Url = url;
        var other = new Store { Id = "a-store", Name = "A store", Url = otherUrl };

        //Act
        var findings = await CreatePart(stores: [store, other]).CheckAsync(new StoreSetupContext { Store = store });

        //Assert
        var finding = Assert.Single(findings, x => x.Id == "basics-host-shared");
        Assert.Equal((McpFindingSeverities.Warning, $"Another store answers at {host}"), (finding.Severity, finding.Title));
        Assert.Equal($"At {host}: A store, EU store. A storefront there shows A store.", finding.Evidence);
    }

    [Fact]
    public async Task CheckAsync_StoresAtDistinctHosts_HaveNoWarning()
    {
        //Arrange
        var store = CreateCompleteStore();
        var other = new Store { Id = "us-store", Name = "US store", Url = "https://us.example.com" };

        //Act
        var findings = await CreatePart(stores: [store, other]).CheckAsync(new StoreSetupContext { Store = store });

        //Assert
        Assert.DoesNotContain(findings, x => x.Id == "basics-host-shared");
    }

    // No tool adds a language to the platform: the refusal names the screen that does, with its link.
    [Fact]
    public async Task PlanAsync_AnUnknownLanguage_NamesTheScreenThatAddsIt_WithItsLink()
    {
        //Arrange
        var linkResolver = new Mock<IMcpLinkResolver>();
        linkResolver.Setup(x => x.GetLinkAsync(It.IsAny<string>())).ReturnsAsync((string route) => $"https://admin.example.com{route}");

        //Act
        var plan = await CreatePart(linkResolver: linkResolver.Object).PlanAsync(new StoreSetupContext { Store = CreateCompleteStore() }, new StoreBasicsSection { Languages = ["de-DE", "es-ES"] });

        //Assert
        Assert.Equal(["Language 'es-ES' is not one of the platform's languages: en-US, de-DE. A person adds it in the back office: " +
            "Settings > Platform > General, setting Languages (https://admin.example.com/#!/workspace/settings?setting=VirtoCommerce.Core.General.Languages)."], plan.Errors);
    }

    private static Store CreateCompleteStore()
    {
        return new Store
        {
            Id = "eu-store",
            Name = "EU store",
            DefaultLanguage = "de-DE",
            Languages = ["de-DE", "en-US"],
            DefaultCurrency = "EUR",
            Currencies = ["EUR"],
            Catalog = "b2b-catalog",
            Url = "https://eu.example.com",
            Email = "shop@example.com",
        };
    }

    // The platform's languages and countries as the tool tests have them, and two currencies.
    internal static StoreBasicsSetupPart CreatePart(IStoreService storeService = null, IList<Store> stores = null, IMcpLinkResolver linkResolver = null)
    {
        var currencyService = new Mock<ICurrencyService>();
        currencyService
            .Setup(x => x.GetAllCurrenciesAsync())
            .ReturnsAsync([new Currency { Code = "USD", Name = "US dollar" }, new Currency { Code = "EUR", Name = "Euro" }]);

        return new StoreBasicsSetupPart(
            StoreToolsTestContext.CreateSettingsManager(),
            currencyService.Object,
            StoreToolsTestContext.CreateCountriesService(),
            storeService ?? Mock.Of<IStoreService>(),
            new InMemoryStoreSearchService(stores ?? []),
            linkResolver == null ? [] : [linkResolver]);
    }
}
