using System.Collections.Generic;
using VirtoCommerce.Seo.Core.Models;
using VirtoCommerce.StoreModule.Core.Extensions;
using VirtoCommerce.StoreModule.Core.Model;
using Xunit;

namespace VirtoCommerce.StoreModule.Tests;

public class SeoExtensionsTests
{
    [Fact]
    public void GetSeoInfos_UseDefaultLanguage_WhenRequestWithEmpty()
    {
        var store = new Store { Id = "Store1", DefaultLanguage = "de-DE" };
        var seoInfos = new List<SeoInfo>
        {
            new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1" },
            new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product2" },
            new() { StoreId = "Store1", LanguageCode = "de-DE", SemanticUrl = "product1" },
            new() { StoreId = "Store1", LanguageCode = "de-DE", SemanticUrl = "product2" },
        };

        // Act
        var result = seoInfos.GetBestMatchingSeoInfo(store: store, language: null);
        var resultWithLang = seoInfos.GetBestMatchingSeoInfo(store: store, language: "en-US");

        // Assert
        Assert.Equal("product1", result.SemanticUrl);
        Assert.Equal("de-DE", result.LanguageCode);

        Assert.Equal("product1", resultWithLang.SemanticUrl);
        Assert.Equal("en-US", resultWithLang.LanguageCode);
    }
}
