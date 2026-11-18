using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
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
    [InlineData("no-languages", "basics-no-language", StoreSetupFindingSeverity.Blocking)]
    [InlineData("default-language-not-among-them", "basics-no-language", StoreSetupFindingSeverity.Blocking)]
    [InlineData("no-currencies", "basics-no-currency", StoreSetupFindingSeverity.Blocking)]
    [InlineData("default-currency-not-among-them", "basics-no-currency", StoreSetupFindingSeverity.Blocking)]
    [InlineData("no-catalog", "basics-no-catalog", StoreSetupFindingSeverity.Warning)]
    [InlineData("no-storefront-url", "basics-no-storefront-url", StoreSetupFindingSeverity.Warning)]
    [InlineData("no-email", "basics-no-email", StoreSetupFindingSeverity.Warning)]
    public async Task CheckAsync_ReportsWhatTheStoreIsMissing(string gap, string expectedId, StoreSetupFindingSeverity expectedSeverity)
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
            case "no-catalog":
                store.Catalog = null;
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
        Assert.Equal("basics", finding.Part);
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
        Assert.Equal(blocked, findings.Any(x => x.Id == "currency-not-in-store" && x.Severity == StoreSetupFindingSeverity.Blocking));
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
        Assert.Equal("b2b-catalog", section.CatalogId);
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

    private static StoreBasicsSetupPart CreatePart()
    {
        var settingsManager = new Mock<ISettingsManager>();
        settingsManager
            .Setup(x => x.GetObjectSettingAsync(PlatformConstants.Settings.General.Languages.Name, null, null))
            .ReturnsAsync(new ObjectSettingEntry { Name = PlatformConstants.Settings.General.Languages.Name, AllowedValues = ["en-US", "de-DE"] });

        var currencyService = new Mock<ICurrencyService>();
        currencyService
            .Setup(x => x.GetAllCurrenciesAsync())
            .ReturnsAsync([new Currency { Code = "USD", Name = "US dollar" }, new Currency { Code = "EUR", Name = "Euro" }]);

        return new StoreBasicsSetupPart(settingsManager.Object, currencyService.Object);
    }
}
