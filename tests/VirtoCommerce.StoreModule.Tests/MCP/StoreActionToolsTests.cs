using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.MCP.Testing;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;
using Xunit;
using static VirtoCommerce.StoreModule.Tests.MCP.StoreToolsTestContext;

namespace VirtoCommerce.StoreModule.Tests.MCP;

public class StoreActionToolsTests
{
    private const string NewStoreId = "eu-store";

    [Fact]
    public async Task CreateConfiguredStore_DryRun_ReturnsThePreviewOfEveryPart_AndSavesNothing()
    {
        //Arrange
        var data = new StoreToolsTestData();
        await using var harness = CreateHarness(data);

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "create_configured_store", new { store_id = NewStoreId, setup = CompleteSetup(), dryRun = true });

        //Assert
        var json = result.EnsureSucceeded().Json;
        Assert.True(json.GetProperty("dry_run").GetBoolean());
        Assert.Equal("create", json.GetProperty("action").GetString());
        var fields = json.GetProperty("changes").EnumerateArray().Select(x => x.GetProperty("field").GetString()).ToList();
        Assert.Contains("basics: name", fields);
        Assert.Contains("settings: Stores.TaxCalculationEnabled", fields);
        Assert.DoesNotContain(data.Stores, x => x.Id == NewStoreId);
    }

    [Fact]
    public async Task CreateConfiguredStore_WithoutDryRun_WaitsForApproval_AndSavesNothing()
    {
        //Arrange
        var data = new StoreToolsTestData();
        await using var harness = CreateHarness(data);

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "create_configured_store", new { store_id = NewStoreId, setup = CompleteSetup() });

        //Assert
        result.EnsureOutcome(McpToolCallOutcome.PendingApproval);
        Assert.Equal("create", result.Json.GetProperty("action").GetString());
        Assert.DoesNotContain(data.Stores, x => x.Id == NewStoreId);
    }

    [Fact]
    public async Task CreateConfiguredStore_Approved_CreatesTheStoreClosed_WithEveryPart()
    {
        //Arrange
        var data = new StoreToolsTestData();
        var shipping = new TestApplyPart("shipping");
        await using var harness = CreateHarness(data, shipping);
        var setup = CompleteSetup();
        setup["shipping"] = new { methods = new[] { "FixedRate" } };

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "create_configured_store", new { store_id = NewStoreId, setup }, approved: true);

        //Assert
        var json = result.EnsureSucceeded().Json;
        Assert.False(json.GetProperty("dry_run").GetBoolean());
        Assert.Equal(["applied", "applied", "applied"], json.GetProperty("parts").EnumerateArray().Select(x => x.GetProperty("status").GetString()));
        var store = Assert.Single(data.Stores, x => x.Id == NewStoreId);
        Assert.Equal(StoreState.Closed, store.StoreState);
        Assert.Equal("EU store", store.Name);
        Assert.Equal(["USD"], store.Currencies);
        Assert.Equal(true, store.Settings.Single(x => x.Name == TaxCalculation.Name).Value);
        Assert.Equal(["FixedRate"], shipping.Saved[NewStoreId]);
    }

    [Fact]
    public async Task CreateConfiguredStore_WithBasicsOnly_IsAccepted_AndWarnsThatTheStoreWillNotBeReady()
    {
        //Arrange
        await using var harness = CreateHarness();
        var setup = new Dictionary<string, object> { ["basics"] = new { name = "EU store" } };

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "create_configured_store", new { store_id = NewStoreId, setup, dryRun = true });

        //Assert
        var warnings = GetWarnings(result.EnsureSucceeded());
        Assert.Contains(warnings, x => x.StartsWith("The store will not be ready to open:") && x.Contains("has no language"));
    }

    [Fact]
    public async Task CreateConfiguredStore_WithoutAName_IsRefused_AndNamesWhatIsMissing()
    {
        //Arrange
        var data = new StoreToolsTestData();
        await using var harness = CreateHarness(data);
        var setup = new Dictionary<string, object> { ["basics"] = new { email = "eu@example.com" } };

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "create_configured_store", new { store_id = NewStoreId, setup });

        //Assert
        result.EnsureOutcome(McpToolCallOutcome.Failed);
        Assert.Contains("basics: A new store needs a name.", result.Error);
        Assert.DoesNotContain(data.Stores, x => x.Id == NewStoreId);
    }

    [Fact]
    public async Task CreateConfiguredStore_WithAFieldTheSectionDoesNotHave_IsRefused()
    {
        //Arrange
        await using var harness = CreateHarness();
        var setup = new Dictionary<string, object> { ["basics"] = new { name = "EU store", catalog = "catalog-eu" } };

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "create_configured_store", new { store_id = NewStoreId, setup, dryRun = true });

        //Assert
        result.EnsureOutcome(McpToolCallOutcome.Failed);
        Assert.Contains("basics: the section does not match its schema at $.catalog", result.Error);
    }

    [Fact]
    public async Task CreateConfiguredStore_OnAStoreThatExists_IsRefused_UnlessAFailedCreateLeftItClosed()
    {
        //Arrange
        var data = new StoreToolsTestData();
        await using var harness = CreateHarness(data);
        var arguments = new { store_id = StoreA, setup = new Dictionary<string, object> { ["basics"] = new { name = "Store A", email = "new@example.com" } }, dryRun = true };

        //Act
        var madeByHand = await CallAsync(harness, StoreAdministrator(), "create_configured_store", arguments);
        data.PendingChanges.Add(CreateChange("change-1", StoreA, McpPendingChangeStatus.Failed, minutesAgo: 5));
        var leftByAFailedCreate = await CallAsync(harness, StoreAdministrator(), "create_configured_store", arguments);

        //Assert
        madeByHand.EnsureOutcome(McpToolCallOutcome.Failed);
        Assert.Contains("already exists. Change its setup with apply_store_setup", madeByHand.Error);
        Assert.Contains(GetWarnings(leftByAFailedCreate.EnsureSucceeded()), x => x.Contains("failed change change-1"));
    }

    // Store B is open: it was opened since the failed create. Store A is closed, but a later create of it succeeded.
    [Theory]
    [InlineData(StoreB, false)]
    [InlineData(StoreA, true)]
    public async Task CreateConfiguredStore_OnAStoreAFailedCreateLeft_IsRefused_WhenTheStoreIsNoLongerALeftover(string storeId, bool laterCreateSucceeded)
    {
        //Arrange
        var data = new StoreToolsTestData();
        data.PendingChanges.Add(CreateChange("change-1", storeId, McpPendingChangeStatus.Failed, minutesAgo: 10));

        if (laterCreateSucceeded)
        {
            data.PendingChanges.Add(CreateChange("change-2", storeId, McpPendingChangeStatus.Applied, minutesAgo: 5));
        }

        await using var harness = CreateHarness(data);
        var setup = new Dictionary<string, object> { ["basics"] = new { email = "new@example.com" } };

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "create_configured_store", new { store_id = storeId, setup, dryRun = true });

        //Assert
        result.EnsureOutcome(McpToolCallOutcome.Failed);
        Assert.Contains("already exists", result.Error);
    }

    [Fact]
    public async Task ApplyStoreSetup_OnAStoreTheUserDoesNotManage_IsForbidden()
    {
        //Arrange
        await using var harness = CreateHarness();
        var setup = new Dictionary<string, object> { ["basics"] = new { email = "new@example.com" } };

        //Act
        var own = await CallAsync(harness, StoreAAdministrator(), "apply_store_setup", new { store_id = StoreA, setup, dryRun = true });
        var other = await CallAsync(harness, StoreAAdministrator(), "apply_store_setup", new { store_id = StoreB, setup, dryRun = true });

        //Assert
        own.EnsureSucceeded();
        other.EnsureOutcome(McpToolCallOutcome.Forbidden);
    }

    [Fact]
    public async Task ApplyStoreSetup_WithTheSectionOfAPartTheUserMayNotChange_IsForbidden()
    {
        //Arrange
        var shipping = new TestApplyPart("shipping")
        {
            Permissions = new McpSetupPermissions { Read = "shipping:read", Update = "shipping:update", Create = "shipping:update", ScopeType = "SelectedStoreScope" },
        };
        await using var harness = CreateHarness(shipping);
        var setup = new Dictionary<string, object> { ["shipping"] = new { methods = new[] { "FixedRate" } } };

        //Act
        var result = await CallAsync(harness, StoreAAdministrator(), "apply_store_setup", new { store_id = StoreA, setup, dryRun = true });

        //Assert
        result.EnsureOutcome(McpToolCallOutcome.Forbidden);
        Assert.Contains("shipping:update", result.Error);
    }

    [Fact]
    public async Task ApplyStoreSetup_ThatChangesNothing_IsNotSubmitted()
    {
        //Arrange
        await using var harness = CreateHarness();
        var setup = new Dictionary<string, object> { ["basics"] = new { name = "Store A" } };

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "apply_store_setup", new { store_id = StoreA, setup });

        //Assert
        result.EnsureOutcome(McpToolCallOutcome.Failed);
        Assert.Contains("Nothing to change", result.Error);
    }

    [Fact]
    public async Task ApplyStoreSetup_FailingHalfWay_SaysWhichPartsWereApplied_WithoutTheError()
    {
        //Arrange
        var payment = new TestApplyPart("payment");
        var shipping = new TestApplyPart("shipping") { ApplyError = new InvalidOperationException("Carrier key sk_live_123 rejected") };
        var data = new StoreToolsTestData();
        await using var harness = CreateHarness(data, payment, shipping);
        var setup = new Dictionary<string, object>
        {
            ["basics"] = new { email = "new@example.com" },
            ["payment"] = new { methods = new[] { "Manual" } },
            ["shipping"] = new { methods = new[] { "FixedRate" } },
        };

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "apply_store_setup", new { store_id = StoreA, setup }, approved: true);

        //Assert
        result.EnsureOutcome(McpToolCallOutcome.Failed);
        Assert.Contains("Applied: basics, payment.", result.Error);
        Assert.Contains("shipping (failed:", result.Error);
        Assert.Contains("Repeat the same call to finish it", result.Error);
        Assert.DoesNotContain("sk_live_123", result.Error);
        Assert.Equal("new@example.com", data.Stores.Single(x => x.Id == StoreA).Email);
    }

    [Fact]
    public async Task OpenStore_WithBlockingFindings_IsRefused_AndNamesThem()
    {
        //Arrange
        var data = new StoreToolsTestData();
        data.Stores.Single(x => x.Id == StoreA).Currencies = [];
        await using var harness = CreateHarness(data);

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "open_store", new { store_id = StoreA });

        //Assert
        result.EnsureOutcome(McpToolCallOutcome.Failed);
        Assert.Contains("The store has no currency", result.Error);
        Assert.Equal(StoreState.Closed, data.Stores.Single(x => x.Id == StoreA).StoreState);
    }

    [Fact]
    public async Task OpenStore_ReadyStore_WaitsForApproval_AndOpensWhenApproved()
    {
        //Arrange
        var data = new StoreToolsTestData();
        await using var harness = CreateHarness(data);

        //Act
        var pending = await CallAsync(harness, StoreAdministrator(), "open_store", new { store_id = StoreA });
        var stateWhilePending = data.Stores.Single(x => x.Id == StoreA).StoreState;
        var approved = await CallAsync(harness, StoreAdministrator(), "open_store", new { store_id = StoreA }, approved: true);

        //Assert
        pending.EnsureOutcome(McpToolCallOutcome.PendingApproval);
        Assert.Equal(StoreState.Closed, stateWhilePending);
        approved.EnsureSucceeded();
        Assert.Equal(StoreState.Open, data.Stores.Single(x => x.Id == StoreA).StoreState);
    }

    [Fact]
    public async Task CloseStore_Approved_ClosesTheStore_WithTheReasonInThePreview()
    {
        //Arrange
        var data = new StoreToolsTestData();
        await using var harness = CreateHarness(data);

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "close_store", new { store_id = StoreB, reason = "Stock count" }, approved: true);

        //Assert
        Assert.Contains("Reason: Stock count", GetWarnings(result.EnsureSucceeded()));
        Assert.Equal(StoreState.Closed, data.Stores.Single(x => x.Id == StoreB).StoreState);
    }

    [Theory]
    [InlineData("open_store", StoreB, "is already open")]
    [InlineData("close_store", StoreA, "is already closed")]
    public async Task StateChange_ToTheStateTheStoreHas_IsRefused(string toolName, string storeId, string expectedError)
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), toolName, new { store_id = storeId });

        //Assert
        result.EnsureOutcome(McpToolCallOutcome.Failed);
        Assert.Contains(expectedError, result.Error);
    }

    // The basics and settings of a store that only misses payment and shipping, which this test platform does not have.
    private static Dictionary<string, object> CompleteSetup()
    {
        return new Dictionary<string, object>
        {
            ["basics"] = new
            {
                name = "EU store",
                default_language = "en-US",
                languages = new[] { "en-US" },
                default_currency = "USD",
                currencies = new[] { "USD" },
                catalog_id = "catalog-eu",
                url = "https://eu.example.com",
                email = "eu@example.com",
            },
            ["settings"] = new { values = new Dictionary<string, object> { [TaxCalculation.Name] = true } },
        };
    }

    private static McpPendingChange CreateChange(string id, string storeId, string status, int minutesAgo)
    {
        return new McpPendingChange
        {
            Id = id,
            ToolName = "create_configured_store",
            Status = status,
            Arguments = JsonSerializer.Serialize(new { store_id = storeId, setup = new { } }),
            CreatedDate = DateTime.UtcNow.AddMinutes(-minutesAgo),
        };
    }

    private static List<string> GetWarnings(McpToolCallResult result)
    {
        return result.Json.GetProperty("warnings").EnumerateArray().Select(x => x.GetString()).ToList();
    }
}
