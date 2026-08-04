using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.StoreModule.Data.Jobs
{
    /// <summary>
    /// Sends the store-branded email verification notification, off the request thread.
    /// </summary>
    public class SendUserEmailVerificationJobHandler(IStoreNotificationSender storeNotificationSender)
        : IBackgroundJobHandler<SendUserEmailVerificationJobPayload>
    {
        public virtual async Task Execute(SendUserEmailVerificationJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            var user = payload.User;

            if (!user.StoreId.IsNullOrEmpty())
            {
                await storeNotificationSender.SendUserEmailVerificationAsync(user);
            }
        }
    }
}
