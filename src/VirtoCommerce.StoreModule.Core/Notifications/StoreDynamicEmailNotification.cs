using System.Collections.Generic;
using VirtoCommerce.NotificationsModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.Notifications
{
    public class StoreDynamicEmailNotification : EmailNotification
    {
        public StoreDynamicEmailNotification()
            : base(nameof(StoreDynamicEmailNotification))
        {
        }

        public string FormType { get; set; }

        public IDictionary<string, string> Fields { get; set; }
    }
}
