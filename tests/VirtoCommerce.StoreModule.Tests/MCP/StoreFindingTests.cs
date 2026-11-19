using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.MCP.Models;
using Xunit;

namespace VirtoCommerce.StoreModule.Tests.MCP;

public class StoreFindingTests
{
    private const string Link = "https://admin.example.com/#!/workspace/store";

    // An empty secret is entered by a person: its fix names no tool, though the part takes a section.
    [Theory]
    [InlineData(false, "Fill it. Tool: apply_store_setup with the 'payment' section. Back office: Stores > EU store (" + Link + ").")]
    [InlineData(true, "Fill it. Back office: Stores > EU store (" + Link + ").")]
    public void Create_NamesTheTool_UnlessTheFindingIsManualOnly(bool manualOnly, string expectedFix)
    {
        //Arrange
        var finding = new McpFinding { Id = "payment-secret", Category = "payment", Fix = "Fill it.", Screen = "Stores > EU store", ManualOnly = manualOnly };

        //Act
        var row = StoreFinding.Create(finding, partTakesSection: true, Link);

        //Assert
        Assert.Equal(expectedFix, row.Fix);
        Assert.Equal(manualOnly, row.ManualOnly);
    }
}
