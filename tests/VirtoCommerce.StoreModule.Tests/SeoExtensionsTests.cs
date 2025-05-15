using System.Collections.Generic;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.StoreModule.Core.Extensions;
using VirtoCommerce.StoreModule.Core.Model;
using Xunit;

namespace VirtoCommerce.StoreModule.Tests
{
    public class SeoExtensionsTests
    {
        [Fact]
        public void GetBestMatchingSeoInfo_WithNullParameters_ReturnsNull()
        {
            // Arrange
            var seoInfos = new List<SeoInfo>
            {
                new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1" },
                new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product2" },
                new() { StoreId = "Store2", LanguageCode = "en-US", SemanticUrl = "product1" },
                new() { StoreId = "Store2", LanguageCode = "en-US", SemanticUrl = "product2" },
            };

            // Act
            var result = seoInfos.GetBestMatchingSeoInfo(store: null, language: null, slug: null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetBestMatchingSeoInfo_WithValidParameters_ReturnsSeoInfo()
        {
            // Arrange
            var store = new Store { Id = "Store1", DefaultLanguage = "en-US", };

            var seoInfos = new List<SeoInfo>
            {
                new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1" },
                new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product2" },
                new() { StoreId = "Store2", LanguageCode = "en-US", SemanticUrl = "product1" },
                new() { StoreId = "Store2", LanguageCode = "en-US", SemanticUrl = "product2" },
            };

            // Act
            var result = seoInfos.GetBestMatchingSeoInfo(store, language: "en-US", slug: "product1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Store1", result.StoreId);
            Assert.Equal("en-US", result.LanguageCode);
            Assert.Equal("product1", result.SemanticUrl);
        }

        [Fact]
        public void GetBestMatchingSeoInfo_WithUnknownLanguage_ReturnsSeoInfoWithDefaultStoreLanguage()
        {
            // Arrange
            var store = new Store { Id = "Store1", DefaultLanguage = "en-US", };

            var seoInfos = new List<SeoInfo>
            {
                new() { StoreId = "Store1", LanguageCode = null, SemanticUrl = "product1" },
                new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1" },
                new() { StoreId = "Store1", LanguageCode = "fr-FR", SemanticUrl = "product1" },
            };

            // Act
            var result = seoInfos.GetBestMatchingSeoInfo(store, language: "de-DE", slug: "product1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(result.LanguageCode, store.DefaultLanguage);
        }

        [Fact]
        public void GetBestMatchingSeoInfo_WithUnknownLanguage_ReturnsSeoInfoWithEmptyLanguage()
        {
            // Arrange
            var store = new Store { Id = "Store1", DefaultLanguage = "en-US", };

            var seoInfos = new List<SeoInfo>
            {
                new() { StoreId = "Store1", LanguageCode = "fr-FR", SemanticUrl = "product1" },
                new() { StoreId = "Store1", LanguageCode = null, SemanticUrl = "product1" },
            };

            // Act
            var result = seoInfos.GetBestMatchingSeoInfo(store, language: "de-DE", slug: "product1");

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.LanguageCode);
        }

        [Fact]
        public void GetBestMatchingSeoInfo_WithObjectType_ReturnsSeoInfoWithHighPriority()
        {
            // Arrange
            var store = new Store { Id = "Store1", DefaultLanguage = "en-US", };

            var seoInfos = new List<SeoInfo>
            {
                new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1", ObjectType = "Category"},
                new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1", ObjectType = "Pages"},
            };

            // Act
            var result = seoInfos.GetBestMatchingSeoInfo(store, language: "en-US", slug: "product1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Pages", result.ObjectType);
        }

        [Fact]
        public void GetBestMatchingSeoInfo_NonEqualPermalink_ReturnsNull()
        {
            // Arrange
            var store = new Store { Id = "Store1", DefaultLanguage = "en-US", };

            var seoInfos = new List<SeoInfo>
            {
                new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1", ObjectType = "Category"},
                new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1", ObjectType = "Pages"},
            };

            // Act
            var result = seoInfos.GetBestMatchingSeoInfo(store, language: "en-US", permalink: "product2");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetBestMatchingSeoInfo_NonEqualSlug_ReturnsNull()
        {
            // Arrange
            var store = new Store { Id = "Store1", DefaultLanguage = "en-US", };

            var seoInfos = new List<SeoInfo>
            {
                new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1", ObjectType = "Category"},
                new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1", ObjectType = "Pages"},
            };

            // Act
            var result = seoInfos.GetBestMatchingSeoInfo(store, language: "en-US", slug: "product2");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetBestMatchingSeoInfo_NonEqualStore_ReturnsNull()
        {
            // Arrange
            var store = new Store { Id = "Store2", DefaultLanguage = "en-US", };

            var seoInfos = new List<SeoInfo>
            {
                new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1", ObjectType = "Category"},
                new() { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1", ObjectType = "Pages"},
            };

            // Act
            var result = seoInfos.GetBestMatchingSeoInfo(store, language: "en-US", slug: "product1");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetBestMatchingSeoInfo_NonEqualLanguage_ReturnsNull()
        {
            // Arrange
            var store = new Store { Id = "Store1", DefaultLanguage = "en-US", };

            var seoInfos = new List<SeoInfo>
            {
                new() { StoreId = "Store1", LanguageCode = "de-DE", SemanticUrl = "product1", ObjectType = "Category"},
                new() { StoreId = "Store1", LanguageCode = "de-DE", SemanticUrl = "product1", ObjectType = "Pages"},
            };

            // Act
            var result = seoInfos.GetBestMatchingSeoInfo(store, language: "en-US", slug: "product1");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetBestMatchingSeoInfo_WithEmptyStore_ReturnsValue()
        {
            // Arrange
            var store = new Store { Id = "Store2", DefaultLanguage = "en-US", };

            var seoInfos = new List<SeoInfo>
            {
                new() { StoreId = null, LanguageCode = "en-US", SemanticUrl = "product1", ObjectType = "Category"},
                new() { StoreId = null, LanguageCode = "en-US", SemanticUrl = "product1", ObjectType = "Pages"},
            };

            // Act
            var result = seoInfos.GetBestMatchingSeoInfo(store, language: "en-US", slug: "product1");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetBestMatchingSeoInfo_WithCorrectParameters_ReturnsActive()
        {
            // Arrange
            var store = new Store { Id = "Store2", DefaultLanguage = "en-US", };

            var seoInfos = new List<SeoInfo>
            {
                new() { StoreId = null, LanguageCode = "en-US", SemanticUrl = "product1", IsActive = false },
                new() { StoreId = null, LanguageCode = "en-US", SemanticUrl = "product1"},
            };

            // Act
            var result = seoInfos.GetBestMatchingSeoInfo(store, language: "en-US", slug: "product1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsActive);
        }

        [Fact]
        public void GetBestMatchingSeoInfo_WithInactiveAndUrl_ReturnsInactive()
        {
            // Arrange
            var store = new Store { Id = "Store2", DefaultLanguage = "en-US", };

            var seoInfos = new List<SeoInfo>
            {
                new() { StoreId = null, LanguageCode = "en-US", SemanticUrl = "product2", IsActive = false },
                new() { StoreId = null, LanguageCode = "en-US", SemanticUrl = "product1"},
            };

            // Act
            var result = seoInfos.GetBestMatchingSeoInfo(store, language: "en-US", slug: "product2");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsActive);
        }

        [Fact]
        public void GetBestMatchingSeoInfo_SemanticUrl_IsHigherStore()
        {
            // Arrange
            var store = new Store { Id = "Store2", DefaultLanguage = "en-US", };

            var seoInfos = new List<SeoInfo>
            {
                new() { StoreId = "Store2", LanguageCode = "en-US", SemanticUrl = "category/product2"},
                new() { StoreId = null, LanguageCode = "en-US", SemanticUrl = "category/product2"},
            };

            // Act
            var result = seoInfos.GetBestMatchingSeoInfo(store, language: null, permalink: "category/product2", slug: "product2");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Store2", result.StoreId);
        }
    }
}
