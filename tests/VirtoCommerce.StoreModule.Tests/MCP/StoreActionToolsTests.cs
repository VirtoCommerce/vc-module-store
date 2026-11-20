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
        Assert.Equal(["basics", "settings", "catalog", "shipping"], json.GetProperty("parts").EnumerateArray().Select(x => x.GetProperty("part").GetString()));
        Assert.All(json.GetProperty("parts").EnumerateArray(), x => Assert.Equal("applied", x.GetProperty("status").GetString()));
        var store = Assert.Single(data.Stores, x => x.Id == NewStoreId);
        Assert.Equal(StoreState.Closed, store.StoreState);
        Assert.Equal("EU store", store.Name);
        Assert.Equal("catalog-EU catalog", store.Catalog);
        Assert.Equal(["USD"], store.Currencies);
        Assert.Equal(true, store.Settings.Single(x => x.Name == TaxCalculation.Name).Value);
        Assert.Equal(["FixedRate"], shipping.Saved[NewStoreId]);
    }

    [Fact]
    public async Task CreateConfiguredStore_WithANameAndACatalogOnly_IsAccepted_AndWarnsThatTheStoreWillNotBeReady()
    {
        //Arrange
        await using var harness = CreateHarness();
        var setup = new Dictionary<string, object> { ["basics"] = new { name = "EU store" }, ["catalog"] = new { name = "EU catalog" } };

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "create_configured_store", new { store_id = NewStoreId, setup, dryRun = true });

        //Assert
        var warnings = GetWarnings(result.EnsureSucceeded());
        Assert.Contains(warnings, x => x.StartsWith("The store will not be ready to open:") && x.Contains("has no language"));
    }

    // Before a create the store does not exist, so a finding of a part that takes a section is fixed in the same call.
    [Fact]
    public async Task CreateConfiguredStore_AFindingsFix_SendsItsSectionInTheSameCall()
    {
        //Arrange
        await using var harness = CreateHarness();
        var setup = new Dictionary<string, object> { ["basics"] = new { name = "EU store" }, ["catalog"] = new { name = "EU catalog" } };

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "create_configured_store", new { store_id = NewStoreId, setup, dryRun = true });

        //Assert
        var row = Assert.Single(result.EnsureSucceeded().Json.GetProperty("missing_after").EnumerateArray(), x => x.GetProperty("id").GetString() == "basics-no-language");
        var fix = row.GetProperty("fix").GetString();
        Assert.Contains("Send the 'basics' section in the same create_configured_store call.", fix);
        Assert.DoesNotContain("apply_store_setup", fix);
    }

    // The store's row needs a catalog: without the catalog section, the request is refused before any approval.
    [Fact]
    public async Task CreateConfiguredStore_WithoutTheCatalogSection_IsRefused()
    {
        //Arrange
        await using var harness = CreateHarness();
        var setup = new Dictionary<string, object> { ["basics"] = new { name = "EU store" } };

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "create_configured_store", new { store_id = NewStoreId, setup, dryRun = true });

        //Assert
        result.EnsureOutcome(McpToolCallOutcome.Failed);
        Assert.Contains("A new store needs the 'catalog' section.", result.Error);
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

    // The user may change the part on every store, but setting it up on a new store takes its Create permission.
    [Fact]
    public async Task CreateConfiguredStore_WithTheSectionOfAPartTheUserMayNotCreate_IsForbidden_AndCreatesNothing()
    {
        //Arrange
        var data = new StoreToolsTestData();
        var shipping = new TestApplyPart("shipping")
        {
            Permissions = new McpSetupPermissions { Read = "shipping:read", Update = "shipping:update", Create = "shipping:create", ScopeType = "SelectedStoreScope" },
        };
        await using var harness = CreateHarness(data, shipping);
        var user = McpTestUser.WithPermissions("store-creator", "store:create", "store:read", "store:update", "shipping:read", "shipping:update");
        var setup = new Dictionary<string, object>
        {
            ["basics"] = new { name = "EU store" },
            ["shipping"] = new { methods = new[] { "FixedRate" } },
        };

        //Act
        var result = await CallAsync(harness, user, "create_configured_store", new { store_id = NewStoreId, setup }, approved: true);

        //Assert
        result.EnsureOutcome(McpToolCallOutcome.Forbidden);
        Assert.Contains("shipping:create", result.Error);
        Assert.DoesNotContain(data.Stores, x => x.Id == NewStoreId);
        Assert.Empty(shipping.Saved);
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

    // Continuing a failed create changes the store it left, so it takes read and update access to that store, as any
    // other change does. Every user here may change the shipping part on every store, so only the store's own
    // permissions refuse the call.
    [Fact]
    public async Task CreateConfiguredStore_OnAStoreAFailedCreateLeft_IsForbidden_WithoutReadAndUpdateForIt()
    {
        //Arrange
        var data = new StoreToolsTestData();
        data.PendingChanges.Add(CreateChange("change-1", StoreA, McpPendingChangeStatus.Failed, minutesAgo: 5));
        var shipping = new TestApplyPart("shipping")
        {
            Permissions = new McpSetupPermissions { Read = "shipping:read", Update = "shipping:update", Create = "shipping:update", ScopeType = "SelectedStoreScope" },
        };
        await using var harness = CreateHarness(data, shipping);
        var storeBAdmin = McpTestUser.Create("store-b-admin")
            .WithPermissions("store:create", "shipping:update")
            .WithScopedPermission("store:read", "StoreSelectedScope", StoreB)
            .WithScopedPermission("store:update", "StoreSelectedScope", StoreB);
        var updaterWithoutRead = McpTestUser.WithPermissions("updater", "store:create", "store:update", "shipping:update");
        var readerWithoutUpdate = McpTestUser.WithPermissions("reader", "store:create", "store:read", "shipping:update");
        var readerAndUpdater = McpTestUser.WithPermissions("reader-updater", "store:create", "store:read", "store:update", "shipping:update");
        var setup = new Dictionary<string, object> { ["shipping"] = new { methods = new[] { "FixedRate" } } };

        //Act
        var ofStoreBAdmin = await CallAsync(harness, storeBAdmin, "create_configured_store", new { store_id = StoreA, setup }, approved: true);
        var withoutRead = await CallAsync(harness, updaterWithoutRead, "create_configured_store", new { store_id = StoreA, setup }, approved: true);
        var withoutUpdate = await CallAsync(harness, readerWithoutUpdate, "create_configured_store", new { store_id = StoreA, setup }, approved: true);
        var withBoth = await CallAsync(harness, readerAndUpdater, "create_configured_store", new { store_id = StoreA, setup, dryRun = true });

        //Assert
        ofStoreBAdmin.EnsureOutcome(McpToolCallOutcome.Forbidden);
        withoutRead.EnsureOutcome(McpToolCallOutcome.Forbidden);
        Assert.Contains("store:read", withoutRead.Error);
        withoutUpdate.EnsureOutcome(McpToolCallOutcome.Forbidden);
        Assert.Contains("store:update", withoutUpdate.Error);
        Assert.Contains(GetWarnings(withBoth.EnsureSucceeded()), x => x.Contains("failed change change-1"));
        Assert.Empty(shipping.Saved);
        Assert.Equal(StoreState.Closed, data.Stores.Single(x => x.Id == StoreA).StoreState);
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

    // The user may read every store but change store A only. The user may change the shipping part on every store, so
    // only the store's own update permission refuses the setup. Store B is restricted, so opening and closing it would
    // both change it.
    [Fact]
    public async Task StoreChanges_WithTheUpdatePermissionOfAnotherStoreOnly_AreForbidden_AndChangeNothing()
    {
        //Arrange
        var data = new StoreToolsTestData();
        data.Stores.Single(x => x.Id == StoreB).StoreState = StoreState.RestrictedAccess;
        var shipping = new TestApplyPart("shipping")
        {
            Permissions = new McpSetupPermissions { Read = "shipping:read", Update = "shipping:update", Create = "shipping:update", ScopeType = "SelectedStoreScope" },
        };
        await using var harness = CreateHarness(data, shipping);
        var user = McpTestUser.Create("store-a-editor")
            .WithPermissions("store:read", "shipping:update")
            .WithScopedPermission("store:update", "StoreSelectedScope", StoreA);
        var setup = new Dictionary<string, object> { ["shipping"] = new { methods = new[] { "FixedRate" } } };

        //Act
        var own = await CallAsync(harness, user, "apply_store_setup", new { store_id = StoreA, setup, dryRun = true });
        var apply = await CallAsync(harness, user, "apply_store_setup", new { store_id = StoreB, setup }, approved: true);
        var open = await CallAsync(harness, user, "open_store", new { store_id = StoreB }, approved: true);
        var close = await CallAsync(harness, user, "close_store", new { store_id = StoreB }, approved: true);

        //Assert
        own.EnsureSucceeded();
        apply.EnsureOutcome(McpToolCallOutcome.Forbidden);
        open.EnsureOutcome(McpToolCallOutcome.Forbidden);
        close.EnsureOutcome(McpToolCallOutcome.Forbidden);
        Assert.Empty(shipping.Saved);
        Assert.Equal(StoreState.RestrictedAccess, data.Stores.Single(x => x.Id == StoreB).StoreState);
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

    // The preview shows what the store would miss from the parts the user may read; the catalog part is only counted,
    // and its finding still makes the store not ready.
    [Fact]
    public async Task ApplyStoreSetup_DryRun_FindingOfAPartTheUserMayNotRead_IsCountedNotShown()
    {
        //Arrange
        await using var harness = CreateHarness(CreateCatalogPart());
        var withoutCatalogRead = StoreManagerWithoutCatalogRead();
        var withCatalogRead = StoreManagerWithCatalogRead();
        var setup = new Dictionary<string, object> { ["basics"] = new { name = "Store A renamed" } };

        //Act
        var hidden = await CallAsync(harness, withoutCatalogRead, "apply_store_setup", new { store_id = StoreA, setup, dryRun = true });
        var shown = await CallAsync(harness, withCatalogRead, "apply_store_setup", new { store_id = StoreA, setup, dryRun = true });

        //Assert
        var hiddenJson = hidden.EnsureSucceeded().Json;
        Assert.Empty(hiddenJson.GetProperty("missing_after").EnumerateArray());
        Assert.Equal([CatalogNotShown], hiddenJson.GetProperty("not_shown").EnumerateArray().Select(x => x.GetString()));
        Assert.Contains($"The store will not be ready to open: {CatalogNotShown}.", GetWarnings(hidden));
        Assert.DoesNotContain("Finding catalog-no-products", hidden.Text);
        Assert.DoesNotContain("Evidence of catalog-no-products", hidden.Text);

        var shownJson = shown.EnsureSucceeded().Json;
        Assert.Equal("catalog-no-products", Assert.Single(shownJson.GetProperty("missing_after").EnumerateArray()).GetProperty("id").GetString());
        Assert.Empty(shownJson.GetProperty("not_shown").EnumerateArray());
        Assert.Contains("The store will not be ready to open: Finding catalog-no-products.", GetWarnings(shown));
        Assert.Contains("Evidence of catalog-no-products", shown.Text);
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

    // Severities are text that other modules' parts write: a critical finding blocks, whatever its case.
    [Fact]
    public async Task OpenStore_IsRefused_ForACriticalFinding_WhateverTheCaseOfItsSeverity()
    {
        //Arrange
        var data = new StoreToolsTestData();
        var catalog = new TestCheckPart("catalog").WithFinding("catalog-no-products", "Critical");
        await using var harness = CreateHarness(data, catalog);

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "open_store", new { store_id = StoreA });

        //Assert
        result.EnsureOutcome(McpToolCallOutcome.Failed);
        Assert.Contains("Finding catalog-no-products", result.Error);
        Assert.Equal(StoreState.Closed, data.Stores.Single(x => x.Id == StoreA).StoreState);
    }

    // A blocking finding keeps the store closed even when the user may not read its part; the refusal names the part, not
    // the finding.
    [Fact]
    public async Task OpenStore_BlockedByAFindingOfAPartTheUserMayNotRead_IsRefused_NamingOnlyThePart()
    {
        //Arrange
        var data = new StoreToolsTestData();
        await using var harness = CreateHarness(data, CreateCatalogPart());
        var withoutCatalogRead = StoreManagerWithoutCatalogRead();
        var withCatalogRead = StoreManagerWithCatalogRead();

        //Act
        var hidden = await CallAsync(harness, withoutCatalogRead, "open_store", new { store_id = StoreA }, approved: true);
        var shown = await CallAsync(harness, withCatalogRead, "open_store", new { store_id = StoreA }, approved: true);

        //Assert
        hidden.EnsureOutcome(McpToolCallOutcome.Failed);
        Assert.Contains(CatalogNotShown, hidden.Error);
        Assert.DoesNotContain("Finding catalog-no-products", hidden.Error);
        Assert.DoesNotContain("Evidence of catalog-no-products", hidden.Error);
        shown.EnsureOutcome(McpToolCallOutcome.Failed);
        Assert.Contains("Finding catalog-no-products", shown.Error);
        Assert.Equal(StoreState.Closed, data.Stores.Single(x => x.Id == StoreA).StoreState);
    }

    // A warning of a part the user may not read is counted, never repeated among the warnings of a preview.
    [Fact]
    public async Task OpenStore_And_ApplyStoreSetup_WarningOfAPartTheUserMayNotRead_IsNotAPreviewWarning()
    {
        //Arrange
        var catalog = new TestCheckPart("catalog") { Permissions = new McpSetupPermissions { Read = "catalog:read" } }
            .WithFinding("catalog-few-images", McpFindingSeverities.Warning);
        await using var harness = CreateHarness(catalog);
        var withoutCatalogRead = StoreManagerWithoutCatalogRead();
        var withCatalogRead = StoreManagerWithCatalogRead();
        var setup = new Dictionary<string, object> { ["basics"] = new { name = "Store A renamed" } };

        //Act
        var hiddenOpen = await CallAsync(harness, withoutCatalogRead, "open_store", new { store_id = StoreA, dryRun = true });
        var hiddenApply = await CallAsync(harness, withoutCatalogRead, "apply_store_setup", new { store_id = StoreA, setup, dryRun = true });
        var shownOpen = await CallAsync(harness, withCatalogRead, "open_store", new { store_id = StoreA, dryRun = true });
        var shownApply = await CallAsync(harness, withCatalogRead, "apply_store_setup", new { store_id = StoreA, setup, dryRun = true });

        //Assert
        Assert.DoesNotContain("Finding catalog-few-images", hiddenOpen.EnsureSucceeded().Text);
        Assert.DoesNotContain("Finding catalog-few-images", hiddenApply.EnsureSucceeded().Text);
        Assert.DoesNotContain("Evidence of catalog-few-images", hiddenApply.Text);
        Assert.Equal(["Test part catalog: 1 finding not shown (needs catalog:read)"], hiddenApply.Json.GetProperty("not_shown").EnumerateArray().Select(x => x.GetString()));
        Assert.Contains("Finding catalog-few-images.", GetWarnings(shownOpen.EnsureSucceeded()));
        Assert.Contains("Finding catalog-few-images.", GetWarnings(shownApply.EnsureSucceeded()));
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
    // A store already named so may come after many stores whose names only hold the name: every store the keyword finds
    // is read.
    [Fact]
    public async Task CreateConfiguredStore_ANameAnotherStoreHas_IsWarned_AfterManyStoresThatHoldIt()
    {
        //Arrange
        var data = new StoreToolsTestData();
        data.Stores.AddRange(Enumerable.Range(1, 21).Select(x => new Store { Id = $"a-shop-{x:00}", Name = $"A shop {x:00}", Catalog = "catalog-a", Settings = [] }));
        data.Stores.Add(new Store { Id = "shop", Name = "Shop", Catalog = "catalog-a", Settings = [] });
        await using var harness = CreateHarness(data);
        var setup = CompleteSetup();
        setup["basics"] = new { name = "shop", default_language = "en-US", languages = new[] { "en-US" }, default_currency = "USD", currencies = new[] { "USD" } };

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "create_configured_store", new { store_id = NewStoreId, setup, dryRun = true });

        //Assert
        Assert.Contains("Another store is already named 'shop'.", GetWarnings(result.EnsureSucceeded()));
    }

    // A rename through apply gets the warning a new store's name gets; a name that does not change gets none, though
    // another store has it too.
    [Fact]
    public async Task ApplyStoreSetup_ARenameToAnotherStoresName_IsWarned_ButAnUnchangedNameIsNot()
    {
        //Arrange
        var data = new StoreToolsTestData();
        data.Stores.Add(new Store { Id = "store-a-copy", Name = "Store A", Catalog = "catalog-a", Settings = [] });
        await using var harness = CreateHarness(data);

        //Act
        var renamed = await CallAsync(harness, StoreAdministrator(), "apply_store_setup", new { store_id = StoreA, setup = new { basics = new { name = "Store B" } }, dryRun = true });
        var unchanged = await CallAsync(harness, StoreAdministrator(), "apply_store_setup", new { store_id = StoreA, setup = new { basics = new { name = "Store A", email = "new@example.com" } }, dryRun = true });

        //Assert
        Assert.Contains("Another store is already named 'Store B'.", GetWarnings(renamed.EnsureSucceeded()));
        Assert.DoesNotContain(GetWarnings(unchanged.EnsureSucceeded()), x => x.StartsWith("Another store is already named"));
    }

    // A refusal ends with the Stores screen and its link, where a person can also finish the setup: a part's error names
    // its screen in words only.
    [Fact]
    public async Task ApplyStoreSetup_ARefusal_EndsWithTheStoresScreen_AndItsLink()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "apply_store_setup", new { store_id = StoreA, setup = new { basics = new { name = "" } }, dryRun = true });

        //Assert
        Assert.Equal(McpToolCallOutcome.Failed, result.Outcome);
        Assert.StartsWith("The setup was refused, and nothing was changed. basics: A store needs a name", result.Error);
        Assert.EndsWith("Back office: Stores (https://admin.example.com/#!/workspace/store).", result.Error);
    }

    // An approved change runs as the approver, and the requester reads its answer: the findings are counted by part and
    // none is named. A dry run, which runs as the requester, keeps the rows.
    [Fact]
    public async Task ApplyStoreSetup_Approved_CountsTheFindingsByPart_AndNamesNone()
    {
        //Arrange
        await using var harness = CreateHarness(new TestCheckPart("audit").WithFinding("audit-gap", McpFindingSeverities.Critical));
        var setup = new { basics = new { email = "new@example.com" } };

        //Act
        var dryRun = await CallAsync(harness, StoreAdministrator(), "apply_store_setup", new { store_id = StoreA, setup, dryRun = true });
        var applied = await CallAsync(harness, StoreAdministrator(), "apply_store_setup", new { store_id = StoreA, setup }, approved: true);

        //Assert
        Assert.Contains(dryRun.EnsureSucceeded().Json.GetProperty("missing_after").EnumerateArray(), x => x.GetProperty("id").GetString() == "audit-gap");
        var json = applied.EnsureSucceeded().Json;
        Assert.False(json.TryGetProperty("missing_after", out _));
        Assert.Contains("Test part audit: 1 finding, 1 blocking going live", json.GetProperty("missing_after_by_part").EnumerateArray().Select(x => x.GetString()));
        Assert.Contains(GetWarnings(applied), x => x.StartsWith("The store is not ready to open: ") && x.EndsWith("get_store_readiness_report lists them."));
        Assert.DoesNotContain("Finding audit-gap", json.GetRawText());
    }

    // A failed create that saved nothing, then a store made by hand under that id: that store is not the create's
    // leftover.
    [Fact]
    public async Task CreateConfiguredStore_OnAStoreMadeAfterTheFailedCreate_IsRefused()
    {
        //Arrange
        var data = new StoreToolsTestData();
        data.Stores.Single(x => x.Id == StoreA).CreatedDate = DateTime.UtcNow;
        data.PendingChanges.Add(CreateChange("change-1", StoreA, McpPendingChangeStatus.Failed, minutesAgo: 5));
        await using var harness = CreateHarness(data);
        var setup = new Dictionary<string, object> { ["basics"] = new { name = "Store A", email = "new@example.com" } };

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "create_configured_store", new { store_id = StoreA, setup, dryRun = true });

        //Assert
        result.EnsureOutcome(McpToolCallOutcome.Failed);
        Assert.Contains("already exists. Change its setup with apply_store_setup", result.Error);
    }

    // The approver sees an existing store under the name it has; the new name is a row.
    [Fact]
    public async Task ApplyStoreSetup_ARename_NamesTheStoreAsItIs_AndShowsTheNewNameAsARow()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "apply_store_setup", new { store_id = StoreA, setup = new { basics = new { name = "Store A renamed" } }, dryRun = true });

        //Assert
        var json = result.EnsureSucceeded().Json;
        Assert.Equal("Store A", json.GetProperty("object_name").GetString());
        Assert.Contains(json.GetProperty("changes").EnumerateArray(), x => x.GetProperty("field").GetString() == "basics: name" && x.GetProperty("to").GetString() == "Store A renamed");
    }

    [Fact]
    public async Task CreateConfiguredStore_SaysTheStoreIdCannotBeChangedLater()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var schema = harness.GetTool("create_configured_store").ProtocolTool.InputSchema;

        //Assert
        Assert.EndsWith("It cannot be changed later.", schema.GetProperty("properties").GetProperty("store_id").GetProperty("description").GetString());
    }

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
                url = "https://eu.example.com",
                email = "eu@example.com",
            },
            ["catalog"] = new { name = "EU catalog" },
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
