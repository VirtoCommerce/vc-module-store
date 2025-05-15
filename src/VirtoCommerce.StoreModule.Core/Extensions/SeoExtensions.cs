using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Core.Extensions;

public static class SeoExtensions
{
    /// <summary>
    /// Returns SEO record with the highest score
    /// </summary>
    public static SeoInfo GetBestMatchingSeoInfo(this ISeoSupport seoSupport, Store store, string language, string slug = null, string permalink = null)
    {
        return seoSupport?.SeoInfos?.GetBestMatchingSeoInfo(store, language, slug, permalink);
    }

    /// <summary>
    /// Returns SEO record with the highest score
    /// </summary>
    public static SeoInfo GetBestMatchingSeoInfo(this IEnumerable<SeoInfo> seoInfos, Store store, string language, string slug = null, string permalink = null)
    {
        return seoInfos?.GetBestMatchingSeoInfo(store?.Id, store?.DefaultLanguage, language ?? store?.DefaultLanguage, slug, permalink);
    }

    /// <summary>
    /// Returns SEO record with the highest score
    /// </summary>
    public static SeoInfo GetBestMatchingSeoInfo(this ISeoSupport seoSupport, string storeId, string storeDefaultLanguage, string language, string slug = null, string permalink = null)
    {
        return seoSupport?.SeoInfos?.GetBestMatchingSeoInfo(storeId, storeDefaultLanguage, language, slug, permalink);
    }

    /// <summary>
    /// Returns SEO record with the highest score
    /// </summary>
    public static SeoInfo GetBestMatchingSeoInfo(this IEnumerable<SeoInfo> seoInfos, string storeId, string storeDefaultLanguage, string language, string slug = null, string permalink = null)
    {
        // this is impossible situation
        if (storeId == null || storeDefaultLanguage == null)
        {
            return null;
        }

        // todo: should me moved to settings
        var prioritiesSettings = new[] { "ContentFile", "Pages", "Catalog", "Category", "CatalogProduct" };

        // unknown object types should have the lowest priority
        // so, the array should be reversed to have the lowest priority at the end
        var priorities = prioritiesSettings.Reverse().ToArray();

        return seoInfos
            ?.Select(seoInfo => new
            {
                SeoRecord = seoInfo,
                ObjectTypePriority = Array.IndexOf(priorities, seoInfo.ObjectType),
                Score = seoInfo.CalculateScore(storeId, storeDefaultLanguage, language, slug, permalink),
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.ObjectTypePriority)
            .Select(x => x.SeoRecord)
            .FirstOrDefault();
    }

    private static int CalculateScore(this SeoInfo seoInfo, string storeId, string storeDefaultLanguage, string language, string slug, string permalink)
    {
        // some conditions should be checked before calculating the score 
        if ((!seoInfo.StoreId.IsNullOrEmpty() && seoInfo.StoreId != storeId)
            || (!seoInfo.SemanticUrl.EqualsWithoutSlash(permalink) && !seoInfo.SemanticUrl.EqualsWithoutSlash(slug))
            || (!seoInfo.LanguageCode.IsNullOrEmpty() && !seoInfo.LanguageCode.EqualsIgnoreCase(language) && !seoInfo.LanguageCode.EqualsIgnoreCase(storeDefaultLanguage)))
        {
            return 0;
        }

        // the order of this array is important
        // the first element has the highest priority
        // so, we need to reverse it before calculating the score
        var score = new[]
            {
                seoInfo.IsActive,
                seoInfo.SemanticUrl.EqualsWithoutSlash(permalink),
                seoInfo.SemanticUrl.EqualsWithoutSlash(slug),
                seoInfo.StoreId.EqualsIgnoreCase(storeId),
                seoInfo.LanguageCode.EqualsIgnoreCase(language),
                seoInfo.LanguageCode.EqualsIgnoreCase(storeDefaultLanguage),
                seoInfo.LanguageCode.IsNullOrEmpty(),
            }
            .Reverse()
            .Select((valid, index) => valid ? 1 << index : 0)
            .Sum();

        // the example of the score calculation:
        // seoInfo = { IsActive = true, SemanticUrl = "blog/article", StoreId = "Store", LanguageCode = null }
        // method parameters are: storeId = "Store", storeDefaultLanguage = "en-US", language = "en-US", slug = null, permalink = "blog/article"
        // result array is: [true, true, false, true, false, false, true]
        // it transforms into binary: 1101001b = 105d

        return score;
    }

    private static bool EqualsWithoutSlash(this string a, string b)
    {
        return a.TrimStart('/').EqualsIgnoreCase(b?.TrimStart('/'));
    }
}
