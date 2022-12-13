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


        public virtual async Task SendUserEmailVerificationInBackground(ApplicationUser user)
        {
            if (!user.StoreId.IsNullOrEmpty())
            {
                await _storeNotificationSender.SendUserEmailVerificationAsync(user);
            }
        }
    }
}
