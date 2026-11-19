using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// The store-level settings of a setup.
public class StoreSettingsSection
{
    [Description("Setting name to value. Sets the named settings and leaves the others as they are; null returns a " +
        "setting to its default, and a setting without a default takes a value instead. get_store_settings lists the " +
        "names. Secrets are entered in the back office.")]
    public IDictionary<string, object> Values { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
}
