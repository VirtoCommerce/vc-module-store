using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Services;

namespace VirtoCommerce.StoreModule.MCP.Services;

// The links of Store's screens, for what the tools do not cover.
public class StoreBackOfficeLinks(IMcpLinkResolver linkResolver)
{
    // The route of the Stores screen. It takes no store id, so the link opens the list of stores.
    public const string StoresRoute = "/#!/workspace/store";

    // The link to the Stores screen; null outside an MCP request and when the scope has no public origin.
    public virtual Task<string> GetStoresLinkAsync()
    {
        return linkResolver.GetLinkAsync(StoresRoute);
    }
}
