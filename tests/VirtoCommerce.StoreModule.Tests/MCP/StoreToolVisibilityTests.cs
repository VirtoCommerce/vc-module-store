using System.Threading.Tasks;
using VirtoCommerce.MCP.Testing;
using Xunit;
using static VirtoCommerce.StoreModule.Tests.MCP.StoreToolsTestContext;

namespace VirtoCommerce.StoreModule.Tests.MCP;

public class StoreToolVisibilityTests
{
    private static readonly string[] _storeTools =
    [
        "compare_store_settings",
        "get_store",
        "get_store_overview",
        "get_store_readiness_report",
        "get_store_settings",
        "get_store_setup_options",
        "list_stores",
    ];

    [Fact]
    public async Task UserWithTheRolePermissions_SeesExactlyTheStoreTools()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var visible = harness.GetVisibleToolNames(StoreAdministrator());

        //Assert
        Assert.Equal(_storeTools, visible);
    }

    [Fact]
    public async Task UserWhoseStorePermissionsAreLimitedToAStore_SeesTheStoreTools()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var visible = harness.GetVisibleToolNames(StoreAAdministrator());

        //Assert
        Assert.Equal(_storeTools, visible);
    }

    [Fact]
    public async Task UserWithoutAStorePermission_SeesNoStoreTool()
    {
        //Arrange
        await using var harness = CreateHarness();

        //Act
        var visible = harness.GetVisibleToolNames(McpTestUser.WithPermissions("shipping-admin", "store:access", "shipping:read", "shipping:update"));

        //Assert
        Assert.Empty(visible);
    }
}
