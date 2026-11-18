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
    [InlineData(true)]
    [InlineData(false)]
    public void Initialize_RegistersStoresParts_Always_AndTheToolsOnlyWithTheMcpModule(bool mcpInstalled)
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

        if (mcpInstalled)
        {
            Assert.Equal(11, tools.Count);
            Assert.Contains("get_store_readiness_report", tools);
        }
        else
        {
            Assert.Empty(tools);
        }
    }
}
