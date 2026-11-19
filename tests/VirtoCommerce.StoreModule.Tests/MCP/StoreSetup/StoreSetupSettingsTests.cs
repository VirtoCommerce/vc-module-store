using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Localizations;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Data.MCP.StoreSetup;
using Xunit;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

public class StoreSetupSettingsTests
{
    private const string KnownSecretName = "Contoso.MerchantCode";

    private static readonly SettingDescriptor _taxCalculation = StoreToolsTestContext.TaxCalculation;
    private static readonly SettingDescriptor _states = new() { Name = "Stores.States", DisplayName = "Store states", ModuleId = "VirtoCommerce.Store", ValueType = SettingValueType.ShortText, IsDictionary = true, AllowedValues = ["Pending", "Live"] };
    private static readonly SettingDescriptor _hidden = new() { Name = "Stores.Internal", ModuleId = "VirtoCommerce.Store", ValueType = SettingValueType.ShortText, IsHidden = true };
    private static readonly SettingDescriptor _paymentMode = new() { Name = "VirtoCommerce.Payment.Mode", ModuleId = "VirtoCommerce.Payment", ValueType = SettingValueType.ShortText, DefaultValue = "Test", AllowedValues = ["Test", "Live"] };
    private static readonly SettingDescriptor _apiKey = StoreToolsTestContext.MapsApiKey;
    private static readonly SettingDescriptor _knownSecret = new() { Name = KnownSecretName, ModuleId = "Contoso", ValueType = SettingValueType.ShortText };

    // The MCP module's tests hold the rule itself (McpSecretSettings); these show Store applies it, with the names that
    // modules add.
    [Theory]
    [InlineData("Stores.TaxCalculationEnabled", SettingValueType.Boolean, false)]
    [InlineData("Shipping.Bopis.GoogleMaps.ApiKey", SettingValueType.ShortText, true)]
    [InlineData("Payments.Gateway.Login", SettingValueType.SecureString, true)]
    [InlineData(KnownSecretName, SettingValueType.ShortText, true)]
    public void IsSecret_FindsKnownAndDiscoveredSecrets(string name, SettingValueType valueType, bool expected)
    {
        //Act
        var isSecret = CreateSettings().IsSecret(new SettingDescriptor { Name = name, ValueType = valueType });

        //Assert
        Assert.Equal(expected, isSecret);
    }

    [Fact]
    public async Task GetStoreSettingsAsync_ListsNoSecretsAndNoHiddenSettings()
    {
        //Arrange
        var store = new Store
        {
            Id = "eu-store",
            Settings =
            [
                new ObjectSettingEntry(_taxCalculation) { Value = true },
                new ObjectSettingEntry(_apiKey) { Value = "AIza-maps-secret" },
                new ObjectSettingEntry(_knownSecret) { Value = "merchant-secret" },
            ],
        };

        //Act
        var settings = await CreateSettings().GetStoreSettingsAsync(store);

        //Assert
        Assert.Equal(["Stores.States", "Stores.TaxCalculationEnabled", "VirtoCommerce.Payment.Mode"], settings.Select(x => x.Name).Order());
        var json = JsonSerializer.Serialize(settings);
        Assert.DoesNotContain("AIza-maps-secret", json);
        Assert.DoesNotContain("merchant-secret", json);
        Assert.DoesNotContain(_apiKey.Name, json);
    }

    [Fact]
    public async Task GetStoreSettingsAsync_DescribesEachSetting_WithItsValueAndWhetherItChanged()
    {
        //Arrange
        var store = new Store { Id = "eu-store", Settings = [new ObjectSettingEntry(_taxCalculation) { Value = true }] };

        //Act
        var settings = (await CreateSettings().GetStoreSettingsAsync(store)).ToDictionary(x => x.Name);

        //Assert
        var taxCalculation = settings[_taxCalculation.Name];
        Assert.Equal("Tax calculation", taxCalculation.Title);
        Assert.Equal("Enables tax calculation in the storefront", taxCalculation.Description);
        Assert.Equal(true, taxCalculation.Value);
        Assert.True(taxCalculation.IsChanged);
        Assert.True(taxCalculation.IsChangeable);
        Assert.Equal("Boolean", taxCalculation.ValueType);

        var paymentMode = settings[_paymentMode.Name];
        Assert.Equal("Payment mode", paymentMode.Title);
        Assert.Equal("Test", paymentMode.Value);
        Assert.False(paymentMode.IsChanged);

        // Without a title in the localization files, the display name.
        var states = settings[_states.Name];
        Assert.Equal("Store states", states.Title);
        Assert.False(states.IsChangeable);
    }

