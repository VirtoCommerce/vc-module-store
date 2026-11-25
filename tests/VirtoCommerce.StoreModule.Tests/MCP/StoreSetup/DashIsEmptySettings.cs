namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

// A solution's rule: a dash is no value.
internal sealed class DashIsEmptySettings : KeySecretSettings
{
    protected override bool IsEmpty(object value)
    {
        return base.IsEmpty(value) || value is "-";
    }
}
