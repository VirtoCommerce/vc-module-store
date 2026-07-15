namespace VirtoCommerce.StoreModule.Core;

/// <summary>
/// Options for store-aware asset URL resolution. Bound from the "VirtoCommerce:StoreAssets" configuration section.
/// </summary>
public class StoreAssetsOptions
{
    public const string SectionName = "VirtoCommerce:StoreAssets";

    /// <summary>
    /// Hosts (domains) of platform/CDN asset URLs that are allowed to be replaced with the store's
    /// AssetPublicUrl. When empty (default), any absolute asset URL is rebased. When set, only URLs
    /// whose host is listed here are rebased; other hosts are treated as external and returned as-is.
    /// </summary>
    public string[] KnownAssetHosts { get; set; } = [];
}
