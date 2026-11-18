using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;
using VirtoCommerce.Platform.Core.Localizations;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Data.MCP.StoreSetup;
using Xunit;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

public class StoreSettingsSectionTests
{
    private static readonly SettingDescriptor _taxCalculation = StoreToolsTestContext.TaxCalculation;
    private static readonly SettingDescriptor _seoLinks = new() { Name = "Stores.SeoLinksType", ModuleId = "VirtoCommerce.Store", ValueType = SettingValueType.ShortText, DefaultValue = "Collapsed", AllowedValues = ["None", "Short", "Collapsed", "Long"] };
    private static readonly SettingDescriptor _mapsApiKey = StoreToolsTestContext.MapsApiKey;

    [Fact]
    public async Task PlanAsync_RefusesAnUnknownSetting_AndASecret()
    {
        //Arrange
        var section = new StoreSettingsSection();
        section.Values["Stores.NoSuchSetting"] = Json("1");
        section.Values[_mapsApiKey.Name] = Json("\"AIza-maps-secret\"");

        //Act
        var plan = await CreatePart([]).PlanAsync(new StoreSetupContext { Store = CreateStore() }, section);

        //Assert
        Assert.Contains("The store has no setting 'Stores.NoSuchSetting'. get_store_settings lists the settings.", plan.Errors);
        Assert.Contains(plan.Errors, x => x.Contains("is a secret") && x.Contains("back office"));
        Assert.DoesNotContain(plan.Errors, x => x.Contains("AIza-maps-secret"));
        Assert.Empty(plan.Changes);
    }

    [Fact]
    public async Task PlanAsync_ListsTheChanges_AndNullReturnsASettingToItsDefault()
    {
        //Arrange
        var context = new StoreSetupContext { Store = CreateStore() };
        var section = new StoreSettingsSection();
        section.Values[_taxCalculation.Name] = Json("false");
        section.Values[_seoLinks.Name] = Json("null");

        //Act
        var plan = await CreatePart([]).PlanAsync(context, section);

        //Assert
        Assert.Empty(plan.Errors);
        Assert.Equal([(_taxCalculation.Name, (object)true, (object)false), (_seoLinks.Name, "Long", "Collapsed")], plan.Changes.Select(x => (x.Field, x.From, x.To)));
        Assert.Equal(false, context.Store.Settings.Single(x => x.Name == _taxCalculation.Name).Value);
    }

    [Fact]
    public async Task ApplyAsync_SavesTheValuesWithTheSavedStore()
    {
        //Arrange
        var stores = new List<Store> { CreateStore() };
        var part = CreatePart(stores);
        var context = new StoreSetupContext { Store = CreateStore() };
        var section = new StoreSettingsSection();
        section.Values[_taxCalculation.Name] = Json("false");
        var plan = await part.PlanAsync(context, section);

        //Act
        await part.ApplyAsync(context, plan);

        //Assert
        var saved = Assert.Single(stores);
        Assert.Equal(false, saved.Settings.Single(x => x.Name == _taxCalculation.Name).Value);
        Assert.Equal("Long", saved.Settings.Single(x => x.Name == _seoLinks.Name).Value);
    }

    private static Store CreateStore()
    {
        return new Store
        {
            Id = "eu-store",
            Settings =
            [
                new ObjectSettingEntry(_taxCalculation) { Value = true },
                new ObjectSettingEntry(_seoLinks) { Value = "Long" },
            ],
        };
    }

    private static object Json(string json)
    {
        return JsonDocument.Parse(json).RootElement;
    }

    private static StoreSettingsSetupPart CreatePart(List<Store> stores)
    {
        var registrar = new Mock<ISettingsRegistrar>();
        registrar.Setup(x => x.GetSettingsForType(nameof(Store))).Returns([_taxCalculation, _seoLinks, _mapsApiKey]);

        var translationService = new Mock<ITranslationService>();
        translationService.Setup(x => x.GetTranslationDataForLanguage("en")).Returns(new JObject());

        var setupSettings = new StoreSetupSettings(registrar.Object, translationService.Object, Options.Create(new StoreSetupSecretOptions()));

        return new StoreSettingsSetupPart(setupSettings, registrar.Object, TestStoreService.Create(stores).Object);
    }
}
