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

    [Fact]
    public async Task GetStoreReadinessReport_WithACurrency_ChecksThatCustomersCanPayInIt()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var report = await CallAsync(harness, StoreAdministrator(), "get_store_readiness_report", new { store_id = StoreA, currency = "EUR" });

        //Assert
        var json = report.EnsureSucceeded().Json;
        Assert.False(json.GetProperty("ready_to_open").GetBoolean());
        Assert.Equal("currency-not-in-store", Assert.Single(json.GetProperty("findings").EnumerateArray()).GetProperty("id").GetString());

        var withStoreCurrency = (await CallAsync(harness, StoreAdministrator(), "get_store_readiness_report", new { store_id = StoreA, currency = "USD" })).EnsureSucceeded().Json;
        Assert.True(withStoreCurrency.GetProperty("ready_to_open").GetBoolean());
        Assert.Empty(withStoreCurrency.GetProperty("findings").EnumerateArray());
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
        Assert.Equal(["basics", "settings", "shipping"], parts.Keys);

        var basics = parts["basics"];
        Assert.True(basics.GetProperty("required_for_new_store").GetBoolean());
        var basicsFields = basics.GetProperty("section_schema").GetProperty("properties");
        Assert.Equal("Store name. Required for a new store.", basicsFields.GetProperty("name").GetProperty("description").GetString());
        Assert.True(basicsFields.TryGetProperty("catalog_id", out _));
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
        Assert.Equal(1, stores[1].GetProperty("warnings").GetInt32());
        Assert.Equal("No catalog is linked to the store", stores[1].GetProperty("top_finding").GetString());
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

    private static string[] GetStoreIds(JsonElement items)
    {
        return items.EnumerateArray().Select(x => x.GetProperty("store_id").GetString()).ToArray();
    }

    private static string[] GetSettingNames(McpToolCallResult result)
    {
        return result.EnsureSucceeded().Json.GetProperty("settings").GetProperty("items").EnumerateArray().Select(x => x.GetProperty("name").GetString()).ToArray();
    }
}
