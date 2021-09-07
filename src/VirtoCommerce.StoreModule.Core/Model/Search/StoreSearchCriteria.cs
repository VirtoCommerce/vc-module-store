using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.StoreModule.Core.Model.Search
{
    public class StoreSearchCriteria : SearchCriteriaBase
    {
        public string[] StoreIds
        {
            get
            {
                return ObjectIds?.ToArray() ?? new string[0];
            }
            set
            {
                ObjectIds = new List<string>(value);
            }
        }

        public StoreState[] StoreStates { get; set; }
        public string[] FulfillmentCenterIds { get; set; }
    }
}
