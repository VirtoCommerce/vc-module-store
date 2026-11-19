using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.Data.MCP.StoreSetup;
using Xunit;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

public class StoreSetupServiceTests
{
    [Fact]
    public void GetParts_PutsStoresOwnPartsFirst_ThenTheOthersByName()
    {
        //Arrange
        var service = CreateService(new TestCheckPart("tax"), new TestCheckPart("settings"), new TestCheckPart("audit"), new TestCheckPart("basics"));

        //Act
        var names = service.GetParts().Select(x => x.Name).ToList();

        //Assert
        Assert.Equal(["basics", "settings", "audit", "tax"], names);
    }

    // A module overrides a part by registering its subclass after the original: the last part of a name is the part.
    [Fact]
    public async Task GetParts_KeepsTheLastPartOfAName_WhateverItsCase()
    {
        //Arrange
        var original = new TestCheckPart("payment").WithFinding("payment-original", McpFindingSeverities.Warning);
        var replacement = new TestCheckPart("Payment").WithFinding("payment-replacement", McpFindingSeverities.Warning);
        var service = CreateService(original, replacement);

        //Act
        var parts = service.GetParts();
        var findings = await service.CheckAsync(TestCheckPart.CreateStore());

        //Assert
        Assert.Same(replacement, Assert.Single(parts));
        Assert.Equal(["payment-replacement"], findings.Select(x => x.Id));
    }

    [Fact]
    public async Task CheckAsync_CollectsTheFindingsOfEveryPartThatChecks()
    {
        //Arrange
        var basics = new TestCheckPart("basics").WithFinding("basics-no-catalog", McpFindingSeverities.Warning);
        var catalog = new TestCheckPart("catalog").WithFinding("catalog-empty", McpFindingSeverities.Critical);

        //Act
        var findings = await CreateService(basics, catalog).CheckAsync(TestCheckPart.CreateStore(), "EUR", "DEU");

        //Assert
        Assert.Equal(["basics-no-catalog", "catalog-empty"], findings.Select(x => x.Id));
        Assert.Equal("EUR", catalog.CheckedContext.Currency);
        Assert.Equal("DEU", catalog.CheckedContext.Country);
        Assert.Empty(catalog.CheckedContext.Plans);
    }

    [Fact]
    public async Task CheckAsync_PartThatFails_GivesABlockingFinding_WithoutTheErrorText()
    {
        //Arrange
        var failing = new TestCheckPart("payment") { Error = new InvalidOperationException("Gateway: invalid key sk_live_123 at Stripe.Charge()") };
        var working = new TestCheckPart("shipping").WithFinding("shipping-no-active-method", McpFindingSeverities.Critical);

        //Act
        var findings = await CreateService(failing, working).CheckAsync(TestCheckPart.CreateStore());

        //Assert
        var notChecked = Assert.Single(findings, x => x.Id == "payment-not-checked");
        Assert.Equal(McpFindingSeverities.Critical, notChecked.Severity);
        Assert.Equal("payment", notChecked.Category);
        Assert.Contains(findings, x => x.Id == "shipping-no-active-method");
        Assert.DoesNotContain("sk_live_123", JsonSerializer.Serialize(findings));
        Assert.DoesNotContain("Stripe", JsonSerializer.Serialize(findings));
    }

    [Fact]
    public async Task CheckAsync_GivesThePartsACopyOfTheStore()
    {
        //Arrange
        var part = new TestCheckPart("basics");
        var store = TestCheckPart.CreateStore();

        //Act
        await CreateService(part).CheckAsync(store);

        //Assert
        Assert.Equal(store.Id, part.CheckedContext.Store.Id);
        Assert.NotSame(store, part.CheckedContext.Store);
    }

    // The store a caller holds may share its lists with the cached store: the parts get copies they may change in place.
    [Fact]
    public async Task CheckAsync_GivesThePartsCopiesOfTheStoresLists()
    {
        //Arrange
        var store = TestCheckPart.CreateStore();
        store.Languages = ["en-US"];
        store.Currencies = ["USD"];

        //Act
        await CreateService(new TestListChangingPart()).CheckAsync(store);

        //Assert
        Assert.Equal(["en-US"], store.Languages);
        Assert.Equal(["USD"], store.Currencies);
    }

    [Theory]
    [InlineData(McpFindingSeverities.Critical, true)]
    [InlineData("Critical", true)]
    [InlineData(McpFindingSeverities.Warning, false)]
    [InlineData(McpFindingSeverities.Info, false)]
    [InlineData(null, false)]
    public void BlocksGoLive_ForACriticalFinding_WhateverTheCase(string severity, bool expected)
    {
        Assert.Equal(expected, CreateService().BlocksGoLive(new McpFinding { Severity = severity }));
    }

    private static StoreSetupService CreateService(params IStoreSetupPart[] parts)
    {
        return new StoreSetupService(parts, Mock.Of<IStoreService>(), NullLogger<StoreSetupService>.Instance);
    }
}
