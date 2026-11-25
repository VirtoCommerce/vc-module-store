using System.Text.Json.Serialization;

namespace VirtoCommerce.StoreModule.MCP.Models;

// A store setting whose values differ, as compare_store_settings returns it. Both values are written, null too, so a
// value that is not set is not read as one the answer leaves out.
public class StoreSettingDifference
{
    public string Name { get; set; }

    public string Title { get; set; }

    public string ModuleId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public object Value { get; set; }

    // The other store's value, or the default when the store is compared with the defaults.
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public object OtherValue { get; set; }
}
