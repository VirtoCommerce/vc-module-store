using System;
using System.Collections.Generic;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// The settings known to be secrets, whatever their name and type. Filled with AddStoreSetupSecrets.
public class StoreSetupSecretOptions
{
    public ISet<string> SettingNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}
