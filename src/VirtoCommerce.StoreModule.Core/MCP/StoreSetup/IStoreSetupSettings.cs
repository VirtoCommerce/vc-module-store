using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// Settings for a setup: what they mean, which are secrets, and how a value from a request becomes a setting value. Store's
// parts and the parts of other modules (method and provider settings) share it.
public interface IStoreSetupSettings
{
    // A secret by the MCP module's rule (McpSecretSettings): a SecureString, a setting a module names with
    // AddMcpSecretSettings, or a name with the word Key, Secret, Password, Token or Credential(s). A secret is never
    // listed, accepted or returned.
    bool IsSecret(SettingDescriptor descriptor);

    // The setting with its title and description in English. A secret is described without its values.
    McpSetupSetting Describe(SettingDescriptor descriptor, object value);

    // The value a request sends, converted to the setting's type and checked against its allowed values. Refuses a
    // secret and a setting that a setup cannot change: hidden, dictionary and localizable settings are changed in the
    // back office only. A null value returns the setting to its default.
    bool TryConvert(SettingDescriptor descriptor, object raw, out object value, out string error);

    // The store-level settings of every installed module with the store's values, without secrets and hidden settings.
    Task<IList<McpSetupSetting>> GetStoreSettingsAsync(Store store);
}
