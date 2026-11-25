using VirtoCommerce.MCP.Testing;
using VirtoCommerce.StoreModule.MCP;
using Xunit;

namespace VirtoCommerce.StoreModule.Tests.MCP;

public class StoreToolsetConventionsTests
{
    [Fact]
    public void Toolset_FollowsTheMcpConventions()
    {
        McpToolsetConventions.Verify<McpAssemblyMarker>();
    }
}
