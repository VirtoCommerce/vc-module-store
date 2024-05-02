using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.StoreModule.Core.Model.Search
{
    public class StoreSearchResult : GenericSearchResult<Store>
    {
        [Obsolete("Use Results instead of Stores", DiagnosticId = "VC0008", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        public IList<Store> Stores => Results;
    }
}
