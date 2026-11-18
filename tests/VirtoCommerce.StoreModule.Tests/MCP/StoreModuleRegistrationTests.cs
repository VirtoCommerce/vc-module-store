using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using VirtoCommerce.MCP.Core;
using VirtoCommerce.MCP.Core.Extensions;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Data.MCP.StoreSetup;
using Xunit;

namespace VirtoCommerce.StoreModule.Tests.MCP;

public class StoreModuleRegistrationTests
{
    [Theory]
    [InlineData(true, 11)]
    [InlineData(false, 0)]
    public void Initialize_RegistersStoresParts_Always_AndTheToolsOnlyWithTheMcpModule(bool mcpInstalled, int expectedToolCount)
    {
        //Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        //Act
        StoreToolsTestContext.CreateModule(mcpInstalled).Initialize(services);

        //Assert
        var parts = services.Where(x => x.ServiceType == typeof(IStoreSetupPart)).Select(x => x.ImplementationType);
        Assert.Equal([typeof(StoreBasicsSetupPart), typeof(StoreSettingsSetupPart)], parts);

        using var provider = services.BuildServiceProvider();
        var tools = provider.GetServices<McpServerTool>().Where(x => x.IsInMcpScope(McpScopes.Admin)).Select(x => x.ProtocolTool.Name).ToList();

        Assert.Equal(expectedToolCount, tools.Count);
        Assert.Equal(mcpInstalled, tools.Contains("get_store_readiness_report"));
    }
}
