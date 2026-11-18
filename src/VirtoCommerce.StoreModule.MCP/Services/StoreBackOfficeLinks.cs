using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VirtoCommerce.MCP.Core.Extensions;
using VirtoCommerce.MCP.Core.Services;

namespace VirtoCommerce.StoreModule.MCP.Services;

// Links into the back office for what the tools do not cover: the platform's public origin, as the MCP module resolves
// it for the scope of the request, followed by the screen's route.
public class StoreBackOfficeLinks(IHttpContextAccessor httpContextAccessor)
{
    // The route of the Stores screen. It takes no store id, so the link opens the list of stores.
    public const string StoresRoute = "/#!/workspace/store";

    // The link to the Stores screen, or null outside an MCP request.
    public virtual async Task<string> GetStoresLinkAsync()
    {
        var httpContext = httpContextAccessor.HttpContext;
        var scope = httpContext?.GetMcpScope();

        if (scope == null)
        {
            return null;
        }

        var origin = await httpContext.RequestServices.GetMcpScopeService<IMcpPublicOriginResolver>(scope).GetOriginAsync(httpContext, scope);

        return string.IsNullOrEmpty(origin) ? null : $"{origin.TrimEnd('/')}{StoresRoute}";
    }
}
