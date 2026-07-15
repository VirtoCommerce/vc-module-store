using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using VirtoCommerce.StoreModule.Core;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Data.Services;
using Xunit;

namespace VirtoCommerce.StoreModule.Tests
{
    public class StoreAssetPublicUrlResolverTests
    {
        private static StoreAssetPublicUrlResolver CreateResolver(string[] knownAssetHosts = null)
        {
            var options = Options.Create(new StoreAssetsOptions { KnownAssetHosts = knownAssetHosts ?? [] });
            return new StoreAssetPublicUrlResolver(options, NullLogger<StoreAssetPublicUrlResolver>.Instance);
        }

        private static Store StoreWith(string assetPublicUrl) => new() { AssetPublicUrl = assetPublicUrl };

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GetAbsoluteUrl_NullOrEmpty_ReturnsAsIs(string url)
        {
            //Arrange
            var resolver = CreateResolver();

            //Act
            var result = resolver.GetAbsoluteUrl(StoreWith("https://cdn.store1.com"), url);

            //Assert
            result.Should().Be(url);
        }

        [Theory]
        [InlineData("data:image/png;base64,iVBORw0KGgo=")]
        [InlineData("blob:https://x/9b2c")]
        public void GetAbsoluteUrl_DataOrBlobScheme_ReturnsAsIs(string url)
        {
            //Arrange
            var resolver = CreateResolver();

            //Act
            var result = resolver.GetAbsoluteUrl(StoreWith("https://cdn.store1.com"), url);

            //Assert
            result.Should().Be(url);
        }

        [Theory]
        [InlineData("catalog/x.jpg")]
        [InlineData("https://global.example.com/assets/catalog/x.jpg")]
        public void GetAbsoluteUrl_NoStoreAssetUrl_ReturnsAsIs(string url)
        {
            //Arrange
            var resolver = CreateResolver();

            //Act
            var result = resolver.GetAbsoluteUrl(StoreWith(null), url);

            //Assert
            result.Should().Be(url);
        }

        [Fact]
        public void GetAbsoluteUrl_NullStore_ThrowsArgumentNullException()
        {
            //Arrange
            var resolver = CreateResolver();

            //Act
            var action = () => resolver.GetAbsoluteUrl(null, "catalog/x.jpg");

            //Assert
            action.Should().Throw<System.ArgumentNullException>();
        }

        [Theory]
        [InlineData("catalog/x.jpg")]
        [InlineData("/catalog/x.jpg")]
        public void GetAbsoluteUrl_RelativeUrl_CombinesWithStoreAssetUrl(string url)
        {
            //Arrange
            var resolver = CreateResolver();

            //Act
            var result = resolver.GetAbsoluteUrl(StoreWith("https://cdn.store1.com"), url);

            //Assert
            result.Should().Be("https://cdn.store1.com/catalog/x.jpg");
        }

        [Fact]
        public void GetAbsoluteUrl_RelativeUrl_TrailingSlashBase_CombinesWithoutDoubleSlash()
        {
            //Arrange
            var resolver = CreateResolver();

            //Act
            var result = resolver.GetAbsoluteUrl(StoreWith("https://cdn.store1.com/assets/"), "catalog/x.jpg");

            //Assert
            result.Should().Be("https://cdn.store1.com/assets/catalog/x.jpg");
        }

        [Fact]
        public void GetAbsoluteUrl_AbsoluteUrl_NoKnownHosts_RebasesAnyHost()
        {
            //Arrange
            var resolver = CreateResolver();

            //Act
            var result = resolver.GetAbsoluteUrl(StoreWith("https://cdn.store1.com"), "https://global.example.com/assets/catalog/x.jpg");

            //Assert
            result.Should().Be("https://cdn.store1.com/assets/catalog/x.jpg");
        }

        [Fact]
        public void GetAbsoluteUrl_KnownHostsSet_ListedHost_IsRebased()
        {
            //Arrange
            var resolver = CreateResolver(knownAssetHosts: ["global.example.com"]);

            //Act
            var result = resolver.GetAbsoluteUrl(StoreWith("https://cdn.store1.com"), "https://global.example.com/assets/catalog/x.jpg");

            //Assert
            result.Should().Be("https://cdn.store1.com/assets/catalog/x.jpg");
        }

