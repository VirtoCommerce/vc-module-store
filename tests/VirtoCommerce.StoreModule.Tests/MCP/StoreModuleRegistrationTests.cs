using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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

    // Another module's part uses Store's setup services whether the MCP module is installed or not, so Store registers
    // them, and the MCP module's setting rules they use, itself: with only what the platform provides, they resolve and plan.
    [Fact]
    public async Task Initialize_WithoutTheMcpModule_TheSetupServicesResolve_AndPlan()
    {
        //Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        StoreToolsTestContext.CreateModule(mcpInstalled: false).Initialize(services);
        StoreToolsTestContext.AddPlatformServices(services, new StoreToolsTestData());
        await using var provider = services.BuildServiceProvider();

        var section = new StoreSettingsSection();
        section.Values[StoreToolsTestContext.TaxCalculation.Name] = JsonSerializer.SerializeToElement(false);
        var request = new StoreSetupRequest { StoreId = StoreToolsTestContext.StoreA };
        request.Sections[StoreSettingsSetupPart.PartName] = section;

        //Act
        var result = await provider.GetRequiredService<IStoreSetupService>().PlanAsync(request);
        var described = provider.GetRequiredService<IStoreSetupSettings>().DescribeAll([StoreToolsTestContext.TaxCalculation, StoreToolsTestContext.MapsApiKey]);

        //Assert
        Assert.Empty(result.Errors);
        var change = Assert.Single(Assert.Single(result.Parts).Changes);
        Assert.Equal((StoreToolsTestContext.TaxCalculation.Name, (object)true, (object)false), (change.Field, change.From, change.To));
        Assert.DoesNotContain(result.Findings, x => x.Id.EndsWith("-not-checked"));
        Assert.Equal("Tax calculation", Assert.Single(described).Title);
    }
}
