using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.MCP.Testing;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;
using Xunit;
using static VirtoCommerce.StoreModule.Tests.MCP.StoreToolsTestContext;

namespace VirtoCommerce.StoreModule.Tests.MCP;

public class StoreReadToolsTests
{
    [Fact]
    public async Task ListStores_UserOfOneStore_SeesOnlyThatStore()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var mine = await CallAsync(harness, StoreAAdministrator(), "list_stores");
        var all = await CallAsync(harness, StoreAdministrator(), "list_stores");

        //Assert
        Assert.Equal([StoreA], GetStoreIds(mine.EnsureSucceeded().Json.GetProperty("items")));
        Assert.Equal([StoreA, StoreB], GetStoreIds(all.EnsureSucceeded().Json.GetProperty("items")));
    }

    [Fact]
    public async Task ListStores_UserWhoseStoreScopeNamesNoStore_SeesNoStore()
    {
        //Arrange
        await using var harness = CreateHarness();
        var user = McpTestUser.Create("other-scope").WithScopedPermission("store:read", "OtherScope", "x");

        //Act
        var stores = await CallAsync(harness, user, "list_stores");
        var overview = await CallAsync(harness, user, "get_store_overview");

        //Assert
        Assert.Empty(GetStoreIds(stores.EnsureSucceeded().Json.GetProperty("items")));
        Assert.Equal(0, overview.EnsureSucceeded().Json.GetProperty("total_count").GetInt32());
    }

    [Fact]
    public async Task GetStore_OfAStoreTheUserDoesNotManage_IsForbidden()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var own = await CallAsync(harness, StoreAAdministrator(), "get_store", new { store_id = StoreA });
        var other = await CallAsync(harness, StoreAAdministrator(), "get_store", new { store_id = StoreB });

        //Assert
        own.EnsureSucceeded();
        other.EnsureOutcome(McpToolCallOutcome.Forbidden);
    }

    // The comparison starts from the user's own store, which the user may read, so only the other store refuses it.
    [Fact]
    public async Task StoreReads_OfAStoreTheUserDoesNotManage_AreForbidden()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var ownComparison = await CallAsync(harness, StoreAAdministrator(), "compare_store_settings", new { store_id = StoreA });
        var report = await CallAsync(harness, StoreAAdministrator(), "get_store_readiness_report", new { store_id = StoreB });
        var settings = await CallAsync(harness, StoreAAdministrator(), "get_store_settings", new { store_id = StoreB });
        var comparison = await CallAsync(harness, StoreAAdministrator(), "compare_store_settings", new { store_id = StoreA, other_store_id = StoreB });

        //Assert
        ownComparison.EnsureSucceeded();
        report.EnsureOutcome(McpToolCallOutcome.Forbidden);
        settings.EnsureOutcome(McpToolCallOutcome.Forbidden);
        comparison.EnsureOutcome(McpToolCallOutcome.Forbidden);
    }

    // Access is checked before the store is loaded: a user of one store learns nothing about other ids, not even whether
    // a store has one.
    [Fact]
    public async Task GetStore_OfAnUnknownId_IsForbidden_ForAUserOfOneStore()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var ofOneStore = await CallAsync(harness, StoreAAdministrator(), "get_store", new { store_id = "no-such-store" });
        var ofEveryStore = await CallAsync(harness, StoreAdministrator(), "get_store", new { store_id = "no-such-store" });

        //Assert
        ofOneStore.EnsureOutcome(McpToolCallOutcome.Forbidden);
        Assert.DoesNotContain("not found", ofOneStore.Error);
        ofEveryStore.EnsureOutcome(McpToolCallOutcome.Failed);
        Assert.Contains("was not found", ofEveryStore.Error);
    }

    [Fact]
    public async Task GetStore_CountsTheSettingsChangedFromTheirDefaults_WithoutSecrets()
    {
        //Arrange
        var data = new StoreToolsTestData();
        data.Stores.Single(x => x.Id == StoreA).Settings =
        [
            new ObjectSettingEntry(TaxCalculation) { Value = true },
            new ObjectSettingEntry(MapsApiKey) { Value = MapsApiKeyValue },
        ];
        await using var harness = CreateHarness(data);

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "get_store", new { store_id = StoreA });

        //Assert
        var json = result.EnsureSucceeded().Json;
        Assert.Equal(1, json.GetProperty("changed_settings").GetInt32());
        Assert.True(json.GetProperty("parts").GetProperty("settings").GetProperty("values").GetProperty(TaxCalculation.Name).GetBoolean());
        Assert.DoesNotContain(MapsApiKeyValue, result.Text);
    }

    [Fact]
    public async Task GetStore_ShowsAPartsState_OnlyWithThePartsReadPermissionForTheStore()
    {
        //Arrange
        var shipping = new TestStatePart("shipping", new McpSetupPermissions { Read = "shipping:read", ScopeType = "SelectedStoreScope" });
        await using var harness = CreateHarness(shipping);
        var withShipping = McpTestUser.Create("a").WithPermissions("store:read").WithScopedPermission("shipping:read", "SelectedStoreScope", StoreA);
        var withoutShipping = McpTestUser.WithPermissions("b", "store:read");

        //Act
        var shown = await CallAsync(harness, withShipping, "get_store", new { store_id = StoreA });
        var hidden = await CallAsync(harness, withoutShipping, "get_store", new { store_id = StoreA });

        //Assert
        var shownState = shown.EnsureSucceeded().Json.GetProperty("parts").GetProperty("shipping");
        Assert.Equal("FixedRate", shownState.GetProperty("methods")[0].GetString());
        var hiddenState = hidden.EnsureSucceeded().Json.GetProperty("parts").GetProperty("shipping");
        Assert.Equal("Not shown: needs shipping:read.", hiddenState.GetString());
        Assert.Equal("Store A", hidden.Json.GetProperty("parts").GetProperty("basics").GetProperty("name").GetString());
    }

    // The part's read permission is limited to stores, and the user's grant names store B only.
    [Fact]
    public async Task GetStore_WithThePartsReadPermissionForAnotherStoreOnly_DoesNotShowThePart()
    {
        //Arrange
        var shipping = new TestStatePart("shipping", new McpSetupPermissions { Read = "shipping:read", ScopeType = "StoreSelectedScope" });
        await using var harness = CreateHarness(shipping);
        var user = McpTestUser.Create("store-b-shipping").WithPermissions("store:read").WithScopedPermission("shipping:read", "StoreSelectedScope", StoreB);

        //Act
        var storeA = await CallAsync(harness, user, "get_store", new { store_id = StoreA });
        var storeB = await CallAsync(harness, user, "get_store", new { store_id = StoreB });

        //Assert
        Assert.Equal("Not shown: needs shipping:read.", storeA.EnsureSucceeded().Json.GetProperty("parts").GetProperty("shipping").GetString());
        Assert.Equal("FixedRate", storeB.EnsureSucceeded().Json.GetProperty("parts").GetProperty("shipping").GetProperty("methods")[0].GetString());
    }

    [Fact]
    public async Task GetStoreReadinessReport_ShowsTheFindingsOfEveryPart_AndWhichPartsWereChecked()
    {
        //Arrange
        var catalog = new TestCheckPart("catalog").WithFinding("catalog-no-products", McpFindingSeverities.Critical);
        await using var harness = CreateHarness(catalog);

        //Act
        var report = await CallAsync(harness, StoreAdministrator(), "get_store_readiness_report", new { store_id = StoreA });

        //Assert
        var json = report.EnsureSucceeded().Json;
        Assert.False(json.GetProperty("ready_to_open").GetBoolean());
        Assert.Equal(["basics", "catalog"], json.GetProperty("checked_parts").EnumerateArray().Select(x => x.GetString()));
        var finding = Assert.Single(json.GetProperty("findings").EnumerateArray());
        Assert.Equal("catalog-no-products", finding.GetProperty("id").GetString());
        Assert.Equal("critical", finding.GetProperty("severity").GetString());
        Assert.True(finding.GetProperty("blocks_go_live").GetBoolean());
        Assert.Equal("catalog", finding.GetProperty("category").GetString());
        Assert.Equal("It matters.", finding.GetProperty("why").GetString());
        Assert.Equal("Fix it. Back office: Stores > Test (https://admin.example.com/#!/workspace/store).", finding.GetProperty("fix").GetString());
        Assert.Equal("https://admin.example.com/#!/workspace/store", json.GetProperty("back_office").GetString());
    }

    // A currency the store lacks keeps its customers from paying in it, but not the store from opening: ready_to_open is
    // open_store's answer, and can_check_out is the currency's.
    [Fact]
    public async Task GetStoreReadinessReport_WithACurrency_SaysWhetherCustomersCanPayInIt_ApartFromGoingLive()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var report = await CallAsync(harness, StoreAdministrator(), "get_store_readiness_report", new { store_id = StoreA, currency = "EUR" });

        //Assert
        var json = report.EnsureSucceeded().Json;
        Assert.True(json.GetProperty("ready_to_open").GetBoolean());
        Assert.False(json.GetProperty("can_check_out").GetBoolean());
        Assert.Equal("currency-not-in-store", Assert.Single(json.GetProperty("findings").EnumerateArray()).GetProperty("id").GetString());

        var withStoreCurrency = (await CallAsync(harness, StoreAdministrator(), "get_store_readiness_report", new { store_id = StoreA, currency = "USD" })).EnsureSucceeded().Json;
        Assert.True(withStoreCurrency.GetProperty("ready_to_open").GetBoolean());
        Assert.True(withStoreCurrency.GetProperty("can_check_out").GetBoolean());
        Assert.Empty(withStoreCurrency.GetProperty("findings").EnumerateArray());

        var withoutCurrency = (await CallAsync(harness, StoreAdministrator(), "get_store_readiness_report", new { store_id = StoreA })).EnsureSucceeded().Json;
        Assert.False(withoutCurrency.TryGetProperty("can_check_out", out _));
    }

    [Fact]
    public async Task GetStoreSettings_NeverListsASecret()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "get_store_settings", new { store_id = StoreA });

        //Assert
        var json = result.EnsureSucceeded().Json;
        var names = json.GetProperty("settings").GetProperty("items").EnumerateArray().Select(x => x.GetProperty("name").GetString());
        Assert.Equal([TaxCalculation.Name], names);
        Assert.DoesNotContain(MapsApiKeyValue, result.Text);
        Assert.DoesNotContain(MapsApiKey.Name, result.Text);
        Assert.Contains("back office", json.GetProperty("secrets").GetString());
    }

    // "calculation tax" is not a part of any text, but both words are in the title "Tax calculation".
    [Fact]
    public async Task GetStoreSettings_NeedsEveryWordOfTheKeyword_InTheNameTitleOrDescription()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var bothWords = await CallAsync(harness, StoreAdministrator(), "get_store_settings", new { store_id = StoreA, keyword = "calculation tax" });
        var oneWordMissing = await CallAsync(harness, StoreAdministrator(), "get_store_settings", new { store_id = StoreA, keyword = "tax shipping" });

        //Assert
        Assert.Equal([TaxCalculation.Name], GetSettingNames(bothWords));
        Assert.Empty(GetSettingNames(oneWordMissing));
    }

    [Fact]
    public async Task GetStoreSettings_RefusesAModuleWithoutStoreSettings_AndTakesTheIdInAnyCase()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var unknown = await CallAsync(harness, StoreAdministrator(), "get_store_settings", new { store_id = StoreA, module_id = "VirtoCommerce.Stores" });
        var anyCase = await CallAsync(harness, StoreAdministrator(), "get_store_settings", new { store_id = StoreA, module_id = " virtocommerce.store " });

        //Assert
        unknown.EnsureOutcome(McpToolCallOutcome.Failed);
        Assert.Contains("Module 'VirtoCommerce.Stores' has no store settings here", unknown.Error);
        Assert.Contains("get_store_settings without module_id", unknown.Error);
        Assert.Equal([TaxCalculation.Name], GetSettingNames(anyCase));
    }

    [Fact]
    public async Task GetStoreSettings_SendsSecretsToTheStoresScreen_WithItsLink()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "get_store_settings", new { store_id = StoreA });

        //Assert
        Assert.Equal("Secrets are not listed. A person enters them in the back office: Stores > Store A > Settings (https://admin.example.com/#!/workspace/store).",
            result.EnsureSucceeded().Json.GetProperty("secrets").GetString());
    }

    [Fact]
    public async Task CompareStoreSettings_ListsTheSettingsWhoseValuesDiffer()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "compare_store_settings", new { store_id = StoreA, other_store_id = StoreB });

        //Assert
        var difference = Assert.Single(result.EnsureSucceeded().Json.GetProperty("differences").GetProperty("items").EnumerateArray());
        Assert.Equal(TaxCalculation.Name, difference.GetProperty("name").GetString());
        Assert.True(difference.GetProperty("value").GetBoolean());
        Assert.False(difference.GetProperty("other_value").GetBoolean());
        Assert.DoesNotContain(MapsApiKeyValue, result.Text);
    }

    // Store B saves the value store A has, so the stores do not differ, although the value is not the default.
    [Fact]
    public async Task CompareStoreSettings_ReadsTheOtherStoresValues()
    {
        //Arrange
        var data = new StoreToolsTestData();
        data.Stores.Single(x => x.Id == StoreB).Settings = [new ObjectSettingEntry(TaxCalculation) { Value = true }];
        await using var harness = CreateHarness(data);

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "compare_store_settings", new { store_id = StoreA, other_store_id = StoreB });

        //Assert
        Assert.Empty(result.EnsureSucceeded().Json.GetProperty("differences").GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task GetStoreSetupOptions_ListsTheChoices_AndTheSchemaOfEachSection()
    {
        //Arrange
        await using var harness = CreateHarness(new TestSectionPart("shipping"));

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "get_store_setup_options");

        //Assert
        var parts = result.EnsureSucceeded().Json.GetProperty("parts").EnumerateArray().ToDictionary(x => x.GetProperty("name").GetString());
        Assert.Equal(["basics", "settings", "catalog", "shipping"], parts.Keys);

        var basics = parts["basics"];
        Assert.True(basics.GetProperty("required_for_new_store").GetBoolean());
        var basicsFields = basics.GetProperty("section_schema").GetProperty("properties");
        Assert.Equal("Store name. Required for a new store.", basicsFields.GetProperty("name").GetProperty("description").GetString());
        Assert.True(basicsFields.TryGetProperty("main_fulfillment_center_id", out _));
        Assert.False(basicsFields.TryGetProperty("catalog_id", out _));
        Assert.True(parts["catalog"].GetProperty("required_for_new_store").GetBoolean());
        Assert.Contains(basics.GetProperty("choices").EnumerateArray(), x => x.GetProperty("field").GetString() == "basics.languages" && x.GetProperty("value").GetString() == "de-DE");

        var settings = parts["settings"];
        Assert.False(settings.GetProperty("required_for_new_store").GetBoolean());
        Assert.True(settings.GetProperty("section_schema").GetProperty("properties").TryGetProperty("values", out _));

        var shipping = parts["shipping"];
        Assert.True(shipping.GetProperty("required_for_new_store").GetBoolean());
        var methods = shipping.GetProperty("section_schema").GetProperty("properties").GetProperty("methods");
        Assert.Equal("Codes of the active methods: the complete list.", methods.GetProperty("description").GetString());
        Assert.Equal("shipping.methods", shipping.GetProperty("choices")[0].GetProperty("field").GetString());
    }

    [Fact]
    public async Task GetStoreOverview_CountsTheStoresByState_AndListsTheClosedOnesFirst()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "get_store_overview");

        //Assert
        var json = result.EnsureSucceeded().Json;
        Assert.Equal(1, json.GetProperty("stores_by_state").GetProperty("open").GetInt32());
        Assert.Equal(1, json.GetProperty("stores_by_state").GetProperty("closed").GetInt32());
        var stores = json.GetProperty("stores").EnumerateArray().ToList();
        Assert.Equal([StoreA, StoreB], stores.Select(x => x.GetProperty("store_id").GetString()));
        Assert.True(stores[0].GetProperty("ready_to_open").GetBoolean());
        Assert.Equal(0, stores[1].GetProperty("warnings").GetInt32());
        Assert.False(stores[1].TryGetProperty("top_finding", out _));
    }

    // Store B, which store A's administrator may not read, is open: a count of every store would show it.
    [Fact]
    public async Task GetStoreOverview_UserOfOneStore_CountsOnlyThatStore()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var result = await CallAsync(harness, StoreAAdministrator(), "get_store_overview");

        //Assert
        var json = result.EnsureSucceeded().Json;
        var byState = json.GetProperty("stores_by_state");
        Assert.Equal((0, 1, 0), (byState.GetProperty("open").GetInt32(), byState.GetProperty("closed").GetInt32(), byState.GetProperty("restricted_access").GetInt32()));
        Assert.Equal([StoreA], GetStoreIds(json.GetProperty("stores")));
    }

    // A warning comes before an info, which the order of the severities' names would reverse.
    [Fact]
    public async Task GetStoreOverview_TopFindingIsTheMostSevere()
    {
        //Arrange
        var notes = new TestCheckPart("notes").WithFinding("note-info", McpFindingSeverities.Info).WithFinding("note-warning", McpFindingSeverities.Warning);
        await using var harness = CreateHarness(notes);

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "get_store_overview");

        //Assert
        var storeA = result.EnsureSucceeded().Json.GetProperty("stores").EnumerateArray().Single(x => x.GetProperty("store_id").GetString() == StoreA);
        Assert.Equal("Finding note-warning", storeA.GetProperty("top_finding").GetString());
    }

    [Fact]
    public async Task GetStore_ListsTheTopFindings_MostSevereFirst()
    {
        //Arrange
        var notes = new TestCheckPart("notes").WithFinding("note-info", McpFindingSeverities.Info).WithFinding("note-warning", McpFindingSeverities.Warning);
        await using var harness = CreateHarness(notes);

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "get_store", new { store_id = StoreA });

        //Assert
        var topFindings = result.EnsureSucceeded().Json.GetProperty("readiness").GetProperty("top_findings");
        Assert.Equal(["Finding note-warning", "Finding note-info"], topFindings.EnumerateArray().Select(x => x.GetString()));
    }

    [Fact]
    public async Task GetStoreOverview_And_GetStore_CountWhatBlocksTheStore()
    {
        //Arrange
        var catalog = new TestCheckPart("catalog").WithFinding("catalog-no-products", McpFindingSeverities.Critical).WithFinding("catalog-few-images", McpFindingSeverities.Warning);
        await using var harness = CreateHarness(catalog);

        //Act
        var overview = await CallAsync(harness, StoreAdministrator(), "get_store_overview");
        var store = await CallAsync(harness, StoreAdministrator(), "get_store", new { store_id = StoreA });

        //Assert
        var storeA = overview.EnsureSucceeded().Json.GetProperty("stores").EnumerateArray().Single(x => x.GetProperty("store_id").GetString() == StoreA);
        Assert.Equal((1, false), (storeA.GetProperty("blocking").GetInt32(), storeA.GetProperty("ready_to_open").GetBoolean()));
        var readiness = store.EnsureSucceeded().Json.GetProperty("readiness");
        Assert.Equal((1, false), (readiness.GetProperty("blocking").GetInt32(), readiness.GetProperty("ready_to_open").GetBoolean()));
    }

    // A finding shows with its part's state only. A user who may not read the catalog part gets the part's note instead,
    // and the finding still keeps the store from opening.
    [Fact]
    public async Task GetStoreReadinessReport_FindingOfAPartTheUserMayNotRead_IsCountedNotShown_AndStillBlocks()
    {
        //Arrange
        await using var harness = CreateHarness(CreateCatalogPart());
        var withoutCatalogRead = McpTestUser.WithPermissions("store-manager", "store:read", "store:update");
        var withCatalogRead = McpTestUser.WithPermissions("catalog-manager", "store:read", "store:update", "catalog:read");

        //Act
        var hidden = await CallAsync(harness, withoutCatalogRead, "get_store_readiness_report", new { store_id = StoreA });
        var shown = await CallAsync(harness, withCatalogRead, "get_store_readiness_report", new { store_id = StoreA });

        //Assert
        var hiddenJson = hidden.EnsureSucceeded().Json;
        Assert.Empty(hiddenJson.GetProperty("findings").EnumerateArray());
        Assert.Equal([CatalogNotShown], hiddenJson.GetProperty("not_shown").EnumerateArray().Select(x => x.GetString()));
        Assert.False(hiddenJson.GetProperty("ready_to_open").GetBoolean());
        Assert.DoesNotContain("Finding catalog-no-products", hidden.Text);
        Assert.DoesNotContain("Evidence of catalog-no-products", hidden.Text);

        var shownJson = shown.EnsureSucceeded().Json;
        Assert.Equal("catalog-no-products", Assert.Single(shownJson.GetProperty("findings").EnumerateArray()).GetProperty("id").GetString());
        Assert.Empty(shownJson.GetProperty("not_shown").EnumerateArray());
        Assert.Contains("Evidence of catalog-no-products", shown.Text);
    }

    // On store B, the warning the user may read is the top finding, although the hidden finding is more severe.
    [Fact]
    public async Task GetStoreOverview_FindingOfAPartTheUserMayNotRead_IsCountedNotShown_AndStillBlocks()
    {
        //Arrange
        var notes = new TestCheckPart("notes").WithFinding("note-warning", McpFindingSeverities.Warning);
        await using var harness = CreateHarness(CreateCatalogPart(), notes);
        var withoutCatalogRead = McpTestUser.WithPermissions("store-manager", "store:read", "store:update");
        var withCatalogRead = McpTestUser.WithPermissions("catalog-manager", "store:read", "store:update", "catalog:read");

        //Act
        var hidden = await CallAsync(harness, withoutCatalogRead, "get_store_overview");
        var shown = await CallAsync(harness, withCatalogRead, "get_store_overview");

        //Assert
        var hiddenStoreA = hidden.EnsureSucceeded().Json.GetProperty("stores").EnumerateArray().Single(x => x.GetProperty("store_id").GetString() == StoreA);
        // The warning the user may read is the top finding, although the hidden one is more severe.
        Assert.Equal("Finding note-warning", hiddenStoreA.GetProperty("top_finding").GetString());
        Assert.Equal((1, false), (hiddenStoreA.GetProperty("blocking").GetInt32(), hiddenStoreA.GetProperty("ready_to_open").GetBoolean()));
        Assert.Equal([CatalogNotShown], hiddenStoreA.GetProperty("not_shown").EnumerateArray().Select(x => x.GetString()));
        Assert.DoesNotContain("Finding catalog-no-products", hidden.Text);
        Assert.DoesNotContain("Evidence of catalog-no-products", hidden.Text);

        var shownStoreA = shown.EnsureSucceeded().Json.GetProperty("stores").EnumerateArray().Single(x => x.GetProperty("store_id").GetString() == StoreA);
        Assert.Equal("Finding catalog-no-products", shownStoreA.GetProperty("top_finding").GetString());
        Assert.Empty(shownStoreA.GetProperty("not_shown").EnumerateArray());
        Assert.Contains("Finding catalog-no-products", shown.Text);
    }

    [Fact]
    public async Task GetStore_FindingOfAPartTheUserMayNotRead_IsCountedNotShown_AndStillBlocks()
    {
        //Arrange
        await using var harness = CreateHarness(CreateCatalogPart());
        var withoutCatalogRead = McpTestUser.WithPermissions("store-manager", "store:read", "store:update");
        var withCatalogRead = McpTestUser.WithPermissions("catalog-manager", "store:read", "store:update", "catalog:read");

        //Act
        var hidden = await CallAsync(harness, withoutCatalogRead, "get_store", new { store_id = StoreA });
        var shown = await CallAsync(harness, withCatalogRead, "get_store", new { store_id = StoreA });

        //Assert
        var hiddenReadiness = hidden.EnsureSucceeded().Json.GetProperty("readiness");
        Assert.Empty(hiddenReadiness.GetProperty("top_findings").EnumerateArray());
        Assert.Equal((1, false), (hiddenReadiness.GetProperty("blocking").GetInt32(), hiddenReadiness.GetProperty("ready_to_open").GetBoolean()));
        Assert.Equal([CatalogNotShown], hiddenReadiness.GetProperty("not_shown").EnumerateArray().Select(x => x.GetString()));
        Assert.DoesNotContain("Finding catalog-no-products", hidden.Text);
        Assert.DoesNotContain("Evidence of catalog-no-products", hidden.Text);

        var shownReadiness = shown.EnsureSucceeded().Json.GetProperty("readiness");
        Assert.Equal(["Finding catalog-no-products"], shownReadiness.GetProperty("top_findings").EnumerateArray().Select(x => x.GetString()));
        Assert.Empty(shownReadiness.GetProperty("not_shown").EnumerateArray());
        Assert.Contains("Finding catalog-no-products", shown.Text);
    }

    [Fact]
    public async Task GetStoreReadinessReport_ListsTheFindings_MostSevereFirst()
    {
        //Arrange
        var notes = new TestCheckPart("notes")
            .WithFinding("note-info", McpFindingSeverities.Info)
            .WithFinding("note-warning", McpFindingSeverities.Warning)
            .WithFinding("note-critical", McpFindingSeverities.Critical);
        await using var harness = CreateHarness(notes);

        //Act
        var report = await CallAsync(harness, StoreAdministrator(), "get_store_readiness_report", new { store_id = StoreA });

        //Assert
        var ids = report.EnsureSucceeded().Json.GetProperty("findings").EnumerateArray().Select(x => x.GetProperty("id").GetString());
        Assert.Equal(["note-critical", "note-warning", "note-info"], ids);
    }

    [Fact]
    public async Task GetStoreReadinessReport_SaysOnWhichScreenEachFindingIsFixed()
    {
        //Arrange
        var catalog = new TestCheckPart("catalog").WithFinding("catalog-no-products", McpFindingSeverities.Critical);
        await using var harness = CreateHarness(catalog);

        //Act
        var report = await CallAsync(harness, StoreAdministrator(), "get_store_readiness_report", new { store_id = StoreA });

        //Assert
        var finding = Assert.Single(report.EnsureSucceeded().Json.GetProperty("findings").EnumerateArray());
        Assert.Equal("Stores > Test", finding.GetProperty("screen").GetString());
    }

    [Fact]
    public async Task ListStores_FiltersByState_AndPagesWithNextSkip()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var open = (await CallAsync(harness, StoreAdministrator(), "list_stores", new { state = "open" })).EnsureSucceeded().Json;
        var first = (await CallAsync(harness, StoreAdministrator(), "list_stores", new { take = 1 })).EnsureSucceeded().Json;
        var second = (await CallAsync(harness, StoreAdministrator(), "list_stores", new { skip = first.GetProperty("next_skip").GetInt32(), take = 1 })).EnsureSucceeded().Json;

        //Assert
        Assert.Equal([StoreB], GetStoreIds(open.GetProperty("items")));
        Assert.Equal([StoreA], GetStoreIds(first.GetProperty("items")));
        Assert.True(first.GetProperty("has_more").GetBoolean());
        Assert.Equal(2, first.GetProperty("total_count").GetInt32());
        Assert.Equal([StoreB], GetStoreIds(second.GetProperty("items")));
        Assert.False(second.GetProperty("has_more").GetBoolean());
        Assert.False(second.TryGetProperty("next_skip", out _));
    }

    // Store B keeps every default, store A changed one setting.
    [Fact]
    public async Task GetStoreSettings_ChangedOnly_ListsOnlyTheSettingsChangedFromTheirDefaults()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var storeA = (await CallAsync(harness, StoreAdministrator(), "get_store_settings", new { store_id = StoreA, changed_only = true })).EnsureSucceeded().Json;
        var storeB = (await CallAsync(harness, StoreAdministrator(), "get_store_settings", new { store_id = StoreB, changed_only = true })).EnsureSucceeded().Json;
        var storeBAll = (await CallAsync(harness, StoreAdministrator(), "get_store_settings", new { store_id = StoreB })).EnsureSucceeded().Json;

        //Assert
        Assert.Equal([TaxCalculation.Name], GetSettingNames(storeA));
        Assert.Empty(GetSettingNames(storeB));
        Assert.Equal([TaxCalculation.Name], GetSettingNames(storeBAll));
    }

    // Shipping's only store setting is a secret: the module is real, so the answer is an empty page with the note on
    // secrets, not "a wrong id".
    [Fact]
    public async Task GetStoreSettings_AModuleWhoseStoreSettingsAreAllSecrets_IsAnEmptyPage_WithTheNoteOnSecrets()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "get_store_settings", new { store_id = StoreA, module_id = MapsApiKey.ModuleId });

        //Assert
        var json = result.EnsureSucceeded().Json;
        Assert.Empty(GetSettingNames(json));
        Assert.StartsWith("Secrets are not listed.", json.GetProperty("secrets").GetString());
    }

    [Fact]
    public async Task CompareStoreSettings_WithoutAnotherStore_ComparesWithTheDefaults()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "compare_store_settings", new { store_id = StoreA });

        //Assert
        var json = result.EnsureSucceeded().Json;
        Assert.Equal("defaults", json.GetProperty("compared_with").GetString());
        var difference = Assert.Single(json.GetProperty("differences").GetProperty("items").EnumerateArray());
        Assert.Equal(TaxCalculation.Name, difference.GetProperty("name").GetString());
        Assert.True(difference.GetProperty("value").GetBoolean());
        Assert.False(difference.GetProperty("other_value").GetBoolean());
    }

    // A part that cannot list its options is named with a reason, never with the exception's text, and the others are
    // still listed.
    [Fact]
    public async Task GetStoreSetupOptions_APartThatCannotListItsOptions_IsNamedUnavailable_AndTheOthersAreListed()
    {
        //Arrange
        await using var harness = CreateHarness(new TestFailingChoicesPart());

        //Act
        var result = await CallAsync(harness, StoreAdministrator(), "get_store_setup_options");

        //Assert
        var json = result.EnsureSucceeded().Json;
        var parts = json.GetProperty("parts").EnumerateArray().ToList();
        var broken = Assert.Single(parts, x => x.GetProperty("name").GetString() == "broken");
        Assert.StartsWith("Broken part could not list its options", broken.GetProperty("unavailable").GetString());
        Assert.Contains(parts, x => x.GetProperty("name").GetString() == "basics");
        Assert.DoesNotContain(TestFailingChoicesPart.ExceptionText, json.GetRawText());
    }

    // A part's choices are its module's data: they show with the part's read permission, as its state and findings do,
    // and the row says when they do not.
    [Fact]
    public async Task GetStoreSetupOptions_ShowsAPartsChoices_OnlyWithThePartsReadPermission()
    {
        //Arrange
        await using var harness = CreateHarness(new TestChoicesPart(2));
        var reader = McpTestUser.Create("lists-reader").WithPermissions(RolePermissions).WithPermissions(TestChoicesPart.ReadPermission);

        //Act
        var withoutGrant = await CallAsync(harness, StoreAdministrator(), "get_store_setup_options");
        var withGrant = await CallAsync(harness, reader, "get_store_setup_options");

        //Assert
        var hidden = GetOptionsPart(withoutGrant, "lists");
        Assert.Empty(hidden.GetProperty("choices").EnumerateArray());
        Assert.Equal("needs test:read", hidden.GetProperty("choices_not_shown").GetString());
        var shown = GetOptionsPart(withGrant, "lists");
        Assert.Equal(["list-01", "list-02"], shown.GetProperty("choices").EnumerateArray().Select(x => x.GetProperty("value").GetString()));
        Assert.False(shown.TryGetProperty("choices_not_shown", out _));
    }

    // Store's own parts are limited to stores: a user of one store reads them for that store, so their choices show,
    // though the options name no store.
    [Fact]
    public async Task GetStoreSetupOptions_ShowsStoresOwnChoices_ToAUserOfOneStore()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var result = await CallAsync(harness, StoreAAdministrator(), "get_store_setup_options");

        //Assert
        var basics = GetOptionsPart(result, "basics");
        Assert.NotEmpty(basics.GetProperty("choices").EnumerateArray());
        Assert.False(basics.TryGetProperty("choices_not_shown", out _));
    }

    // The cap is per field: 60 lists are cut to 50, and the kinds after them are all listed, as the currencies after a
    // long list of languages are.
    [Fact]
    public async Task GetStoreSetupOptions_ListsAtMost50ChoicesPerField_AndSaysWhichFieldItCuts()
    {
        //Arrange
        await using var harness = CreateHarness(new TestChoicesPart(60, kindCount: 3));
        var reader = McpTestUser.Create("lists-reader").WithPermissions(RolePermissions).WithPermissions(TestChoicesPart.ReadPermission);

        //Act
        var result = await CallAsync(harness, reader, "get_store_setup_options");

        //Assert
        var part = GetOptionsPart(result, "lists");
        var choices = part.GetProperty("choices").EnumerateArray().Select(x => x.GetProperty("value").GetString()).ToList();
        Assert.Equal(53, choices.Count);
        Assert.Equal("list-01", choices[0]);
        Assert.Equal("list-50", choices[49]);
        Assert.Equal(["kind-1", "kind-2", "kind-3"], choices.Skip(50));
        Assert.Equal(["lists.id: 50 of 60 listed, the most likely first."], part.GetProperty("choices_cut").EnumerateArray().Select(x => x.GetString()));
    }

    private static JsonElement GetOptionsPart(McpToolCallResult result, string name)
    {
        return result.EnsureSucceeded().Json.GetProperty("parts").EnumerateArray().Single(x => x.GetProperty("name").GetString() == name);
    }

    private static string[] GetStoreIds(JsonElement items)
    {
        return items.EnumerateArray().Select(x => x.GetProperty("store_id").GetString()).ToArray();
    }

    private static string[] GetSettingNames(McpToolCallResult result)
    {
        return GetSettingNames(result.EnsureSucceeded().Json);
    }

    private static string[] GetSettingNames(JsonElement json)
    {
        return json.GetProperty("settings").GetProperty("items").EnumerateArray().Select(x => x.GetProperty("name").GetString()).ToArray();
    }
}
