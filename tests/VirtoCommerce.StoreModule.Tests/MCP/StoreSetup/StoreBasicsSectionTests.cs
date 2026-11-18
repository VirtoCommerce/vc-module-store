using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using Xunit;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

public class StoreBasicsSectionTests
{
    [Fact]
    public async Task PlanAsync_NewStore_ListsEveryFieldItSets()
    {
        //Arrange
        var context = CreateNewStoreContext("eu-store");
        var section = new StoreBasicsSection
        {
            Name = "EU store",
            DefaultLanguage = "de-DE",
            Languages = ["de-DE"],
            DefaultCurrency = "EUR",
            Currencies = ["EUR"],
            Country = "DEU",
        };

        //Act
        var plan = await StoreBasicsSetupPartTests.CreatePart().PlanAsync(context, section);

        //Assert
        Assert.Empty(plan.Errors);
        Assert.Equal(["name", "default_language", "languages", "default_currency", "currencies", "country"], plan.Changes.Select(x => x.Field));
        Assert.Equal("EU store", context.Store.Name);
        Assert.Equal(["EUR"], context.Store.Currencies);
    }

    [Theory]
    [InlineData("store-id", "letters, digits")]
    [InlineData("name", "needs a name")]
    [InlineData("language", "Language 'xx-XX' is not one of the platform's languages")]
    [InlineData("default-language", "default language 'de-DE' is not one of the store's languages")]
    [InlineData("currency", "Currency 'ABC' is not one of the platform's currencies")]
    [InlineData("country", "Country 'Narnia' is not one of the platform's countries")]
    [InlineData("url", "is not an absolute http or https address")]
    [InlineData("email", "is not an email address")]
    public async Task PlanAsync_RefusesWhatTheStoreCannotTake(string gap, string expectedError)
    {
        //Arrange
        var context = CreateNewStoreContext(gap == "store-id" ? "eu store" : "eu-store");
        var section = new StoreBasicsSection { Name = gap == "name" ? null : "EU store" };

        switch (gap)
        {
            case "store-id":
            case "name":
                break;
            case "language":
                section.Languages = ["xx-XX"];
                break;
            case "default-language":
                section.Languages = ["en-US"];
                section.DefaultLanguage = "de-DE";
                break;
            case "currency":
                section.Currencies = ["ABC"];
                break;
            case "country":
                section.Country = "Narnia";
                break;
            case "url":
                section.Url = "eu.example.com";
                break;
            case "email":
                section.Email = "shop";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(gap));
        }

        //Act
        var plan = await StoreBasicsSetupPartTests.CreatePart().PlanAsync(context, section);

        //Assert
        Assert.Contains(plan.Errors, x => x.Contains(expectedError));
    }

    [Fact]
    public async Task PlanAsync_ExistingStore_ChangesOnlyTheFieldsTheSectionNames()
    {
        //Arrange
        var store = new Store
        {
            Id = "eu-store",
            Name = "EU store",
            Catalog = "b2b-catalog",
            DefaultLanguage = "de-DE",
            Languages = ["de-DE", "en-US"],
            Email = "shop@example.com",
        };
        var context = new StoreSetupContext { Store = store };

        //Act
        var plan = await StoreBasicsSetupPartTests.CreatePart().PlanAsync(context, new StoreBasicsSection { Languages = ["de-DE"], CatalogId = "" });

        //Assert
        Assert.Empty(plan.Errors);
        Assert.Equal(["languages", "catalog_id"], plan.Changes.Select(x => x.Field));
        Assert.Contains("Removes from languages: en-US.", plan.Warnings);
        Assert.Null(store.Catalog);
        Assert.Equal("shop@example.com", store.Email);
    }

    [Fact]
    public async Task ApplyAsync_ExistingStore_AppliesTheSectionToTheSavedStore()
    {
        //Arrange
        var stores = new List<Store> { new() { Id = "eu-store", Name = "EU store", Email = "old@example.com", Description = "Before" } };
        var part = StoreBasicsSetupPartTests.CreatePart(TestStoreService.Create(stores).Object);
        var context = new StoreSetupContext { Store = stores[0].CloneTyped() };
        var plan = await part.PlanAsync(context, new StoreBasicsSection { Email = "shop@example.com" });

        // A person changes the description in the back office between the plan and the apply.
        stores[0].Description = "Changed in the back office";

        //Act
        await part.ApplyAsync(context, plan);

        //Assert
        var saved = Assert.Single(stores);
        Assert.Equal("shop@example.com", saved.Email);
        Assert.Equal("Changed in the back office", saved.Description);
    }

    [Fact]
    public async Task ApplyAsync_NewStore_SavesThePlannedStore_WithoutItsSettings()
    {
        //Arrange
        var stores = new List<Store>();
        var part = StoreBasicsSetupPartTests.CreatePart(TestStoreService.Create(stores).Object);
        var context = CreateNewStoreContext("eu-store");
        context.Store.Settings.Add(new ObjectSettingEntry { Name = "Stores.TaxCalculationEnabled", Value = true });
        var plan = await part.PlanAsync(context, new StoreBasicsSection { Name = "EU store" });

        //Act
        await part.ApplyAsync(context, plan);

        //Assert
        var saved = Assert.Single(stores);
        Assert.Equal("EU store", saved.Name);
        Assert.Equal(StoreState.Closed, saved.StoreState);
        Assert.Empty(saved.Settings);
    }

    private static StoreSetupContext CreateNewStoreContext(string storeId)
    {
        return new StoreSetupContext
        {
            IsNewStore = true,
            Store = new Store { Id = storeId, StoreState = StoreState.Closed, Languages = [], Currencies = [], Settings = [] },
        };
    }
}
