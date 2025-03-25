using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.Extensions;

public static class StoreExtensions
{
    public static string GetSeoLinksType(this Store store)
    {
        return store.Settings?.GetValue<string>(ModuleConstants.Settings.SEO.SeoLinksType);
    }
}
