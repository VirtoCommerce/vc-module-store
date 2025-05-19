using System;
//using System.Linq;
//using VirtoCommerce.Platform.Core.Common;
//using VirtoCommerce.Seo.Core.Models;
//using VirtoCommerce.StoreModule.Core.Model;

//namespace VirtoCommerce.StoreModule.Core.Extensions;

//public static class SeoExtensions
//{
//    /// <summary>
//    /// Returns SEO record with the highest score
//    /// </summary>
//    public static SeoInfo GetBestMatchingSeoInfo(this ISeoSupport seoSupport, Store store, string language, string slug = null, string permalink = null)
//    {
//        return seoSupport?.SeoInfos?.GetBestMatchingSeoInfo(store, language, slug, permalink);
//    }

//    /// <summary>
//    /// Returns SEO record with the highest score
//    /// </summary>
//    public static SeoInfo GetBestMatchingSeoInfo(this IEnumerable<SeoInfo> seoInfos, Store store, string language, string slug = null, string permalink = null)
//    {
//        return seoInfos?.GetBestMatchingSeoInfo(store?.Id, store?.DefaultLanguage, language ?? store?.DefaultLanguage, slug, permalink);
//    }

//    /// <summary>
//    /// Returns SEO record with the highest score
//    /// </summary>
//    public static SeoInfo GetBestMatchingSeoInfo(this ISeoSupport seoSupport, string storeId, string storeDefaultLanguage, string language, string slug = null, string permalink = null)
//    {
//        return seoSupport?.SeoInfos?.GetBestMatchingSeoInfo(storeId, storeDefaultLanguage, language, slug, permalink);
//    }

    // unknown object types should have the lowest priority
    // so, the array should be reversed to have the lowest priority at the end
    // todo: should be moved to settings and has reverse order
    private static readonly string[] PrioritiesSettings =
    [
        "CatalogProduct",
        "Category",
        "Catalog",
        "Pages",
        "ContentFile"
    ];

        // this is impossible situation
        if (storeId.IsNullOrEmpty() || storeDefaultLanguage.IsNullOrEmpty())

        var priorities = PrioritiesSettings; // .Reverse().ToArray();

        return seoInfos
            ?.Where(x => SeoCanBeFound(x, storeId, storeDefaultLanguage, language, slug, permalink))
            .Select(seoInfo => new
//            })
//            .OrderByDescending(x => x.Score)
                ObjectTypePriority = Array.IndexOf(priorities, seoInfo.ObjectType),
//            .FirstOrDefault();
            .Where(x => x.Score > 0)
            .ThenByDescending(x => x.ObjectTypePriority)
//    }

//    private static int CalculateScore(this SeoInfo seoInfo, string storeId, string storeDefaultLanguage, string language, string slug, string permalink)
//    {
        // the order of this array is important
        // the first element has the highest priority
        // the array is reversed below using the .Reverse() method to prioritize elements correctly
//                seoInfo.SemanticUrl.EqualsWithoutSlash(permalink),
//                seoInfo.SemanticUrl.EqualsWithoutSlash(slug),
//                seoInfo.StoreId.EqualsIgnoreCase(storeId),
//                seoInfo.LanguageCode.EqualsIgnoreCase(language),
//                seoInfo.LanguageCode.EqualsIgnoreCase(storeDefaultLanguage),
//                seoInfo.LanguageCode.IsNullOrEmpty(),
//            }
//            .Reverse()
//            .Select((valid, index) => valid ? 1 << index : 0)
//            .Sum();

        // the example of the score calculation:
        // seoInfo = { IsActive = true, SemanticUrl = "blog/article", StoreId = "Store", LanguageCode = null }
        // method parameters are: storeId = "Store", storeDefaultLanguage = "en-US", language = "en-US", slug = null, permalink = "blog/article"
        // result array is: [true, true, false, true, false, false, true]
        // it transforms into binary: 1101001b = 105d

    }

    private static bool SeoCanBeFound(SeoInfo seoInfo, string storeId, string storeDefaultLanguage, string language, string slug, string permalink)
    {
        // some conditions should be checked before calculating the score 
        return (seoInfo.StoreId.IsNullOrEmpty() || seoInfo.StoreId == storeId)
                && (seoInfo.SemanticUrl.EqualsWithoutSlash(permalink) || seoInfo.SemanticUrl.EqualsWithoutSlash(slug))
                && (seoInfo.LanguageCode.IsNullOrEmpty() || seoInfo.LanguageCode.EqualsIgnoreCase(language) || seoInfo.LanguageCode.EqualsIgnoreCase(storeDefaultLanguage));

//    private static bool EqualsWithoutSlash(this string a, string b)
//    {
//        return a.TrimStart('/').EqualsIgnoreCase(b?.TrimStart('/'));
//    }
//}
