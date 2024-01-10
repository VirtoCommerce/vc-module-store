using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Security.Model;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace VirtoCommerce.StoreModule.Data.Services.Security
{
    public static class StoreSecurityErrorDescriber
    {
        public static TokenLoginResponse UserCannotLoginInStore() => new()
        {
            Error = Errors.InvalidGrant,
            Code = nameof(UserCannotLoginInStore).PascalToKebabCase(),
            ErrorDescription = "Access denied. You cannot sign in to the current store"
        };
    }
}
