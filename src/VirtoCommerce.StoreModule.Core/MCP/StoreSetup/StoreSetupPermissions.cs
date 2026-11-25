using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Security;
using StorePermissions = VirtoCommerce.StoreModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

public static class StoreSetupPermissions
{
    // Store's own permissions, limited to stores by StoreSelectedScope: the permissions of the store screen.
    public static McpSetupPermissions ForStore()
    {
        var permissions = AbstractTypeFactory<McpSetupPermissions>.TryCreateInstance();
        permissions.Read = StorePermissions.Read;
        permissions.Update = StorePermissions.Update;
        permissions.Create = StorePermissions.Create;
        permissions.ScopeType = nameof(StoreSelectedScope);

        return permissions;
    }
}
