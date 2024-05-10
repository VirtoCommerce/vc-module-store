using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.StoreModule.Core.Model.Search
{
    public class StoreSearchCriteria : SearchCriteriaBase
    {
        [Obsolete("Please use ObjectIds property to filter by store ids")]
        public string[] StoreIds
        {
            get
            {
#pragma warning disable S2365 
                return ObjectIds?.ToArray() ?? Array.Empty<string>();
#pragma warning restore S2365
            }
            set
            {
                ObjectIds = new List<string>(value);
            }
        }

        public StoreState[] StoreStates { get; set; }
        public string[] FulfillmentCenterIds { get; set; }

        public string Domain { get; set; }
    }
}
