using System.Collections.Generic;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// A setting described for a person: what it means, its default and its value. Never a secret.
public class StoreSetupSetting
{
    public string Name { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public string ModuleId { get; set; }

    // The name of the SettingValueType: Boolean, ShortText, Integer and so on.
    public string ValueType { get; set; }

    public object DefaultValue { get; set; }

    public object Value { get; set; }

    public IList<object> AllowedValues { get; set; }

    public bool IsRequired { get; set; }

    // Whether a setup can set it: hidden, dictionary and localizable settings are changed in the back office only.
    public bool IsChangeable { get; set; }

    // Whether the value differs from the default.
    public bool IsChanged { get; set; }
}
