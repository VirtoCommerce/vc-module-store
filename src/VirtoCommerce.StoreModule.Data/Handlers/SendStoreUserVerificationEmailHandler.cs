using System;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Security.Events;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.Data.Jobs;

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
            var payload = AbstractTypeFactory<SendUserEmailVerificationJobPayload>.TryCreateInstance();
            payload.User = message.ApplicationUser;

            // The static facade, not an injected IBackgroundJob: RegisterEventHandler resolves this handler once from
            // the root provider and holds it for the process lifetime, so it must not capture a Scoped dependency.
            return BackgroundJob.Enqueue<SendUserEmailVerificationJobHandler>(payload);
        }


        /// <summary>
        /// Kept for background jobs enqueued by an earlier version, which reference this method by name.
        /// New work goes through <see cref="SendUserEmailVerificationJobHandler"/>; remove this once no such job
        /// can still be pending.
        /// </summary>
        // Signature is byte-identical on purpose: Hangfire persists a queued job as type name + method name +
        // parameter types + serialized args, so changing any of them would strand already-queued jobs as Failed.
        [Obsolete("Enqueued indirectly by legacy Hangfire jobs only; new work uses SendUserEmailVerificationJobHandler.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
        public virtual async Task SendUserEmailVerificationInBackground(ApplicationUser user)
        {
            if (!user.StoreId.IsNullOrEmpty())
            {
                await _storeNotificationSender.SendUserEmailVerificationAsync(user);
            }
        }
    }
}
