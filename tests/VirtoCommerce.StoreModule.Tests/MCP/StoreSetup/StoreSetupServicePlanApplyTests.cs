using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Data.MCP.StoreSetup;
using Xunit;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

public class StoreSetupServicePlanApplyTests
{
    private const string StoreId = "eu-store";

    [Fact]
    public async Task PlanAsync_RefusesASectionNoPartTakes_AndAPartThatTakesNone()
    {
        //Arrange
        var service = CreateService([CreateStore()], new TestApplyPart("shipping"), new TestCheckPart("catalog"));

        //Act
        var result = await service.PlanAsync(CreateRequest(("tax", new TestSection()), ("catalog", new TestSection())));

        //Assert
        Assert.Contains(result.Errors, x => x.Contains("'tax'") && x.Contains("get_store_setup_options"));
        Assert.Contains(result.Errors, x => x.Contains("'catalog' part takes no section"));
    }

    [Fact]
    public async Task PlanAsync_NewStore_NeedsTheSectionsRequiredForANewStore()
    {
        //Arrange
        var service = CreateService([], new TestApplyPart("basics") { IsRequiredForNewStore = true }, new TestApplyPart("shipping"));
        var request = CreateRequest(("shipping", new TestSection { Methods = ["FixedRate"] }));
        request.IsNewStore = true;

        //Act
        var result = await service.PlanAsync(request);

        //Assert
        Assert.Equal(["A new store needs the 'basics' section."], result.Errors);
    }

    [Fact]
    public async Task PlanAsync_ChangesNothing_AndSaysWhatTheStoreWouldMiss()
    {
        //Arrange
        var stores = new List<Store> { CreateStore() };
        var storeService = TestStoreService.Create(stores);
        var shipping = new TestApplyPart("shipping");
        var catalog = new TestCheckPart("catalog").WithFinding("catalog-no-products", McpFindingSeverities.Critical);
        var service = new StoreSetupService([shipping, catalog], storeService.Object, new StoreSetupFindings(), NullLogger<StoreSetupService>.Instance);

        //Act
        var result = await service.PlanAsync(CreateRequest(("shipping", new TestSection { Methods = ["FixedRate"] })));

        //Assert
        Assert.Empty(result.Errors);
        var part = Assert.Single(result.Parts);
        Assert.Equal(McpSetupPartStatus.WillChange, part.Status);
        Assert.Equal("catalog-no-products", Assert.Single(result.Findings).Id);
        Assert.Equal(0, shipping.ApplyCount);
        storeService.Verify(x => x.SaveChangesAsync(It.IsAny<IList<Store>>()), Times.Never);
    }

    [Fact]
    public async Task ApplyAsync_AnErrorInAnyPart_AppliesNoPart()
    {
        //Arrange
        var shipping = new TestApplyPart("shipping");
        var tax = new TestApplyPart("tax") { PlanError = "Provider 'X' is not installed." };
        var service = CreateService([CreateStore()], shipping, tax);

        //Act
        var result = await service.ApplyAsync(CreateRequest(("shipping", new TestSection { Methods = ["FixedRate"] }), ("tax", new TestSection { Methods = ["X"] })));

        //Assert
        Assert.Equal(["tax: Provider 'X' is not installed."], result.Errors);
        Assert.Equal(0, shipping.ApplyCount);
        Assert.Equal(0, tax.ApplyCount);
    }

    [Fact]
    public async Task ApplyAsync_StopsAtTheFirstFailure_AndSaysWhichPartsWereApplied()
    {
        //Arrange
        var payment = new TestApplyPart("payment");
        var shipping = new TestApplyPart("shipping") { ApplyError = new InvalidOperationException("Gateway key sk_live_123 rejected") };
        var tax = new TestApplyPart("tax");
        var service = CreateService([CreateStore()], payment, shipping, tax);

        //Act
        var result = await service.ApplyAsync(CreateFullRequest());

        //Assert
        Assert.Equal([McpSetupPartStatus.Applied, McpSetupPartStatus.Failed, McpSetupPartStatus.NotApplied], result.Parts.Select(x => x.Status));
        Assert.Equal(0, tax.ApplyCount);
        Assert.DoesNotContain("sk_live_123", JsonSerializer.Serialize(result.Parts));
    }

