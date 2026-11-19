using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.Core.Security;
using StorePermissions = VirtoCommerce.StoreModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

public static class StoreSetupPermissions
{
    // Store's own permissions, limited to stores by StoreSelectedScope: the permissions of the store screen.
    public static McpSetupPermissions ForStore()
    {
        return new McpSetupPermissions
        {
            Read = StorePermissions.Read,
            Update = StorePermissions.Update,
            Create = StorePermissions.Create,
            ScopeType = nameof(StoreSelectedScope),
        };
    }
}
