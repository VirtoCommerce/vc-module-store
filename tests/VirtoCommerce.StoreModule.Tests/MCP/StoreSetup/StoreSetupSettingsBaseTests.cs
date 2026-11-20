using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using Xunit;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

// The settings rules every part uses for an object that has settings, such as a payment method.
public class StoreSetupSettingsBaseTests
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
        var setupSettings = new KeySecretSettings();

        Assert.Equal(4.5m, setupSettings.GetValue(method, _rate));
        Assert.Equal("Test", setupSettings.GetValue(method, _mode));
        Assert.Equal("Test", setupSettings.GetValue(new TestMethod(), _mode));
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

    [Fact]
    public void GetEmptyRequiredSettings_NamesTheEmptyOnes_AndOnlyCountsSecrets()
    {
        //Arrange
        var merchant = new SettingDescriptor { Name = "Test.Method.Merchant", ValueType = SettingValueType.ShortText, IsRequired = true };
        var apiKey = new SettingDescriptor { Name = "Test.Method.ApiKey", ValueType = SettingValueType.ShortText, IsRequired = true };
        var region = new SettingDescriptor { Name = "Test.Method.Region", ValueType = SettingValueType.ShortText, IsRequired = true, DefaultValue = "EU" };
        var method = new TestMethod { Settings = [new ObjectSettingEntry(merchant) { Value = " " }] };

        //Act
        var (names, secretCount) = new KeySecretSettings().GetEmptyRequiredSettings(method, [merchant, apiKey, region, _mode]);

        //Assert
        Assert.Equal([merchant.Name], names);
        Assert.Equal(1, secretCount);
        Assert.Equal("Empty: Test.Method.Merchant. 1 secret setting is empty.", StoreSetupFindings.GetEmptySettingsEvidence(names, secretCount));
        Assert.Equal("2 secret settings are empty.", StoreSetupFindings.GetEmptySettingsEvidence([], 2));
    }

    // A solution changes a rule by overriding it: the other rules use the override, as the parts do.
    [Fact]
    public void GetEmptyRequiredSettings_UsesTheRuleOfASubclass()
    {
        //Arrange
        var merchant = new SettingDescriptor { Name = "Test.Method.Merchant", ValueType = SettingValueType.ShortText, IsRequired = true };
        var method = new TestMethod { Settings = [new ObjectSettingEntry(merchant) { Value = "-" }] };

        //Act
        var (names, _) = new DashIsEmptySettings().GetEmptyRequiredSettings(method, [merchant]);

        //Assert
        Assert.Equal([merchant.Name], names);
    }

    // Applying a plan to a method read again: only what the plan changes is copied, and what changed since stays.
    [Fact]
    public void GetChangedFields_And_CopySettings_ApplyOnlyWhatThePlanChanges()
    {
        //Arrange
        var plan = new McpSetupPartPlan()
            .AddChange("methods.card.active", false, true)
            .AddChange($"methods.card.{_rate.Name}", 4.5m, 5m)
            .AddChange("methods.cash.active", true, false);
        var planned = new TestMethod { Settings = [new ObjectSettingEntry(_rate) { Value = 5m }, new ObjectSettingEntry(_mode) { Value = "Live" }] };
        var readAgain = new TestMethod { Settings = [new ObjectSettingEntry(_mode) { Value = "Edited since" }] };

        //Act
        var fields = plan.GetChangedFields("methods.card.");
        new KeySecretSettings().CopySettings(planned, readAgain, fields);

        //Assert
        Assert.Equal(["active", _rate.Name], fields.Order());
        Assert.Equal(5m, readAgain.Settings.Single(x => x.Name == _rate.Name).Value);
        Assert.Equal("Edited since", readAgain.Settings.Single(x => x.Name == _mode.Name).Value);
    }

    // A change of letter case is a change of a text, as the platform compares setting values.
    [Fact]
    public void PlanSettings_ACaseOnlyChange_IsAChange()
    {
        //Arrange
        var plan = new McpSetupPartPlan();
        var method = new TestMethod { Settings = [new ObjectSettingEntry(_mode) { Value = "Test" }] };

        //Act
        var changed = new KeySecretSettings().PlanSettings(plan, method, [_mode], new Dictionary<string, object> { [_mode.Name] = "test" },
            "methods.card.", "Method 'card'", "the options");

        //Assert
        Assert.Equal("test", changed[_mode.Name]);
        Assert.Equal([$"methods.card.{_mode.Name}"], plan.Changes.Select(x => x.Field));
    }

    private sealed class TestMethod : IHasSettings
    {
        public string Id { get; set; }

        public string TypeName => nameof(TestMethod);

        public ICollection<ObjectSettingEntry> Settings { get; set; }
    }

    // Store's settings rules in short: a name ending with Key is a secret.
    private class KeySecretSettings : StoreSetupSettingsBase
    {
        public override bool IsSecret(SettingDescriptor descriptor)
        {
            return descriptor.Name.EndsWithIgnoreCase("Key");
        }

        public override McpSetupSetting Describe(SettingDescriptor descriptor, object value)
        {
            return new McpSetupSetting { Name = descriptor.Name, DefaultValue = descriptor.DefaultValue, Value = value ?? descriptor.DefaultValue };
        }

        public override bool TryConvert(SettingDescriptor descriptor, object raw, out object value, out string error)
        {
            value = raw ?? descriptor.DefaultValue;
            error = null;
            return true;
        }

        public override Task<IList<McpSetupSetting>> GetStoreSettingsAsync(Store store)
        {
            return Task.FromResult<IList<McpSetupSetting>>([]);
        }
    }

    // A solution's rule: a dash is no value.
    private sealed class DashIsEmptySettings : KeySecretSettings
    {
        protected override bool IsEmpty(object value)
        {
            return base.IsEmpty(value) || value is "-";
        }
    }
}
