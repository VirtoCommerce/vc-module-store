using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.MCP.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

// Store's settings rules in short: a name ending with Key is a secret.
internal class KeySecretSettings() : StoreSetupSettingsBase(new McpSetupValues())
{
    public override bool IsSecret(SettingDescriptor descriptor)
    {
        return descriptor.Name.EndsWithIgnoreCase("Key");
    }

    public override McpSetupSetting Describe(SettingDescriptor descriptor, object value)
    {
        return new McpSetupSetting { Name = descriptor.Name, DefaultValue = descriptor.DefaultValue, Value = value ?? descriptor.DefaultValue };
    }

    public override bool TryConvert(SettingDescriptor descriptor, object raw, out object value, out string error)
    {
        value = raw ?? descriptor.DefaultValue;
        error = null;
        return true;
    }

    public override Task<IList<McpSetupSetting>> GetStoreSettingsAsync(Store store)
    {
        return Task.FromResult<IList<McpSetupSetting>>([]);
    }
}
