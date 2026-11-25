using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using Xunit;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

// How every part writes the findings of a store's setup.
public class StoreSetupFindingsTests
{
    private readonly StoreSetupFindings _findings = new();

    [Theory]
    [InlineData(null, "Stores > EU store")]
    [InlineData("Payment methods", "Stores > EU store > Payment methods")]
    public void ForStore_StartsAFindingAboutTheStore_OnItsScreen(string tab, string expectedScreen)
    {
        //Act
        var finding = _findings.ForStore(new Store { Id = "eu-store", Name = "EU store" }, "payment", "payment-no-active-method", McpFindingSeverities.Critical, tab);

        //Assert
        Assert.Equal(("payment-no-active-method", "payment", McpFindingSeverities.Critical), (finding.Id, finding.Category, finding.Severity));
        Assert.Equal(expectedScreen, finding.Screen);
        Assert.Equal((nameof(Store), "eu-store", "EU store"), (finding.ObjectType, finding.ObjectId, finding.ObjectName));
    }

    // Secrets are counted, never named.
    [Fact]
    public void GetEmptySettingsEvidence_NamesTheSettings_AndOnlyCountsSecrets()
    {
        Assert.Equal("Empty: Test.Method.Merchant. 1 secret setting is empty.", _findings.GetEmptySettingsEvidence(["Test.Method.Merchant"], 1));
        Assert.Equal("2 secret settings are empty.", _findings.GetEmptySettingsEvidence([], 2));
    }
}
