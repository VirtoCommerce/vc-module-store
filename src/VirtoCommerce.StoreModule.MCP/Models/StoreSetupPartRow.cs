namespace VirtoCommerce.StoreModule.MCP.Models;

public class StoreSetupPartRow
{
    public string Part { get; set; }

    // no_change, will_change, applied, failed or not_applied.
    public string Status { get; set; }

    public int Changes { get; set; }
}