    [Fact]
    public async Task ApplyAsync_RepeatedAfterAFailure_AppliesOnlyWhatIsLeft()
    {
        //Arrange
        var payment = new TestApplyPart("payment");
        var shipping = new TestApplyPart("shipping") { ApplyError = new InvalidOperationException("Timeout") };
        var tax = new TestApplyPart("tax");
        var service = CreateService([CreateStore()], payment, shipping, tax);
        await service.ApplyAsync(CreateFullRequest());
        shipping.ApplyError = null;

        //Act
        var result = await service.ApplyAsync(CreateFullRequest());

        //Assert
        Assert.Equal([McpSetupPartStatus.NoChange, McpSetupPartStatus.Applied, McpSetupPartStatus.Applied], result.Parts.Select(x => x.Status));
        Assert.Equal(1, payment.ApplyCount);
        Assert.Equal(["FixedRate"], shipping.Saved[StoreId]);
        Assert.Equal(1, tax.ApplyCount);
    }

    private static StoreSetupRequest CreateFullRequest()
    {
        return CreateRequest(
            ("payment", new TestSection { Methods = ["Manual"] }),
            ("shipping", new TestSection { Methods = ["FixedRate"] }),
            ("tax", new TestSection { Methods = ["FixedTax"] }));
    }

    // The store table refuses texts longer than their columns, and the test store service refuses them as it does: the
    // plan refuses them first, so an approved apply never fails on them, and nothing is saved.
    [Fact]
    public async Task ApplyAsync_TextsTheStoreTableRefuses_AreRefusedBeforeAnythingIsSaved()
    {
        //Arrange
        var stores = new List<Store> { CreateStore() };
        var storeService = TestStoreService.Create(stores);
        var service = new StoreSetupService([StoreBasicsSetupPartTests.CreatePart(storeService.Object)], storeService.Object, new StoreSetupFindings(), NullLogger<StoreSetupService>.Instance);
        var section = new StoreBasicsSection { Name = new string('n', 129), Description = new string('d', 257), Region = new string('r', 129) };

        //Act
        var result = await service.ApplyAsync(CreateRequest(("basics", section)));

        //Assert
        Assert.Equal(["basics: name takes at most 128 characters.", "basics: description takes at most 256 characters.", "basics: region takes at most 128 characters."], result.Errors);
        Assert.Equal("EU store", Assert.Single(stores).Name);
        storeService.Verify(x => x.SaveChangesAsync(It.IsAny<IList<Store>>()), Times.Never);
    }

    // A store without a default currency takes the first of the currencies the request sends, as its plan shows, and the
    // apply saves it too.
    [Fact]
    public async Task ApplyAsync_AStoreWithoutADefault_SavesTheFirstOfTheListItIsSent()
    {
        //Arrange
        var store = CreateStore();
        store.Currencies = ["EUR"];
        var stores = new List<Store> { store };
        var storeService = TestStoreService.Create(stores);
        var service = new StoreSetupService([StoreBasicsSetupPartTests.CreatePart(storeService.Object)], storeService.Object, new StoreSetupFindings(), NullLogger<StoreSetupService>.Instance);

        //Act
        var result = await service.ApplyAsync(CreateRequest(("basics", new StoreBasicsSection { Currencies = ["USD", "EUR"] })));

        //Assert
        Assert.Empty(result.Errors);
        Assert.Contains(Assert.Single(result.Parts).Changes, x => x.Field == "default_currency" && Equals(x.To, "USD"));
        Assert.Equal("USD", Assert.Single(stores).DefaultCurrency);
    }

    // A part whose plan fails is named without the exception's text, which may hold a third party's trace or a secret.
    [Fact]
    public async Task PlanAsync_APartWhosePlanFails_IsRefused_WithoutTheExceptionsText()
    {
        //Arrange
        var service = CreateService([CreateStore()], new TestFailingPlanPart("payment"));

        //Act
        var result = await service.PlanAsync(CreateRequest(("payment", new TestSection())));

        //Assert
        Assert.Equal(["payment: Test part payment could not be planned. Review the store in the back office, or try again later."], result.Errors);
        Assert.DoesNotContain(result.Errors, x => x.Contains(TestFailingPlanPart.ExceptionMessage));
    }

    [Fact]
    public async Task PlanAsync_ANullSection_IsRefused()
    {
        //Arrange
        var service = CreateService([CreateStore()], new TestApplyPart("shipping"));

        //Act
        var result = await service.PlanAsync(CreateRequest(("shipping", null)));

        //Assert
        Assert.Equal(["shipping: the section is empty or does not match its schema. get_store_setup_options shows it."], result.Errors);
    }

