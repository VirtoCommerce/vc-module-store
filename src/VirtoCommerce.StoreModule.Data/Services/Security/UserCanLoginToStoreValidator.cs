using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.Model;
using VirtoCommerce.Platform.Security.Services;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.StoreModule.Data.Services.Security
{
    public class UserCanLoginToStoreValidator : IUserSignInValidator
    {
        public int Priority { get; set; } = 1;

        private readonly IStoreService _storeService;

        public UserCanLoginToStoreValidator(IStoreService storeService)
        {
            _storeService = storeService;
        }

        public async Task<IList<TokenLoginResponse>> ValidateUserAsync(ApplicationUser user, SignInResult signInResult, IDictionary<string, object> context)
        {
            var result = new List<TokenLoginResponse>();

            var store = default(Store);
            if (context.TryGetValue("storeId", out var storeIdValue) && storeIdValue is string storeId)
            {
                store = await _storeService.GetNoCloneAsync(storeId);
                if (store == null)
                {
                    result.Add(StoreSecurityErrorDescriber.UserCannotLoginInStore());
                    return result;
                }
            }

            if (signInResult.Succeeded)
            {
                //Allow to login to store for administrators or for users not assigned to store
                var canLoginToStore = user.IsAdministrator || user.StoreId.IsNullOrEmpty();
                if (!canLoginToStore)
                {
                    canLoginToStore = store.TrustedGroups.Concat(new[] { store.Id }).Contains(user.StoreId);
                }

                if (!canLoginToStore)
                {
                    result.Add(StoreSecurityErrorDescriber.UserCannotLoginInStore());
                }
            }

            return result;
        }
    }
}
