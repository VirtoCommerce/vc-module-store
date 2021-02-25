using System.Threading.Tasks;
using VirtoCommerce.NotificationsModule.Core.Model;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.Services
{
    public interface IStoreNotificationSender
    {
        Task<string> GenerateEmailVerificationLink(ApplicationUser user, Store store);
        Task<NotificationSendResult> SendUserEmailVerificationAsync(ApplicationUser user);
    }
}
