using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// Settings for a setup: what they mean, which are secrets, how a value from a request becomes a setting value, and the
// rules every part follows for an object that has settings, such as a payment method or the store itself. Store's parts
// and the parts of other modules share it; a solution changes a rule for all of them by registering its own subclass of
// StoreSetupSettings. Secrets are never described, shown or accepted, so an agent never proposes to send one. Nor is a
// setting the platform's configuration sets for an object: a value forced by VirtoCommerce:Settings:Override:CurrentValue,
// or a fixed setting, which the settings manager reads as read-only and its save keeps whatever is sent.
public interface IStoreSetupSettings
{
    // A secret by the MCP module's rule (IMcpSecretSettings), with the settings modules name with AddMcpSecretSettings. A
    // secret is never listed, accepted or returned.
    bool IsSecret(SettingDescriptor descriptor);

    // The setting with its title and description in English. A secret is described without its values.
    McpSetupSetting Describe(SettingDescriptor descriptor, object value);

    // The value a request sends, converted to the setting's type and checked against its allowed values. Refuses a
    // secret and a setting that a setup cannot change: hidden, dictionary and localizable settings are changed in the
    // back office only. A null value returns the setting to its default; a setting without a default and without allowed
    // values refuses it. An empty text is refused for a text setting with a default text: the platform keeps no empty
    // text, so the default would show instead.
    bool TryConvert(SettingDescriptor descriptor, object raw, out object value, out string error);

    // The store-level settings of every installed module with the store's values and defaults in effect, without secrets,
    // hidden settings and the settings the platform's configuration sets for the store.
    Task<IList<McpSetupSetting>> GetStoreSettingsAsync(Store store);

    // The settings an object takes, described for a choice, without secrets and the settings the platform's configuration
    // sets for every object: a setting it sets for one object only is still described, and refused for that object.
    IList<McpSetupSetting> DescribeAll(IEnumerable<SettingDescriptor> descriptors);

    // The object's values in effect, for a part's state, without secrets and the settings the platform's configuration sets
    // for the object.
    IDictionary<string, object> GetValues(IHasSettings owner, IEnumerable<SettingDescriptor> descriptors);

    // The value in effect, one the platform's configuration sets included: the saved one, or the default of a setting
    // never saved, which is the entry's default when it has one, such as a configured default
    // (VirtoCommerce:Settings:Override:DefaultValue) that the settings manager sets.
    object GetValue(IHasSettings owner, SettingDescriptor descriptor);

    // Plans a settings map on an object, as every part's section takes one:
    // - a name the object does not take, a setting the platform's configuration sets for the object, or a value
    //   TryConvert refuses (a secret, among others), is an error;
    // - a null returns the setting to its default, a configured default included: TryConvert turns it into the default
    //   value, which is then saved itself, because the platform's settings save skips a null for most settings; the
    //   object keeps today's default. A setting without a default, and without allowed values, refuses a null;
    // - a value equal to the one in effect changes nothing.
    // The object carries the planned values, and each change is a row of the plan whose field is the prefix and the
    // setting's name. Returns the changed settings with their new values.
    IDictionary<string, object> PlanSettings(McpSetupPartPlan plan, IHasSettings owner, IList<SettingDescriptor> descriptors,
        IDictionary<string, object> values, string fieldPrefix, string ownerTitle, string listedBy);

    // The required settings of an object that have no value in effect, as a finding names them: the names of those that
    // are not secrets, and how many secrets, which are never named. A value the platform's configuration sets counts, as
    // customers get it.
    (IList<string> Names, int SecretCount) GetEmptyRequiredSettings(IHasSettings owner, IEnumerable<SettingDescriptor> descriptors);

    // Applying a plan to an object read again: the settings named here take their planned values, and the others keep the
    // values the object has now, so a change made since the plan stays.
    void CopySettings(IHasSettings planned, IHasSettings target, ICollection<string> names);
}
