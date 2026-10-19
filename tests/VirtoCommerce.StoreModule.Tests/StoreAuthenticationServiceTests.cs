using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.ExternalSignIn;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.Data.Services;
using Xunit;

namespace VirtoCommerce.StoreModule.Tests;

public class StoreAuthenticationServiceTests
{
    private const string StoreId = "Store";
    private const string PasswordType = "Password";
    private const string ExternalType = "Google";
    private const string ProviderType = "OTP";

    private readonly Mock<IStoreAuthenticationSchemeService> _crudServiceMock = new();
    private readonly Mock<IStoreAuthenticationSchemeSearchService> _searchServiceMock = new();
    private readonly Mock<IStoreAuthenticationTypeProvider> _providerMock = new();
    private readonly List<StoreAuthenticationScheme> _storedSchemes = [];

    public StoreAuthenticationServiceTests()
    {
        _providerMock.SetupGet(x => x.Name).Returns(ProviderType);
        _providerMock.SetupGet(x => x.DisplayName).Returns(ProviderType);

        _searchServiceMock
            .Setup(x => x.SearchAsync(It.IsAny<StoreAuthenticationSchemeSearchCriteria>(), It.IsAny<bool>()))
            .ReturnsAsync(() => new StoreAuthenticationSchemeSearchResult
            {
                Results = _storedSchemes.ToList(),
                TotalCount = _storedSchemes.Count,
            });
    }

    [Fact]
    public async Task GetStoreSchemesAsync_DoesNotReturnProviderTypes()
    {
        // Arrange
        var service = GetService();

        // Act
        var result = await service.GetStoreSchemesAsync(StoreId);

        // Assert
        result.Select(x => x.Name).Should().Equal(PasswordType, ExternalType);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetStoreAuthenticationTypesAsync_ReturnsProviderTypeAfterPassword(bool isActive)
    {
        // Arrange
        _providerMock.Setup(x => x.IsActiveAsync(StoreId)).ReturnsAsync(isActive);
        var service = GetService();

        // Act
        var result = await service.GetStoreAuthenticationTypesAsync(StoreId);

        // Assert
        result.Select(x => x.Name).Should().Equal(PasswordType, ProviderType, ExternalType);

        var providerType = result.Single(x => x.Name == ProviderType);
        providerType.DisplayName.Should().Be(ProviderType);
        providerType.StoreId.Should().Be(StoreId);
        providerType.IsActive.Should().Be(isActive);
    }

    [Fact]
    public async Task GetStoreAuthenticationTypesAsync_PasswordDisabled_ReturnsProviderTypeFirst()
    {
        // Arrange
        var service = GetService(passwordEnabled: false);

        // Act
        var result = await service.GetStoreAuthenticationTypesAsync(StoreId);

        // Assert
        result.Select(x => x.Name).Should().Equal(ProviderType, ExternalType);
    }

    [Fact]
    public async Task GetStoreAuthenticationTypesAsync_NoProviders_ReturnsSchemesOnly()
    {
        // Arrange
        var service = GetService(withProvider: false);

        // Act
        var result = await service.GetStoreAuthenticationTypesAsync(StoreId);

        // Assert
        result.Select(x => x.Name).Should().Equal(PasswordType, ExternalType);
    }

    [Fact]
    public async Task SaveStoreSchemesAsync_SavesProviderTypeThroughProvider()
    {
        // Arrange
        _providerMock.Setup(x => x.IsActiveAsync(StoreId)).ReturnsAsync(false);
        var service = GetService();
        var models = await service.GetStoreAuthenticationTypesAsync(StoreId);
        models.Single(x => x.Name == ProviderType).IsActive = true;

        // Act
        await service.SaveStoreSchemesAsync(StoreId, models);

        // Assert
        _providerMock.Verify(x => x.SetIsActiveAsync(StoreId, true), Times.Once);
        _crudServiceMock.Verify(x => x.SaveChangesAsync(It.Is<IList<StoreAuthenticationScheme>>(schemes =>
            schemes.Select(scheme => scheme.Name).SequenceEqual(new[] { PasswordType, ExternalType }))), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SaveStoreSchemesAsync_ProviderTypeNotChanged_DoesNotSaveIt(bool isActive)
    {
        // Arrange
        _providerMock.Setup(x => x.IsActiveAsync(StoreId)).ReturnsAsync(isActive);
        var service = GetService();
        var models = await service.GetStoreAuthenticationTypesAsync(StoreId);

        // Act
        await service.SaveStoreSchemesAsync(StoreId, models);

        // Assert
        _providerMock.Verify(x => x.SetIsActiveAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    private StoreAuthenticationService GetService(bool passwordEnabled = true, bool withProvider = true)
    {
        var passwordOptions = Options.Create(new PasswordLoginOptions { Enabled = passwordEnabled, AuthenticationType = PasswordType });
        var providers = withProvider ? new[] { _providerMock.Object } : [];

        return new StoreAuthenticationService(
            _crudServiceMock.Object,
            _searchServiceMock.Object,
            passwordOptions,
            () => GetSignInManager().Object,
            Array.Empty<ExternalSignInProviderConfiguration>(),
            providers);
    }

    private static Mock<SignInManager<ApplicationUser>> GetSignInManager()
    {
        var userManager = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

        var signInManager = new Mock<SignInManager<ApplicationUser>>(
            userManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<ILogger<SignInManager<ApplicationUser>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<ApplicationUser>>());

        signInManager
            .Setup(x => x.GetExternalAuthenticationSchemesAsync())
            .ReturnsAsync([new AuthenticationScheme(ExternalType, ExternalType, typeof(IAuthenticationHandler))]);

        return signInManager;
    }
}