    // A caller's own dictionary may tell names by their case: neither of two sections of one part would be the one applied.
    [Fact]
    public async Task PlanAsync_ASectionSentTwiceInTwoCases_IsRefused()
    {
        //Arrange
        var service = CreateService([CreateStore()], new TestApplyPart("shipping"));
        var request = new StoreSetupRequest
        {
            StoreId = StoreId,
            Sections = new Dictionary<string, object>(StringComparer.Ordinal) { ["shipping"] = new TestSection(), ["Shipping"] = new TestSection() },
        };

        //Act
        var result = await service.PlanAsync(request);

        //Assert
        Assert.Contains("The 'Shipping' section is sent twice, in two cases of its name. Send it once.", result.Errors);
    }

    // The tools check the permissions a part's plan names, so the result carries them.
    [Fact]
    public async Task PlanAsync_CarriesThePermissionsAPartsPlanNames()
    {
        //Arrange
        var service = CreateService([CreateStore()], new TestPlannedPermissionsPart("catalog", "catalog:read", "catalog:update"), new TestApplyPart("shipping"));

        //Act
        var result = await service.PlanAsync(CreateRequest(("catalog", new TestCatalogSection { Name = "EU catalog" }), ("shipping", new TestSection { Methods = ["FixedRate"] })));

        //Assert
        Assert.Equal(["catalog:read", "catalog:update"], result.Parts.Single(x => x.Part == "catalog").RequiredPermissions);
        Assert.Null(result.Parts.Single(x => x.Part == "shipping").RequiredPermissions);
    }

    // The service reads the cached store: the parts plan on a copy of it, so a plan leaves the cached store as it is.
    [Fact]
    public async Task PlanAsync_LeavesTheCachedStoreAsItIs()
    {
        //Arrange
        var store = CreateStore();
        var stores = new List<Store> { store };
        var storeService = TestStoreService.Create(stores);
        var service = new StoreSetupService([StoreBasicsSetupPartTests.CreatePart(storeService.Object)], storeService.Object, new StoreSetupFindings(), NullLogger<StoreSetupService>.Instance);

        //Act
        var result = await service.PlanAsync(CreateRequest(("basics", new StoreBasicsSection { Name = "Renamed", Currencies = ["USD"] })));

        //Assert
        Assert.Empty(result.Errors);
        Assert.Equal("Renamed", result.Store.Name);
        Assert.Same(store, Assert.Single(stores));
        Assert.Equal("EU store", store.Name);
        Assert.Null(store.Currencies);
    }

    // The store table refuses a longer id and longer links to fulfillment centers, so the plan refuses them first.
    [Fact]
    public async Task PlanAsync_ANewStoreWithAnIdOrALinkLongerThanItsColumn_IsRefused()
    {
        //Arrange
        var service = new StoreSetupService([StoreBasicsSetupPartTests.CreatePart()], TestStoreService.Create([]).Object, new StoreSetupFindings(), NullLogger<StoreSetupService>.Instance);
        var request = new StoreSetupRequest { StoreId = new string('s', 129), IsNewStore = true };
        request.Sections["basics"] = new StoreBasicsSection { Name = "New store", AdditionalFulfillmentCenterIds = ["fc-1", new string('f', 129)] };

        //Act
        var result = await service.PlanAsync(request);

        //Assert
        Assert.Contains("basics: store_id takes at most 128 characters.", result.Errors);
        Assert.Contains("basics: Each of additional_fulfillment_center_ids takes at most 128 characters.", result.Errors);
    }

    private static StoreSetupRequest CreateRequest(params (string Name, object Section)[] sections)
    {
        var request = new StoreSetupRequest { StoreId = StoreId };

        foreach (var (name, section) in sections)
        {
            request.Sections[name] = section;
        }

        return request;
    }

    private static Store CreateStore()
    {
        return new Store { Id = StoreId, Name = "EU store", Catalog = "eu-catalog", StoreState = StoreState.Closed, Settings = [] };
    }

    private static StoreSetupService CreateService(List<Store> stores, params IStoreSetupPart[] parts)
    {
        return new StoreSetupService(parts, TestStoreService.Create(stores).Object, new StoreSetupFindings(), NullLogger<StoreSetupService>.Instance);
    }
}
