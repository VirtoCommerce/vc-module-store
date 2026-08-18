using VirtoCommerce.Platform.Core.ChangeLog;

namespace VirtoCommerce.StoreModule.Data.Jobs
{
    /// <summary>
    /// Payload of the background job that persists store change-log entries.
    /// </summary>
    public class LogEntityChangesJobPayload
    {
        public OperationLog[] OperationLogs { get; set; }
    }
}
