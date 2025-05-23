using System;
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
    // unknown object types should have the lowest priority
    // so, the array should be reversed to have the lowest priority at the end
    private static readonly string[] _orderedObjectTypes =
    [
        "CatalogProduct",
        "Category",
        "Catalog",
        "Pages",
        "ContentFile"
    ];

            .Select(seoInfo => new
                ObjectTypePriority = Array.IndexOf(_orderedObjectTypes, seoInfo.ObjectType),
            .Where(x => x.Score > 0)
            .ThenByDescending(x => x.ObjectTypePriority)
    private static bool SeoCanBeFound(SeoInfo seoInfo, string storeId, string storeDefaultLanguage, string language, string slug, string permalink)
    {
        // some conditions should be checked before calculating the score 
        return (seoInfo.StoreId.IsNullOrEmpty() || seoInfo.StoreId == storeId) &&
               (seoInfo.SemanticUrl.EqualsWithoutSlash(permalink) || seoInfo.SemanticUrl.EqualsWithoutSlash(slug)) &&
               (seoInfo.LanguageCode.IsNullOrEmpty() || seoInfo.LanguageCode.EqualsIgnoreCase(language) || seoInfo.LanguageCode.EqualsIgnoreCase(storeDefaultLanguage));
    }
        // the order of this array is important
        // the first element has the highest priority
        // the array is reversed below using the .Reverse() method to prioritize elements correctly

        // the example of the score calculation:
        // seoInfo = { IsActive = true, SemanticUrl = "blog/article", StoreId = "Store", LanguageCode = null }
        // method parameters are: storeId = "Store", storeDefaultLanguage = "en-US", language = "en-US", slug = null, permalink = "blog/article"
        // result array is: [true, true, false, true, false, false, true]
        // it transforms into binary: 1101001b = 105d
    {
        return seoInfos?.GetBestMatchingSeoInfo(store?.Id, store?.DefaultLanguage, language ?? store?.DefaultLanguage,
            slug, permalink);
    }
}
