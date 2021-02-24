using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.NotificationsModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.StoreModule.Data.Handlers
{
    public class SendUserEmailVerificationEventHandler : IEventHandler<UserChangedEvent>
    {
        private readonly INotificationSearchService _notificationSearchService;
        private readonly INotificationSender _notificationSender;
        private readonly IStoreService _storeService;

        public SendUserEmailVerificationEventHandler(INotificationSearchService notificationSearchService,
            INotificationSender notificationSender,
            IStoreService storeService)
        {
            _notificationSearchService = notificationSearchService;
            _notificationSender = notificationSender;
            _storeService = storeService;
        }

        public virtual Task Handle(UserChangedEvent message)
        {
            BackgroundJob.Enqueue(() => ScheduleSendingUserEmailVerificationBackgroundJob(message));

            return Task.CompletedTask;
        }

        public virtual async Task ScheduleSendingUserEmailVerificationBackgroundJob(UserChangedEvent message)
        {
            var storeIdByUserIds = new Dictionary<string, List<string>>();

            foreach (var changedEntry in message.ChangedEntries)
            {
                if (changedEntry.EntryState == EntryState.Added &&
                    !string.IsNullOrEmpty(changedEntry.NewEntry.StoreId))
                {
                    if (!storeIdByUserIds.ContainsKey(changedEntry.NewEntry.StoreId))
                    {
                        storeIdByUserIds.Add(changedEntry.NewEntry.StoreId, new List<string>());
                    }

                    storeIdByUserIds[changedEntry.NewEntry.StoreId].Add(changedEntry.NewEntry.Id);
                }
            }

            var stores = await _storeService.GetByIdsAsync(storeIdByUserIds.Keys.ToArray(), StoreResponseGroup.StoreInfo.ToString());

            foreach (var store in stores)
            {
                if (store.Settings.GetSettingValue(ModuleConstants.Settings.General.EmailVerificationEnabled.Name, false))
                {
                    var userIds = storeIdByUserIds[store.Id];

                    // get notification
                    // get verification link
                    // schedule an email for each user
                }
            }
        }
    }
}
