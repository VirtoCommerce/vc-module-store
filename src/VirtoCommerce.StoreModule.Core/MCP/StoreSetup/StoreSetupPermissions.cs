using VirtoCommerce.StoreModule.Core.Security;
using StorePermissions = VirtoCommerce.StoreModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// The existing permissions that guard a part.
public class StoreSetupPermissions
{
    public string Read { get; set; }

    public string Update { get; set; }

    // Sets the part up on a store that is being created.
    public string Create { get; set; }

    // The permission scope that limits Read and Update to stores, such as StoreSelectedScope; null when the module's
    // permissions cannot be limited to stores.
    public string StoreScopeType { get; set; }

    // Store's own permissions, limited to stores by StoreSelectedScope: the permissions of the store screen.
    public static StoreSetupPermissions ForStore()
    {
        return new StoreSetupPermissions
        {
            Read = StorePermissions.Read,
            Update = StorePermissions.Update,
            Create = StorePermissions.Create,
            StoreScopeType = nameof(StoreSelectedScope),
        };
    }
}
