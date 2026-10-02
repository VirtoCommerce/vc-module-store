using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.StoreModule.Data.Jobs
{
    /// <summary>
    /// Payload of the background job that sends the store-branded email verification notification.
    /// </summary>
    public class SendUserEmailVerificationJobPayload
    {
        /// <summary>
        /// The user to notify. Carried by value rather than by id, as the Hangfire job this replaces did: the event
        /// fires with the user instance at hand, and re-reading it would race with whatever is still writing it.
        /// </summary>
        public ApplicationUser User { get; set; }
    }
}
