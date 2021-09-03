using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.NotificationsModule.Core.Model;
using VirtoCommerce.NotificationsModule.Core.Services;
using VirtoCommerce.NotificationsModule.Core.Types;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.StoreModule.Data.Services;
using Xunit;
using VirtoCommerce.NotificationsModule.Core.Extensions;
using System.Collections;
using System.Linq;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.StoreModule.Tests
{
    public class StoreNotificationSenderTests
    {
        private const string _tokenUrlEncoded = "tokenWithSlashes%2FandPlusSigns%2BandDoubleEquals%3D%3D";

        private readonly Mock<INotificationSearchService> _mockNotificationSearchService = new Mock<INotificationSearchService>();
        private readonly Mock<INotificationSender> _mocknotificationSender = new Mock<INotificationSender>();
        private readonly Mock<IStoreService> _mockStoreService = new Mock<IStoreService>();
        private readonly CustomUserManager _mockUserManager = GetTestUserManager();

        [Fact]
        public async Task EmailVerification_LinkURL_is_Encoded()
        {
            //Arrange
            var sender = GetStoreNotificationSender();
            var user = GetUser();

            //Act
            var link = await sender.GenerateEmailVerificationLink(user, GetStore());

            //Assert
            Assert.NotNull(link);
            Assert.Contains(_tokenUrlEncoded, link);
            Assert.Contains(StaticTokenProvider._token, HttpUtility.UrlDecode(link));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("non-url")]
        [InlineData("/account/confirm/local")]
        public Task EmailVerification_StoreURL_is_WellFormedURI(string storeUrl)
        {
            //Arrange
            var sender = GetStoreNotificationSender();
            var store = GetStore();
            store.Url = storeUrl;

            //Act
            var link = sender.GenerateEmailVerificationLink(null, store);

            //Assert
            return Assert.ThrowsAsync<OperationCanceledException>(async () => await link);
        }

        #region SendUserEmailVerificationAsync

        [Fact]
        public async Task SendUserEmailVerificationAsync_Successful()
        {
            //Arrange
            var sender = GetStoreNotificationSender();
            var user = GetUser();

            //Act
            var actual = await sender.SendUserEmailVerificationAsync(user);

            //Assert
            actual.Should().NotBeNull();
            actual.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task SendUserEmailVerificationAsync_StoreNotFound()
        {
            //Arrange
            var sender = GetStoreNotificationSender();
            var user = GetUser();
            _mockStoreService.As<ICrudService<Store>>()
                .Setup(ss => ss.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((Store)null);

            //Act
            var actual = await sender.SendUserEmailVerificationAsync(user);

            //Assert
            actual.Should().NotBeNull();
            actual.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task SendUserEmailVerificationAsync_VerificationDisabled()
        {
            //Arrange
            var sender = GetStoreNotificationSender();
            var user = GetUser();
            var store = GetStore();
            store.Settings = Array.Empty<ObjectSettingEntry>();
            _mockStoreService.As<ICrudService<Store>>()
                .Setup(ss => ss.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(store);

            //Act
            var actual = await sender.SendUserEmailVerificationAsync(user);

            //Assert
            actual.Should().NotBeNull();
            actual.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task SendUserEmailVerificationAsync_NotificationNotFound()
        {
            //Arrange
            var sender = GetStoreNotificationSender();
            var user = GetUser();
            _mockNotificationSearchService
                .Setup(ss => ss.SearchNotificationsAsync(It.IsAny<NotificationSearchCriteria>()))
                .ReturnsAsync(new NotificationSearchResult
                {
                    TotalCount = 0,
                    Results = Enumerable.Empty<Notification>().ToList()
                });

            //Act
            var actual = await sender.SendUserEmailVerificationAsync(user);

            //Assert
            actual.Should().NotBeNull();
            actual.IsSuccess.Should().BeFalse();
        }

        #endregion SendUserEmailVerificationAsync

        private ApplicationUser GetUser()
        {
            return new ApplicationUser
            {
                StoreId = "StoreId"
            };
        }

        private Store GetStore()
        {
            return new Store
            {
                Email = "",
                Settings = new[] { new ObjectSettingEntry { Name = ModuleConstants.Settings.General.EmailVerificationEnabled.Name, Value = true } },
                Url = "https://store.virtocommerce.com/"
            };
        }

        private IStoreNotificationSender GetStoreNotificationSender()
        {
            _mockNotificationSearchService
                .Setup(ss => ss.SearchNotificationsAsync(It.IsAny<NotificationSearchCriteria>()))
                .ReturnsAsync(new NotificationSearchResult { TotalCount = 1, Results = new[] { new ConfirmationEmailNotification { } } });

            _mocknotificationSender.Setup(ss => ss.SendNotificationAsync(It.IsAny<Notification>())).ReturnsAsync(new NotificationSendResult { IsSuccess = true });
            _mockStoreService.As<ICrudService<Store>>().Setup(ss => ss.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(GetStore());

            var result = new StoreNotificationSender(_mockNotificationSearchService.Object, _mocknotificationSender.Object, _mockStoreService.Object, () => _mockUserManager);
            return result;
        }

        // TODO: PT-274 Move mock of AspNetUserManager to the platform
        internal static CustomUserManager GetTestUserManager(Mock<IUserStore<ApplicationUser>> storeMock = null)
        {
            storeMock ??= new Mock<IUserStore<ApplicationUser>>();
            storeMock.As<IUserRoleStore<ApplicationUser>>()
                .Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>(), CancellationToken.None))
                .ReturnsAsync(Array.Empty<string>());
            storeMock.As<IUserLoginStore<ApplicationUser>>()
                .Setup(x => x.GetLoginsAsync(It.IsAny<ApplicationUser>(), CancellationToken.None))
                .ReturnsAsync(Array.Empty<UserLoginInfo>());
            storeMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>(), CancellationToken.None))
                .ReturnsAsync(IdentityResult.Success);

            var options = new Mock<IOptions<IdentityOptions>>();
            var idOptions = new IdentityOptions();
            idOptions.Lockout.AllowedForNewUsers = false;
            options.Setup(o => o.Value).Returns(idOptions);
            var userValidators = new List<IUserValidator<ApplicationUser>>();
            var validator = new Mock<IUserValidator<ApplicationUser>>();
            userValidators.Add(validator.Object);
            var pwdValidators = new List<PasswordValidator<ApplicationUser>>();
            pwdValidators.Add(new PasswordValidator<ApplicationUser>());
            var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
            passwordHasher.Setup(x => x.VerifyHashedPassword(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(PasswordVerificationResult.Success);

            var userManager = new CustomUserManager(storeMock.Object,
                Mock.Of<IOptions<IdentityOptions>>(),
                passwordHasher.Object,
                userValidators,
                pwdValidators,
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<UserManager<ApplicationUser>>>());

            validator.Setup(x => x.ValidateAsync(userManager, It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success).Verifiable();

            userManager.RegisterTokenProvider("Static", new StaticTokenProvider());
            userManager.Options.Tokens.EmailConfirmationTokenProvider = "Static";
            return userManager;
        }
    }

    // TODO: PT-274 Move mock of AspNetUserManager to the platform
    internal class CustomUserManager : AspNetUserManager<ApplicationUser>
    {
        public CustomUserManager(IUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators, IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }
    }

    internal class StaticTokenProvider : IUserTwoFactorTokenProvider<ApplicationUser>
    {
        internal const string _token = "tokenWithSlashes/andPlusSigns+andDoubleEquals==";

        public async Task<string> GenerateAsync(string purpose, UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            return MakeToken(purpose, await manager.GetUserIdAsync(user));
        }

        public async Task<bool> ValidateAsync(string purpose, string token, UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            return token == MakeToken(purpose, await manager.GetUserIdAsync(user));
        }

        public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            return Task.FromResult(true);
        }

        private static string MakeToken(string purpose, string userId)
        {
            return string.Join(":", userId, purpose, _token);
        }
    }
}
