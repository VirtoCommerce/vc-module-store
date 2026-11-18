using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
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

    [Fact]
    public async Task CheckAsync_CollectsTheFindingsOfEveryPartThatChecks()
    {
        //Arrange
        var basics = new TestCheckPart("basics") { Findings = [TestCheckPart.CreateFinding("basics-no-catalog", "basics", StoreSetupFindingSeverity.Warning)] };
        var catalog = new TestCheckPart("catalog") { Findings = [TestCheckPart.CreateFinding("catalog-empty", "catalog", StoreSetupFindingSeverity.Blocking)] };

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
        var working = new TestCheckPart("shipping") { Findings = [TestCheckPart.CreateFinding("shipping-no-active-method", "shipping", StoreSetupFindingSeverity.Blocking)] };

        //Act
        var findings = await CreateService(failing, working).CheckAsync(TestCheckPart.CreateStore());

        //Assert
        var notChecked = Assert.Single(findings, x => x.Id == "payment-not-checked");
        Assert.Equal(StoreSetupFindingSeverity.Blocking, notChecked.Severity);
        Assert.Equal("payment", notChecked.Part);
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

    private static StoreSetupService CreateService(params IStoreSetupPart[] parts)
    {
        return new StoreSetupService(parts, Mock.Of<IStoreService>(), NullLogger<StoreSetupService>.Instance);
    }
}
