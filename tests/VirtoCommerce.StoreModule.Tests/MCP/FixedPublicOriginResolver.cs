using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.MCP.Core.Services;

namespace VirtoCommerce.StoreModule.Tests.MCP;

// The platform's public origin, as the MCP module resolves it from MCP:PublicOrigin.
public class FixedPublicOriginResolver : IMcpPublicOriginResolver
{
    public const string Origin = "https://admin.example.com";

    public Task<string> GetOriginAsync(HttpContext context, McpScope scope)
    {
        return Task.FromResult(Origin);
    }

    public bool IsOriginConfigured(HttpContext context, McpScope scope)
    {
        return true;
    }
}
