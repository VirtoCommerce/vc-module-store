using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.NotificationsModule.Core.Extensions;
using VirtoCommerce.NotificationsModule.Core.Model;
using VirtoCommerce.NotificationsModule.Core.Services;
using VirtoCommerce.NotificationsModule.Core.Types;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.StoreModule.Data.Services
{
    public class StoreNotificationSender : IStoreNotificationSender
    {
        private readonly INotificationSearchService _notificationSearchService;
        private readonly INotificationSender _notificationSender;
        private readonly IStoreService _storeService;
        private readonly Func<UserManager<ApplicationUser>> _userManagerFactory;

        public StoreNotificationSender(INotificationSearchService notificationSearchService, INotificationSender notificationSender, IStoreService storeService, Func<UserManager<ApplicationUser>> userManagerFactory)
        {
            _notificationSearchService = notificationSearchService;
            _notificationSender = notificationSender;
            _storeService = storeService;
            _userManagerFactory = userManagerFactory;
        }


        public async Task<NotificationSendResult> SendUserEmailVerificationAsync(ApplicationUser user)
        {
            var result = new NotificationSendResult();

            var store = await _storeService.GetByIdAsync(user.StoreId, StoreResponseGroup.None.ToString());

            if (store != null)
            {
                var isVerificationEnabled = store.Settings.GetSettingValue(ModuleConstants.Settings.General.EmailVerificationEnabled.Name,
                                                                     (bool)ModuleConstants.Settings.General.EmailVerificationEnabled.DefaultValue);
                if (isVerificationEnabled)
                {
                    var notification = await _notificationSearchService.GetNotificationAsync<ConfirmationEmailNotification>(new TenantIdentity(store.Id, nameof(Store)));
                    if (notification != null)
                    {
                        notification.Url = await GenerateEmailVerificationLink(user, store);
                        notification.From = store.AdminEmail ?? store.Email;
                        notification.To = user.Email;

                        result = await _notificationSender.SendNotificationAsync(notification);
                    }
                    else
                    {
                        result.ErrorMessage = $"Can't find {nameof(ConfirmationEmailNotification)} notification for store {store.Id}.";
                    }
                }
                else
                {
                    result.ErrorMessage = $"Email verification sending disabled. Check {ModuleConstants.Settings.General.EmailVerificationEnabled.Name} setting for store {store.Id}.";
                }
            }
            else
            {
                result.ErrorMessage = $"Can't find store for user {user.UserName}.";
            }

            return result;
        }


        public async Task<string> GenerateEmailVerificationLink(ApplicationUser user, Store store)
        {
            if (store.Url.IsNullOrEmpty())
            {
                throw new InvalidOperationException($"Required Url property is missing for store '{store.Id}'.");
            }

            using var userManager = _userManagerFactory();
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmationLink = $"{store.Url.TrimEnd('/')}/account/confirmemail?token={token}&userId={user.Id}";
            return confirmationLink;
        }
    }
}
