using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.MCP.Models;
using Xunit;

namespace VirtoCommerce.StoreModule.Tests.MCP;

public class StoreFindingTests
{
    private const string Link = "https://admin.example.com/#!/workspace/store";

    // Before a create the store does not exist: the section belongs in the same create call.
    [Fact]
    public void Create_InACreate_SendsTheSectionInTheSameCall()
    {
        //Arrange
        var finding = new McpFinding { Id = "payment-no-active-method", Category = "payment", Fix = "Fill it.", Screen = "Stores > EU store" };

        //Act
        var row = StoreFinding.Create(finding, partTakesSection: true, blocksGoLive: true, Link, inCreate: true);

        //Assert
        Assert.Equal("Fill it. Send the 'payment' section in the same create_configured_store call. Back office: Stores > EU store (" + Link + ").", row.Fix);
    }

    // An empty secret is entered by a person: its fix names no tool, though the part takes a section. Whether the finding
    // blocks going live is the setup service's call, given with it.
    [Theory]
    [InlineData(false, "Fill it. Tool: apply_store_setup with the 'payment' section. Back office: Stores > EU store (" + Link + ").")]
    [InlineData(true, "Fill it. Back office: Stores > EU store (" + Link + ").")]
    public void Create_NamesTheTool_UnlessTheFindingIsManualOnly(bool manualOnly, string expectedFix)
    {
        //Arrange
        var finding = new McpFinding { Id = "payment-secret", Category = "payment", Fix = "Fill it.", Screen = "Stores > EU store", ManualOnly = manualOnly };

        //Act
        var row = StoreFinding.Create(finding, partTakesSection: true, blocksGoLive: manualOnly, Link);

        //Assert
        Assert.Equal(expectedFix, row.Fix);
        Assert.Equal(manualOnly, row.ManualOnly);
        Assert.Equal(manualOnly, row.BlocksGoLive);
    }
}
