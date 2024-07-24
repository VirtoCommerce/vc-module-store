using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.Events;

public class StoreAuthenticationSchemeChangedEvent : GenericChangedEntryEvent<StoreAuthenticationScheme>
{
    public StoreAuthenticationSchemeChangedEvent(IEnumerable<GenericChangedEntry<StoreAuthenticationScheme>> changedEntries)
        : base(changedEntries)
    {
    }
}
