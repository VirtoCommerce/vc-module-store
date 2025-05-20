using System.Collections.Generic;
using VirtoCommerce.Seo.Core.Extensions;
using VirtoCommerce.Seo.Core.Models;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.Extensions;

public static class SeoExtensions
{
    /// <summary>
    /// Returns SEO record with the highest score
    /// </summary>
    public static SeoInfo GetBestMatchingSeoInfo(this ISeoSupport seoSupport, Store store, string language,
        string slug = null, string permalink = null)
    {
        return seoSupport?.SeoInfos?.GetBestMatchingSeoInfo(store, language, slug, permalink);
    }

    /// <summary>
    /// Returns SEO record with the highest score
    /// </summary>
    public static SeoInfo GetBestMatchingSeoInfo(this IEnumerable<SeoInfo> seoInfos, Store store, string language,
        string slug = null, string permalink = null)
    {
        return seoInfos?.GetBestMatchingSeoInfo(store?.Id, store?.DefaultLanguage, language ?? store?.DefaultLanguage,
            slug, permalink);
    }
}
