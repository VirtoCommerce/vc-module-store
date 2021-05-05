using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using VirtoCommerce.NotificationsModule.Core.Extensions;
using VirtoCommerce.NotificationsModule.Core.Model;
using VirtoCommerce.NotificationsModule.Core.Services;
using VirtoCommerce.NotificationsModule.Core.Types;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using StoreSettings = VirtoCommerce.StoreModule.Core.ModuleConstants.Settings;

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

        public virtual async Task<NotificationSendResult> SendUserEmailVerificationAsync(ApplicationUser user)
        {
            // Return error if store not found
            var store = await _storeService.GetByIdAsync(user.StoreId, StoreResponseGroup.None.ToString());
            if (store is null) return new NotificationSendResult
            {
                ErrorMessage = $"Can't find store for user {user.UserName}."
            };

            // Return error if email verification feature is disabled
            var settingDescriptor = StoreSettings.General.EmailVerificationEnabled;
            var isVerificationEnabled = store.Settings.GetSettingValue(settingDescriptor.Name, (bool)settingDescriptor.DefaultValue);
            if (!isVerificationEnabled) return new NotificationSendResult
            {
                ErrorMessage = $"Email verification sending disabled. Check {settingDescriptor.Name} setting for store {store.Id}."
            };

            // Return error if notification is not registered
            var notification = await _notificationSearchService.GetNotificationAsync<ConfirmationEmailNotification>(new TenantIdentity(store.Id, nameof(Store)));
            if (notification is null) return new NotificationSendResult
            {
                ErrorMessage = $"Can't find {nameof(ConfirmationEmailNotification)} notification for store {store.Id}."
            };

            notification.Url = await GenerateEmailVerificationLink(user, store);
            notification.From = store.AdminEmailWithName ?? store.EmailWithName;
            notification.To = user.Email;

            return await _notificationSender.SendNotificationAsync(notification);
        }

        public virtual async Task<string> GenerateEmailVerificationLink(ApplicationUser user, Store store)
        {
            if (!store.Url.IsNullOrEmpty() && Uri.IsWellFormedUriString(store.Url, UriKind.Absolute))
            {
                using var userManager = _userManagerFactory();
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                return QueryHelpers.AddQueryString(new Uri($"{store.Url.TrimEnd('/')}/account/confirmemail").ToString(),
                     new Dictionary<string, string>
                     {
                        { "UserId", user.Id },
                        { "Token", token }
                    });
            }

            throw new OperationCanceledException($"A valid URL is required in Url property for store '{store.Id}'.");
        }
    }
}