    [Fact]
    public void Describe_Secret_HasNoValues()
    {
        //Act
        var setting = CreateSettings().Describe(_apiKey, "AIza-maps-secret");

        //Assert
        Assert.Null(setting.Value);
        Assert.Null(setting.DefaultValue);
        Assert.False(setting.IsChangeable);
    }

    [Theory]
    [InlineData("Stores.TaxCalculationEnabled", "true", true, true)]
    [InlineData("Stores.TaxCalculationEnabled", "\"false\"", true, false)]
    [InlineData("Stores.TaxCalculationEnabled", "\"maybe\"", false, null)]
    [InlineData("VirtoCommerce.Payment.Mode", "\"live\"", true, "Live")]
    [InlineData("VirtoCommerce.Payment.Mode", "\"Sandbox\"", false, null)]
    [InlineData("VirtoCommerce.Payment.Mode", "null", true, "Test")]
    [InlineData("Shipping.Bopis.GoogleMaps.ApiKey", "\"AIza-maps-secret\"", false, null)]
    [InlineData("Stores.States", "\"Live\"", false, null)]
    public void TryConvert_ConvertsTheValue_OrRefusesIt(string name, string json, bool expected, object expectedValue)
    {
        //Arrange
        var descriptor = new[] { _taxCalculation, _paymentMode, _apiKey, _states }.Single(x => x.Name == name);
        var raw = JsonDocument.Parse(json).RootElement;

        //Act
        var converted = CreateSettings().TryConvert(descriptor, raw, out var value, out var error);

        //Assert
        Assert.Equal(expected, converted);
        Assert.Equal(expectedValue, value);
        Assert.Equal(expected, error == null);
    }

    // The platform's settings save skips a null for a setting without allowed values, so a null could not return a
    // setting without a default to anything: it is refused, and an empty text still clears a text.
    [Fact]
    public void TryConvert_Null_IsRefused_ForASettingWithoutADefault()
    {
        //Arrange
        var measurementId = new SettingDescriptor { Name = "GoogleAnalytics4.MeasurementId", ModuleId = "VirtoCommerce.GoogleAnalytics4", ValueType = SettingValueType.ShortText };
        var settings = CreateSettings();

        //Act
        var nullConverted = settings.TryConvert(measurementId, raw: null, out _, out var error);
        var emptyConverted = settings.TryConvert(measurementId, JsonDocument.Parse("\"\"").RootElement, out var emptyValue, out _);

        //Assert
        Assert.False(nullConverted);
        Assert.Equal("Setting 'GoogleAnalytics4.MeasurementId' has no default to return to. Send a value; an empty text clears a text setting.", error);
        Assert.True(emptyConverted);
        Assert.Equal(string.Empty, emptyValue);
    }

    [Fact]
    public void TryConvert_Secret_IsRefused_WithAMessageThatNamesTheBackOffice()
    {
        //Act
        CreateSettings().TryConvert(_apiKey, "AIza-maps-secret", out _, out var error);

        //Assert
        Assert.Contains("secret", error);
        Assert.Contains("back office", error);
        Assert.DoesNotContain("AIza-maps-secret", error);
    }

    private static StoreSetupSettings CreateSettings()
    {
        var registrar = new Mock<ISettingsRegistrar>();
        registrar
            .Setup(x => x.GetSettingsForType(nameof(Store)))
            .Returns([_taxCalculation, _states, _hidden, _paymentMode, _apiKey, _knownSecret]);

        // The localization files nest a setting name by its dots, and a key may hold several segments with their dots.
        var translation = JObject.Parse("""
            {
              "settings": {
                "Stores": { "TaxCalculationEnabled": { "title": "Tax calculation", "description": "Enables tax calculation in the storefront" } },
                "VirtoCommerce.Payment": { "Mode": { "title": "Payment mode" } }
              }
            }
            """);
        var translationService = new Mock<ITranslationService>();
        translationService.Setup(x => x.GetTranslationDataForLanguage("en")).Returns(translation);

        var secretOptions = new McpSecretSettingsOptions();
        secretOptions.SettingNames.Add(KnownSecretName);

        return new StoreSetupSettings(registrar.Object, translationService.Object, Options.Create(secretOptions));
    }
}
