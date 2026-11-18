namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

public enum StoreSetupPartStatus
{
    NoChange,
    WillChange,
    Applied,
    Failed,
    // Not started, because a part before it failed.
    NotApplied,
}
