using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.StoreModule.Core.Model.Search
{
    public class StoreSearchCriteria : SearchCriteriaBase
    {
        public StoreState[] StoreStates { get; set; }
        public string[] FulfillmentCenterIds { get; set; }

        public string Domain { get; set; }
    }
}
