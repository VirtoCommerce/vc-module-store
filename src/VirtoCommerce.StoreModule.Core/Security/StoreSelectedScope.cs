using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.StoreModule.Core.Security
{
    /// <summary>
    /// Restricts access rights to a particular store
    /// </summary>
    public sealed class StoreSelectedScope : PermissionScope
    {
        public string StoreId => Scope; 
    }
}