        [Fact]
        public void GetAbsoluteUrl_KnownHostsSet_UnlistedHost_ReturnsAsIs()
        {
            //Arrange
            var resolver = CreateResolver(knownAssetHosts: ["global.example.com"]);
            const string external = "https://external.com/img.jpg";

            //Act
            var result = resolver.GetAbsoluteUrl(StoreWith("https://cdn.store1.com"), external);

            //Assert
            result.Should().Be(external);
        }

        [Fact]
        public void GetAbsoluteUrl_KnownHostsSet_AcceptsFullUrlEntries_CaseInsensitive()
        {
            //Arrange
            var resolver = CreateResolver(knownAssetHosts: ["https://Global.Example.com/assets"]);

            //Act
            var result = resolver.GetAbsoluteUrl(StoreWith("https://cdn.store1.com"), "https://global.example.com/assets/catalog/x.jpg");

            //Assert
            result.Should().Be("https://cdn.store1.com/assets/catalog/x.jpg");
        }

        [Fact]
        public void GetAbsoluteUrl_InvalidStoreAssetUrl_AbsoluteUrl_ReturnsOriginal()
        {
            //Arrange
            var resolver = CreateResolver();
            const string original = "https://global.example.com/assets/x.jpg";

            //Act
            var result = resolver.GetAbsoluteUrl(StoreWith("ht tp://not a valid base"), original);

            //Assert
            result.Should().Be(original);
        }

        [Fact]
        public void GetAbsoluteUrl_KnownHostsSet_InvalidEntryIsIgnored_ValidEntryStillGates()
        {
            //Arrange
            var resolver = CreateResolver(knownAssetHosts: ["not a valid host", "global.example.com"]);

            //Act
            var rebased = resolver.GetAbsoluteUrl(StoreWith("https://cdn.store1.com"), "https://global.example.com/assets/x.jpg");
            var external = resolver.GetAbsoluteUrl(StoreWith("https://cdn.store1.com"), "https://external.com/x.jpg");

            //Assert
            rebased.Should().Be("https://cdn.store1.com/assets/x.jpg");
            external.Should().Be("https://external.com/x.jpg");
        }

        [Fact]
        public void GetAbsoluteUrl_KnownHostsSet_RelativeUrl_IsStillCombined()
        {
            //Arrange
            var resolver = CreateResolver(knownAssetHosts: ["global.example.com"]);

            //Act
            var result = resolver.GetAbsoluteUrl(StoreWith("https://cdn.store1.com"), "catalog/x.jpg");

            //Assert
            result.Should().Be("https://cdn.store1.com/catalog/x.jpg");
        }

        [Fact]
        public void GetAbsoluteUrl_BareHostStoreAssetUrl_NormalizesToHttps()
        {
            //Arrange
            var resolver = CreateResolver();

            //Act
            var result = resolver.GetAbsoluteUrl(StoreWith("cdn.store1.com"), "https://global.example.com/assets/catalog/x.jpg");

            //Assert
            result.Should().Be("https://cdn.store1.com/assets/catalog/x.jpg");
        }

        [Fact]
        public void GetAbsoluteUrl_StoreAssetUrlWithBasePath_PrependsPathOnRebase()
        {
            //Arrange
            var resolver = CreateResolver();

            //Act
            var result = resolver.GetAbsoluteUrl(StoreWith("https://cdn.com/tenant1"), "https://global.example.com/assets/catalog/x.jpg");

            //Assert
            result.Should().Be("https://cdn.com/tenant1/assets/catalog/x.jpg");
        }

        [Fact]
        public void GetAbsoluteUrl_AbsoluteUrlWithPort_RebasesToStoreDefaultPort()
        {
            //Arrange
            var resolver = CreateResolver();

            //Act
            var result = resolver.GetAbsoluteUrl(StoreWith("https://cdn.store1.com"), "http://localhost:10645/assets/catalog/x.jpg");

            //Assert
            result.Should().Be("https://cdn.store1.com/assets/catalog/x.jpg");
        }

        [Theory]
        [InlineData("https://global.example.com/assets/x.jpg?sas=abc#frag", "https://cdn.store1.com/assets/x.jpg?sas=abc#frag")]
        [InlineData("catalog/x.jpg?sas=abc#frag", "https://cdn.store1.com/catalog/x.jpg?sas=abc#frag")]
        public void GetAbsoluteUrl_PreservesQueryAndFragment(string url, string expected)
        {
            //Arrange
            var resolver = CreateResolver();

            //Act
            var result = resolver.GetAbsoluteUrl(StoreWith("https://cdn.store1.com"), url);

            //Assert
            result.Should().Be(expected);
        }
    }
}
