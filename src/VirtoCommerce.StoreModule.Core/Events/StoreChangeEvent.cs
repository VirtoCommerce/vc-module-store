using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.Events
{
    public class StoreChangeEvent : GenericChangedEntryEvent<Store>
    {
        public StoreChangeEvent(IEnumerable<GenericChangedEntry<Store>> changedEntries) : base(changedEntries)
        {
        }
    }
}
