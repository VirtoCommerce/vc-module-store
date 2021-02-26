using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Security.Events;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.StoreModule.Data.Handlers
{
    public class SendStoreUserVerificationEmailHandler : IEventHandler<UserVerificationEmailEvent>
    {
        private readonly IStoreNotificationSender _storeNotificationSender;

        public SendStoreUserVerificationEmailHandler(IStoreNotificationSender storeNotificationSender)
        {
            _storeNotificationSender = storeNotificationSender;
        }


        public virtual Task Handle(UserVerificationEmailEvent message)
        {
            BackgroundJob.Enqueue(() => SendUserEmailVerificationInBackground(message.ApplicationUser));

            return Task.CompletedTask;
        }


        [DisableConcurrentExecution(10)]
        // "DisableConcurrentExecutionAttribute" prevents to start simultaneous job payloads.
        // Should have short timeout, because this attribute implemented by following manner: newly started job falls into "processing" state immediately.
        // Then it tries to receive job lock during timeout. If the lock received, the job starts payload.
        // When the job is awaiting desired timeout for lock release, it stucks in "processing" anyway. (Therefore, you should not to set long timeouts (like 24*60*60), this will cause a lot of stucked jobs and performance degradation.)
        // Then, if timeout is over and the lock NOT acquired, the job falls into "scheduled" state (this is default fail-retry scenario).
        // Failed job goes to "Failed" state (by default) after retries exhausted.
        public virtual async Task SendUserEmailVerificationInBackground(ApplicationUser user)
        {
            if (!user.StoreId.IsNullOrEmpty())
            {
                await _storeNotificationSender.SendUserEmailVerificationAsync(user);
            }
        }
    }
}
