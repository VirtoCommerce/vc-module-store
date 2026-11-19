using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using Xunit;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

// The settings rules every part uses for an object that has settings, such as a payment method.
public class StoreSetupSettingsExtensionsTests
{
    private static readonly SettingDescriptor _rate = new() { Name = "Test.Method.Rate", ValueType = SettingValueType.Decimal, DefaultValue = 0m };
    private static readonly SettingDescriptor _mode = new() { Name = "Test.Method.Mode", ValueType = SettingValueType.ShortText, DefaultValue = "Test" };
    private static readonly SettingDescriptor _apiKey = new() { Name = "Test.Method.ApiKey", ValueType = SettingValueType.ShortText };

    [Fact]
    public void DescribeAll_And_GetValues_LeaveSecretsOut()
    {
        //Arrange
        var setupSettings = new KeySecretSettings();
        var method = new TestMethod { Settings = [new ObjectSettingEntry(_rate) { Value = 4.5m }, new ObjectSettingEntry(_apiKey) { Value = "sk_live_4242" }] };

        //Act
        var described = setupSettings.DescribeAll([_rate, _mode, _apiKey]);
        var values = setupSettings.GetValues(method, [_rate, _mode, _apiKey]);

        //Assert
        Assert.Equal([_rate.Name, _mode.Name], described.Select(x => x.Name));
        Assert.Equal(new Dictionary<string, object> { [_rate.Name] = 4.5m, [_mode.Name] = "Test" }, values);
    }

    [Fact]
    public void GetValue_IsTheSavedValue_OrTheDefaultOfASettingNeverSaved()
    {
        //Arrange
        var method = new TestMethod { Settings = [new ObjectSettingEntry(_rate) { Value = 4.5m }, new ObjectSettingEntry(_mode)] };

        //Act and assert
        Assert.Equal(4.5m, StoreSetupSettingsExtensions.GetValue(method, _rate));
        Assert.Equal("Test", StoreSetupSettingsExtensions.GetValue(method, _mode));
        Assert.Equal("Test", StoreSetupSettingsExtensions.GetValue(new TestMethod(), _mode));
    }

    [Theory]
    [InlineData(null, "Stores > EU store")]
    [InlineData("Payment methods", "Stores > EU store > Payment methods")]
    public void ForStore_StartsAFindingAboutTheStore_OnItsScreen(string tab, string expectedScreen)
    {
        //Act
        var finding = StoreSetupFindings.ForStore(new Store { Id = "eu-store", Name = "EU store" }, "payment", "payment-no-active-method", McpFindingSeverities.Critical, tab);

        //Assert
        Assert.Equal(("payment-no-active-method", "payment", McpFindingSeverities.Critical), (finding.Id, finding.Category, finding.Severity));
        Assert.Equal(expectedScreen, finding.Screen);
        Assert.Equal((nameof(Store), "eu-store", "EU store"), (finding.ObjectType, finding.ObjectId, finding.ObjectName));
    }

    private sealed class TestMethod : IHasSettings
    {
        public string Id { get; set; }

        public string TypeName => nameof(TestMethod);

        public ICollection<ObjectSettingEntry> Settings { get; set; }
    }

    // Store's settings rules in short: a name ending with Key is a secret.
    private sealed class KeySecretSettings : IStoreSetupSettings
    {
        public bool IsSecret(SettingDescriptor descriptor)
        {
            return descriptor.Name.EndsWith("Key", StringComparison.OrdinalIgnoreCase);
        }

        public McpSetupSetting Describe(SettingDescriptor descriptor, object value)
        {
            return new McpSetupSetting { Name = descriptor.Name, DefaultValue = descriptor.DefaultValue, Value = value ?? descriptor.DefaultValue };
        }

        public bool TryConvert(SettingDescriptor descriptor, object raw, out object value, out string error)
        {
            value = raw ?? descriptor.DefaultValue;
            error = null;
            return true;
        }

        public Task<IList<McpSetupSetting>> GetStoreSettingsAsync(Store store)
        {
            return Task.FromResult<IList<McpSetupSetting>>([]);
        }
    }
}
