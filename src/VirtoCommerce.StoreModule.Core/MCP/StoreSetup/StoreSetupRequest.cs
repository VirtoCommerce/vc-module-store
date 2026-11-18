using System;
using System.Collections.Generic;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

public class StoreSetupRequest
{
    public string StoreId { get; set; }

    // A new store: it must not exist yet, and the parts required for a new store must have a section.
    public bool IsNewStore { get; set; }

    // The sections by part name, each an instance of its part's SectionType. A section that is left out changes nothing.
    public IDictionary<string, object> Sections { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
}
